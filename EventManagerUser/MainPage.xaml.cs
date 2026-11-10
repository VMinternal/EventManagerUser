namespace EventManagerUser
{
    public partial class MainPage : ContentPage
    {
        private readonly FirebaseService _firebaseService;
        private List<EventModel> _allEvents = new();

        public MainPage()
        {
            InitializeComponent();
            _firebaseService = new FirebaseService();
            LoadEventsData();
        }

        // Load the list of events from Firebase when the application starts
        private async void LoadEventsData()
        {
            var rawEvents = await _firebaseService.GetEventsAsync();

            // Filter out events with duplicate names beforehand to optimize the number of API calls.
            var uniqueEvents = rawEvents
                .GroupBy(e => e.Name?.Trim().ToLower())
                .Select(g => g.First())
                .ToList();

            // Iterate through each unique event to retrieve the corresponding list of sessions via the eventId.
            foreach (var ev in uniqueEvents)
            {
                ev.Sessions = await _firebaseService.GetSessionsByEventIdAsync(ev.Id);
            }

            // Bind the complete list to the UI
            _allEvents = uniqueEvents;
            EventsCollection.ItemsSource = _allEvents;
        }

        // Search function by Name or Event Date
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = e.NewTextValue?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                EventsCollection.ItemsSource = _allEvents;
            }
            else
            {
                var filteredList = _allEvents.Where(ev =>
                    (ev.Name != null && ev.Name.ToLower().Contains(keyword)) ||
                    (ev.Date != null && ev.Date.ToLower().Contains(keyword))
                ).ToList();

                EventsCollection.ItemsSource = filteredList;
            }
        }

        // Handling the 'Favorite' button click event
        private void OnFavoriteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string eventId)
            {
                var eventItem = _allEvents.FirstOrDefault(x => x.Id == eventId);
                if (eventItem != null)
                {
                    eventItem.IsFavorite = !eventItem.IsFavorite;
                    // Change the heart icon when selected or deselected
                    button.Text = eventItem.IsFavorite ? "❤️" : "🤍";
                }
            }
        }

        // Handle the button-click event to open the details page & view a separate list of sessions
        private async void OnViewSessionsClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is EventModel selectedEvent)
            {
                // Navigate to the EventDetailPage and pass the selected event object to it.
                await Navigation.PushAsync(new EventDetailPage(selectedEvent));
            }
        }
    }
}