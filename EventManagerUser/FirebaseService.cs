using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventManagerUser
{
    public class FirebaseService
    {
        private readonly HttpClient _httpClient;
        private const string RealtimeDbUrl =
            "https://dbtest-7a484-default-rtdb.asia-southeast1.firebasedatabase.app";

        public FirebaseService()
        {
            _httpClient = new HttpClient();
        }

        // Retrieve a list of events from Firebase Realtime Database
        public async Task<List<EventModel>> GetEventsAsync()
        {
            var eventList = new List<EventModel>();
            string url = $"{RealtimeDbUrl}/events.json";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "null")
                    {
                        return eventList;
                    }

                    using var doc = JsonDocument.Parse(jsonString);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            if (prop.Value.ValueKind != JsonValueKind.Object)
                                continue;

                            var item = prop.Value;
                            var eventModel = new EventModel();

                            string idFromField = GetFlexibleString(item, "id");
                            eventModel.Id = !string.IsNullOrEmpty(idFromField) ? idFromField : prop.Name;

                            eventModel.Name = GetFlexibleString(item, "name");
                            eventModel.Location = GetFlexibleString(item, "location");
                            eventModel.Date = GetFlexibleString(item, "date");
                            eventModel.Time = GetFlexibleString(item, "time");
                            eventModel.Reporter = GetFlexibleString(item, "reporter");
                            eventModel.Description = GetFlexibleString(item, "description");
                            eventModel.ContactEmail = GetFlexibleString(item, "contactEmail");
                            eventModel.ContactPhone = GetFlexibleString(item, "contactPhone");

                            eventList.Add(eventModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Realtime DB GetEventsAsync error: {ex.Message}");
            }

            return eventList;
        }

        // Retrieve sessions by Event ID from Firebase Realtime Database
        public async Task<List<EventSession>> GetSessionsByEventIdAsync(string eventId)
        {
            var sessionList = new List<EventSession>();
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return sessionList;
            }

            string url = $"{RealtimeDbUrl}/sessions/{Uri.EscapeDataString(eventId.Trim())}.json";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "null")
                    {
                        return sessionList;
                    }

                    using var doc = JsonDocument.Parse(jsonString);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            if (prop.Value.ValueKind != JsonValueKind.Object)
                                continue;

                            var item = prop.Value;
                            var session = new EventSession();

                            string idFromField = GetFlexibleString(item, "id");
                            session.Id = !string.IsNullOrEmpty(idFromField) ? idFromField : prop.Name;

                            string evIdFromField = GetFlexibleString(item, "eventId");
                            session.EventId = !string.IsNullOrEmpty(evIdFromField) ? evIdFromField : eventId.Trim();

                            session.SessionName = GetFlexibleString(item, "sessionName");
                            session.Speaker = GetFlexibleString(item, "speaker");
                            session.StartTime = GetFlexibleString(item, "startTime");
                            session.EndTime = GetFlexibleString(item, "endTime");
                            session.Description = GetFlexibleString(item, "description");

                            System.Diagnostics.Debug.WriteLine(
                                $"[DEBUG SESSION] Name: {session.SessionName} | Session EventId: '{session.EventId}' vs Target EventId: '{eventId}'");

                            sessionList.Add(session);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching sessions from Realtime DB: {ex.Message}");
            }

            return sessionList;
        }

        // Helper to read string, number, or boolean fields from Realtime DB JSON safely
        private string GetFlexibleString(JsonElement obj, string fieldName)
        {
            if (obj.TryGetProperty(fieldName, out var prop))
            {
                switch (prop.ValueKind)
                {
                    case JsonValueKind.String:
                        return prop.GetString() ?? string.Empty;
                    case JsonValueKind.Number:
                        return prop.GetRawText();
                    case JsonValueKind.True:
                        return "true";
                    case JsonValueKind.False:
                        return "false";
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }
    }
}