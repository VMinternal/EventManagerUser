using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EventManagerUser
{
    public class FirebaseService
    {
        private readonly HttpClient _httpClient;
        private const string ProjectId = "dbtest-7a484"; // Project ID Firebase 

        public FirebaseService()
        {
            _httpClient = new HttpClient();
        }

        // Function to retrieve a list of events from the Firestore REST API
        public async Task<List<EventModel>> GetEventsAsync()
        {
            var eventList = new List<EventModel>();
            string url = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/events";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);

                    if (doc.RootElement.TryGetProperty("documents", out var docsElement))
                    {
                        foreach (var docElement in docsElement.EnumerateArray())
                        {
                            var eventModel = new EventModel();

                            // Extract the Document ID from the "name" path (e.g., projects/.../documents/events/ID_ABC)
                            if (docElement.TryGetProperty("name", out var nameProp))
                            {
                                string fullName = nameProp.GetString();
                                eventModel.Id = fullName?.Split('/').Last();
                            }

                            // Read data fields structured in Firestore REST JSON format
                            if (docElement.TryGetProperty("fields", out var fieldsProp))
                            {
                                eventModel.Name = GetStringField(fieldsProp, "name");
                                eventModel.Location = GetStringField(fieldsProp, "location");
                                eventModel.Date = GetStringField(fieldsProp, "date");
                                eventModel.Time = GetStringField(fieldsProp, "time");
                                eventModel.Reporter = GetStringField(fieldsProp, "reporter");
                                eventModel.Description = GetStringField(fieldsProp, "description");
                                eventModel.ContactEmail = GetStringField(fieldsProp, "contactEmail");
                                eventModel.ContactPhone = GetStringField(fieldsProp, "contactPhone");
                            }

                            eventList.Add(eventModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase API call error: {ex.Message}");
            }

            return eventList;
        }

        // Helper function to extract string data types from Firestore JSON
        private string GetStringField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var fieldProp) &&
                fieldProp.TryGetProperty("stringValue", out var valProp))
            {
                return valProp.GetString();
            }
            return string.Empty;
        }
    }
}
