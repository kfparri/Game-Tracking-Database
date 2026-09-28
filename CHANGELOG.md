# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## Added

- Added tags functionality
- Created settings window
- Added the following settings
  - Backup - backup the database file
  - Restore - restore a backup file
  - Export - export game database to text file or csv
  - Reset - delete the current database and create a new one, essentially a full reset of the application.
- Added omnibar on the game list page to filter games.  This filter currently looks at title, platform, publisher and tags
- You can now unselect a game to minimize the detail view
- Added "Import from Steam" functionality
- Added "Import from GOG" functionality

## Changed

- Converted ViewModels to only use services instead of dbContext directly

## [1.0.0] - 2026-08-30

### Added

- Initial Release
- Class libraries for models, services and data context
- WPF application with basic CRUD functionality with local SQLite database
- Simple Python FastAPI app that will also connect to the SQLite database to allow mass data updates
