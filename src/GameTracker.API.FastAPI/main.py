import argparse
from contextlib import asynccontextmanager
from pathlib import Path
import shutil

import uvicorn
from fastapi import (
    FastAPI, 
    Depends, 
    HTTPException,
    File,
    Form,
    UploadFile
)
from sqlalchemy import create_engine, text
from sqlalchemy.orm import Session, sessionmaker
from enum import Enum

class GameCategory(str, Enum):
    board_game = "Board"
    card_game = "Card"
    video_game = "Video"
    other = "Other"

ALLOWED_IMAGE_EXTENSIONS = {
    "image/jpeg": ".jpg",
    "image/png": ".png",
    "image/gif": ".gif",
    "image/bmp": ".bmp",
    "image/tiff": ".tiff",
    "image/webp": ".webp"
}
# ------------------------------------------------------------------
# Database globals
# ------------------------------------------------------------------

engine = None
SessionLocal = None
database_path = None
image_root = None

# ------------------------------------------------------------------
# Database globals
# ------------------------------------------------------------------

engine = None
SessionLocal = None
database_path = None
image_root = None

def configure_database(path: str):
    global engine
    global SessionLocal
    global database_path
    global image_root

    database_path = Path(path).resolve()

    image_root = database_path.parent / "images"

    image_root.mkdir(parents=True, exist_ok=True)

    database_url = f"sqlite:///{database_path}"

    engine = create_engine(database_url)

    SessionLocal = sessionmaker(
        bind=engine,
        autoflush=False,
        autocommit=False,
    )

# ------------------------------------------------------------------
# Helper functions
# ------------------------------------------------------------------

def get_image_extension(filename: str) -> str:
    if filename.content_type not in ALLOWED_IMAGE_EXTENSIONS:
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported image type: {filename.content_type}"
        )
    
    return ALLOWED_IMAGE_EXTENSIONS[filename.content_type]

def save_image(
        upload: UploadFile,
        game_id: int,
        image_name: str
) -> str:
    extension = get_image_extension(upload)

    directory = image_root / str(game_id)

    directory.mkdir(parents=True, exist_ok=True)

    filename = f"{image_name}{extension}"
    destination = directory / filename

    with destination.open("wb") as output:
        shutil.copyfileobj(
            upload.file,
            output
        )
    
    relative_path = Path("images") / str(game_id) / filename
    
    return str(relative_path.as_posix())

# ------------------------------------------------------------------
# FastAPI lifecycle events
# ------------------------------------------------------------------

@asynccontextmanager
async def lifespan(app: FastAPI):
    print("API starting up...")
    #print(f"Database path: {engine.url if engine is not None else 'Not configured'}")
    #print(f"API host: {app.host}:{app.port}")

    if engine is not None:
        with engine.connect() as connection:
            connection.execute(text("SELECT 1"))

        print("Database connection established.")
    
    yield

    print ("API shutting down...")

    if engine is not None:
        engine.dispose()
        print("Database connection closed.")

app = FastAPI(
    title="Games Database API", 
    lifespan=lifespan
)

# ------------------------------------------------------------------
# Database dependency
# ------------------------------------------------------------------

def get_db():
    if SessionLocal is None:
        raise RuntimeError("Database has not been configured.")
    
    db = SessionLocal()

    try:
        yield db
    finally:
        db.close()

# ------------------------------------------------------------------
# Routes
# ------------------------------------------------------------------

@app.get("/")
def root():
    return {
        "status": "running"
    }

@app.get("/tables")
def get_tables(db: Session = Depends(get_db)):
    try:
        result = db.execute(
            text("""
                 SELECT name
                 FROM sqlite_master
                 WHERE type = 'table'
                 ORDER BY name
            """)
        )

        tables = [row[0] for row in result]

        return {
            "table": tables
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error fetching tables: {e}")

@app.get("/games")
def get_games(db: Session = Depends(get_db)):
    try:
        result = db.execute(
            text("""
                 SELECT *
                 FROM games
                 ORDER BY title
            """)
        )

        return [
            dict(row._mapping)
            for row in result
        ]
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error fetching games: {e}")

@app.post("/games")
def create_game(
    title: str = Form(...),
    category: GameCategory = Form(...),
    min_players: int | None = Form(..., alias="minPlayers"),
    max_players: int | None= Form(..., alias="maxPlayers"),
    release_date: str | None = Form(..., alias="releaseDate"),
    notes: str | None= Form(...),
    played: bool = Form(...),
    completed: bool = Form(...),
    icon_path: UploadFile | None= File(..., alias="iconPath"),
    cover_image_path: UploadFile | None = File(..., alias="coverImagePath"),
    physical_copy: bool = Form(..., alias="physicalCopy"),
    purchased_from: str | None = Form(..., alias="purchasedFrom"),

    db: Session = Depends(get_db)
):
    game_directory: Path | None = None

    try:
        # ---------------------------------------------
        # Insert the record first
        # ---------------------------------------------

        print("Inserting game record into database...")

        result = db.execute(
            text("""
                 INSERT INTO games(
                    Title,
                    Category,
                    MinPlayers,
                    MaxPlayers,
                    ReleaseDate,
                    Notes,
                    Played,
                    Completed,
                    PhysicalCopy,
                    PurchasedFrom
                 )
                 VALUES(
                    :title,
                    :category,
                    :min_players,
                    :max_players,
                    :release_date,
                    :notes,
                    :played,
                    :completed,
                    :physical_copy,
                    :purchased_from
                 )
            """),
            {
                "title": title,
                "category": category,
                "min_players": min_players,
                "max_players": max_players,
                "release_date": release_date,
                "notes": notes,
                "played": played,
                "completed": completed,
                "physical_copy": physical_copy,
                "purchased_from": purchased_from
            }
        )

        game_id = result.lastrowid

        print(f"Game record inserted with ID: {game_id}")

        # ---------------------------------------------
        # Create the image directory
        # ---------------------------------------------

        game_directory = image_root / str(game_id)

        game_directory.mkdir(parents=True, exist_ok=True)

        print(f"Image directory created: {game_directory}")
        # ---------------------------------------------
        # Save cover
        # ---------------------------------------------

        cover_path = None

        if cover_image_path:
            cover_path = save_image(cover_image_path, game_id, "cover")
            print(f"Cover image saved at: {cover_path}")
            
        # ---------------------------------------------
        # Save background
        # ---------------------------------------------

        icon = None

        if icon_path:
            icon = save_image(icon_path, game_id, "icon")
            print(f"Icon image saved at: {icon}")
        
        # ---------------------------------------------
        # Update database with image paths
        # ---------------------------------------------

        print("Updating game record with image paths...")

        db.execute(
            text("""
                 UPDATE games
                 SET
                    CoverImagePath = :cover_path,
                    IconPath = :icon_path
                 WHERE ID = :game_id
            """),
            {
                "game_id": game_id,
                "cover_path": str(cover_path) if cover_path else None,
                "icon_path": str(icon) if icon else None
            }
        )

        db.commit()

        # ---------------------------------------------
        # Return created record
        # ---------------------------------------------

        print (f"Game record updated with image paths for ID: {game_id}")

        return {
            "id": game_id,
            "title": title,
            "category": category,
            "minPlayers": min_players,
            "maxPlayers": max_players,
            "releaseDate": release_date,
            "notes": notes,
            "played": played,
            "completed": completed,
            "coverImagePath": str(cover_path) if cover_path else None,
            "iconPath": str(icon) if icon else None,
            "physicalCopy": physical_copy,
            "purchasedFrom": purchased_from
        }
    
    except Exception as e:
        db.rollback()

        # Remove files if the operation failed
        if game_directory and game_directory.exists():
            shutil.rmtree(
                game_directory,
                ignore_errors=True
            )
        raise HTTPException(status_code=500, detail=f"Error creating game: {e}")

# ------------------------------------------------------------------
# Program entry point
# ------------------------------------------------------------------

if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Games Database API"
    )

    parser.add_argument(
        "--database",
        "-d",
        required=True,
        help="Path to the SQLite database file."
    )

    parser.add_argument(
        "--host",
        default="127.0.0.1"
    )

    parser.add_argument(
        "--port",
        type=int,
        default=8000
    )

    args = parser.parse_args()

    configure_database(args.database)

    print(f"Starting API at http://{args.host}:{args.port}")
    print(f"Swagger UI:      http://{args.host}:{args.port}/docs")

    uvicorn.run(
        app, 
        host=args.host,
        port=args.port
    )