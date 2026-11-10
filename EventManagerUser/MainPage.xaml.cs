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

            _allEvents = rawEvents
             .GroupBy(e => e.Name?.Trim().ToLower())
             .Select(g => g.First())
             .ToList();
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

        //Handling the 'Favorite' button click event
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
    }
}