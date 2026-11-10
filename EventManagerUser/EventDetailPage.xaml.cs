using Microsoft.Maui.Controls;

namespace EventManagerUser
{
    public partial class EventDetailPage : ContentPage
    {
        private readonly FirebaseService _firebaseService;
        private readonly EventModel _eventItem;

        public EventDetailPage(EventModel eventItem)
        {
            InitializeComponent();
            _firebaseService = new FirebaseService();
            _eventItem = eventItem;

            // Populate labels with event data
            BindEventData();

            // Retrieve the list of sessions from Firebase based on the ID of this event
            LoadSessionsData();
        }

        private void BindEventData()
        {
            if (_eventItem != null)
            {
                LblEventName.Text = _eventItem.Name;
                LblDate.Text = $"📅 Date: {_eventItem.Date}";
                LblLocation.Text = $"📍 Location: {_eventItem.Location}";
                LblDescription.Text = _eventItem.Description;
            }
        }

        private async void LoadSessionsData()
        {
            if (_eventItem != null)
            {
                // Call the service to retrieve the list of sessions from Firebase
                var sessions = await _firebaseService.GetSessionsByEventIdAsync(_eventItem.Id);

                // REMOVE DUPLICATE SESSIONS (Based on Session Name and Start Time)
                var uniqueSessions = sessions
                    .GroupBy(s => new {
                        Name = s.SessionName?.Trim().ToLower(),
                        StartTime = s.StartTime?.Trim()
                    })
                    .Select(g => g.First())
                    .ToList();

                // Assign the unique filtered list to the interface
                SessionsCollection.ItemsSource = uniqueSessions;
            }
        }
    }
}