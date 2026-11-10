using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerUser
{
    public class EventModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Reporter { get; set; }
        public string Description { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public bool IsFavorite { get; set; }
    }
}
