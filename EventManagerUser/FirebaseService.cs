using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

                            // Use the Document ID from the path name as a fallback.
                            string docId = string.Empty;
                            if (docElement.TryGetProperty("name", out var nameProp))
                            {
                                string fullName = nameProp.GetString();
                                docId = fullName?.Split('/').Last() ?? string.Empty;
                            }

                            // Read data fields structured in Firestore REST JSON format
                            if (docElement.TryGetProperty("fields", out var fieldsProp))
                            {
                                // Retrieve the "id" field from the Firestore fields to match the `eventId` of the sessions exactly.
                                eventModel.Id = GetStringField(fieldsProp, "id");

                                // If the "id" field is empty, use the Document ID as a fallback.
                                if (string.IsNullOrEmpty(eventModel.Id))
                                {
                                    eventModel.Id = docId;
                                }

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

        // Function to retrieve sessions by Event ID (Foreign Key linked to Event)
        public async Task<List<EventSession>> GetSessionsByEventIdAsync(string eventId)
        {
            var sessionList = new List<EventSession>();
            string url = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/sessions";

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
                            var session = new EventSession();

                            if (docElement.TryGetProperty("name", out var nameProp))
                            {
                                session.Id = nameProp.GetString()?.Split('/').Last();
                            }

                            if (docElement.TryGetProperty("fields", out var fieldsProp))
                            {
                                session.EventId = GetStringField(fieldsProp, "eventId");
                                session.SessionName = GetStringField(fieldsProp, "sessionName");
                                session.Speaker = GetStringField(fieldsProp, "speaker");
                                session.StartTime = GetStringField(fieldsProp, "startTime");
                                session.EndTime = GetStringField(fieldsProp, "endTime");
                                session.Description = GetStringField(fieldsProp, "description");
                            }

                            // Debug and cross-check the ID between the Session and the target Event.
                            System.Diagnostics.Debug.WriteLine($"[DEBUG SESSION] Name: {session.SessionName} | Session EventId: '{session.EventId}' vs Target EventId: '{eventId}'");

                            // Comparing the safe use of Trim() with case-insensitive handling
                            if (!string.IsNullOrEmpty(session.EventId) &&
                                !string.IsNullOrEmpty(eventId) &&
                                session.EventId.Trim().Equals(eventId.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                sessionList.Add(session);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching sessions: {ex.Message}");
            }

            return sessionList;
        }

        // Helper function to extract string data types from Firestore JSON securely
        private string GetStringField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var fieldProp))
            {
                if (fieldProp.TryGetProperty("stringValue", out var valProp))
                {
                    return valProp.GetString() ?? string.Empty;
                }
                if (fieldProp.TryGetProperty("integerValue", out var intProp))
                {
                    if (intProp.ValueKind == JsonValueKind.String)
                    {
                        return intProp.GetString() ?? string.Empty;
                    }
                    else if (intProp.ValueKind == JsonValueKind.Number)
                    {
                        return intProp.GetInt32().ToString();
                    }
                    return intProp.GetRawText().Trim('"');
                }
            }
            return string.Empty;
        }
    }
}