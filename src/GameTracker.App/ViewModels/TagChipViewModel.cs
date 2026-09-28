using System;
using System.Collections.Generic;
using System.Text;

namespace GameTracker.App.ViewModels
{
    public class TagChipViewModel
    {
        public int Id { get; }
        public string Name { get; }

        public TagChipViewModel(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
