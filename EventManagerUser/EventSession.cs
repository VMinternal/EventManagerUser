using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerUser
{
    public class EventSession
    {
        public string Id { get; set; }
        public string EventId { get; set; }// Foreign key linked to Event
        public string SessionName { get; set; }
        public string Speaker { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Description { get; set; }
    }
}
