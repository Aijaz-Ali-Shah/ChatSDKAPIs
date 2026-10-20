using LiveChatWidgetAPI.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiveChatWidgetAPI.Services
{
    public class D365Service
    {
        private static readonly JsonSerializerOptions SurveyResponseSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public D365Service(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }
      
public async Task<string> GetPinnedMessagesAsync(
    string accessToken,
    string id,
    bool isAuthenticated)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException(
                    "Access token is required.",
                    nameof(accessToken));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "Workstream ID is required.",
                    nameof(id));
            }

            var workstreamId = id.Trim();

            // Dataverse OData DateTimeOffset format.
            var now = DateTime.UtcNow.ToString(
                "yyyy-MM-ddTHH:mm:ssZ",
                CultureInfo.InvariantCulture);

            // Your Dataverse option-set values.
            const int Registered = 934730000;
            const int Guest = 934730001;
            const int Both = 934730002;

            var customerTypeFilter = isAuthenticated
                ? $"(tvs_customertype eq {Registered} or tvs_customertype eq {Both})"
                : $"(tvs_customertype eq {Guest} or tvs_customertype eq {Both})";

            var query =
                "/api/data/v9.2/tvs_pinnedmessages" +
                "?$select=" +
                    "tvs_message," +
                    "tvs_customertype," +
                    "tvs_pinnedmessagename," +
                    "statuscode," +
                    "_tvs_language_value," +
                    "tvs_messagetype," +
                    "tvs_enddate," +
                    "tvs_startdate" +
                "&$filter=" +
                    "statuscode eq 1" +
                    $" and tvs_startdate le {now}" +
                    $" and (tvs_enddate eq null or tvs_enddate ge {now})" +
                    $" and {customerTypeFilter}" +
                    $" and tvs_PinnedMessage_msdyn_liveworkstream_msdyn_liveworkstream" +
                    $"/any(ws: ws/msdyn_liveworkstreamid eq '{workstreamId}')" +
                "&$expand=" +
                    "tvs_PinnedMessage_msdyn_liveworkstream_msdyn_liveworkstream" +
                    "($select=msdyn_liveworkstreamid,msdyn_name)";

            var response = await SendDataverseGetAsync(
                query,
                accessToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<PinnedMessageResponse>(
                response,
                options);

            if (data?.Value == null)
            {
                return JsonSerializer.Serialize(
                    new PinnedMessageResponse(),
                    options);
            }

            return JsonSerializer.Serialize(
                data,
                options);
        }


       
     //   public async Task<string> GetPinnedMessagesAsync(
     //string accessToken,
     //string id,
     //bool isAuthenticated)
     //   {
     //       if (string.IsNullOrWhiteSpace(accessToken))
     //       {
     //           throw new ArgumentException(
     //               "Access token is required.",
     //               nameof(accessToken));
     //       }

     //       if (string.IsNullOrWhiteSpace(id))
     //       {
     //           throw new ArgumentException(
     //               "Workstream ID is required.",
     //               nameof(id));
     //       }

     //       var query =
     //           "/api/data/v9.2/tvs_pinnedmessages" +
     //           "?$select=" +
     //               "tvs_message," +
     //               "tvs_customertype," +
     //               "tvs_pinnedmessagename," +
     //               "statuscode," +
     //               "_tvs_language_value," +
     //               "tvs_messagetype," +
     //               "tvs_enddate," +
     //               "tvs_startdate" +
     //           "&$expand=" +
     //               "tvs_PinnedMessage_msdyn_liveworkstream_msdyn_liveworkstream" +
     //               "($select=msdyn_liveworkstreamid,msdyn_name)";

     //       var response = await SendDataverseGetAsync(
     //           query,
     //           accessToken);

     //       var options = new JsonSerializerOptions
     //       {
     //           PropertyNameCaseInsensitive = true
     //       };

     //       var data = JsonSerializer.Deserialize<PinnedMessageResponse>(
     //           response,
     //           options);

     //       if (data?.Value == null || data.Value.Count == 0)
     //       {
     //           return JsonSerializer.Serialize(
     //               new PinnedMessageResponse(),
     //               options);
     //       }

     //       var now = DateTime.UtcNow;
     //       var workstreamId = id.Trim();

     //       var filteredRecords = data.Value
     //           .Where(x =>
     //               // Active
     //               x.statuscode == 1

     //               // Start date has arrived
     //               && x.tvs_startdate.HasValue
     //               && x.tvs_startdate.Value <= now

     //               // End date is null OR has not expired
     //               && (!x.tvs_enddate.HasValue ||
     //                   x.tvs_enddate.Value >= now)

     //               // Workstream ID matches
     //               && x.tvs_PinnedMessage_msdyn_liveworkstream_msdyn_liveworkstream
     //                   .Any(ws =>
     //                       string.Equals(
     //                           ws.msdyn_liveworkstreamid?.Trim(),
     //                           workstreamId,
     //                           StringComparison.OrdinalIgnoreCase))

     //               // Customer type is allowed
     //               && IsCustomerTypeAllowed(
     //                   x.tvs_customertype,
     //                   isAuthenticated))
     //           .ToList();

     //       var result = new PinnedMessageResponse
     //       {
     //           Value = filteredRecords
     //       };

     //       return JsonSerializer.Serialize(result, options);
     //   }

        public async Task<string> GetSystemMessagesByWorkstreamAsync(string workstreamId, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(workstreamId))
            {
                throw new ArgumentException("Workstream id is required.", nameof(workstreamId));
            }

            if (!Guid.TryParse(workstreamId, out var parsedWorkstreamId))
            {
                throw new ArgumentException("Workstream id must be a valid GUID.", nameof(workstreamId));
            }

            var configQuery =
                $"/api/data/v9.2/msdyn_livechatconfigs" +
                $"?$select=msdyn_livechatconfigid" +
                $"&$filter=_msdyn_liveworkstreamid_value eq {parsedWorkstreamId}";

            var configJson = await SendDataverseGetAsync(configQuery, accessToken);

            using var configDoc = JsonDocument.Parse(configJson);
            if (!configDoc.RootElement.TryGetProperty("value", out var configs) ||
                configs.ValueKind != JsonValueKind.Array ||
                configs.GetArrayLength() == 0)
            {
                throw new KeyNotFoundException($"No live chat config found for workstream '{parsedWorkstreamId}'.");
            }

            var liveChatConfigId = configs[0].GetProperty("msdyn_livechatconfigid").GetString();
            if (string.IsNullOrWhiteSpace(liveChatConfigId) ||
                !Guid.TryParse(liveChatConfigId, out var parsedConfigId))
            {
                throw new InvalidOperationException("Live chat config record did not contain a valid msdyn_livechatconfigid.");
            }

            // msdyn_instanceid is the live chat config id; statecode 0 = Active.
            var messagesQuery =
                    $"/api/data/v9.2/msdyn_ocsystemmessages" +
                    $"?$select=msdyn_messagetext,msdyn_systemmessageeventtype,msdyn_messagereceiver,statecode,statuscode" +
                    $"&$filter=msdyn_instanceid eq '{parsedConfigId}'" +
                    $" and statecode eq 0" +
                    $" and msdyn_systemmessageeventtype eq 192350036";


            // return await SendDataverseGetAsync(messagesQuery, accessToken);
            var responseJson = await SendDataverseGetAsync(messagesQuery, accessToken);

            using var doc = JsonDocument.Parse(responseJson);

            var result = new Dictionary<string, JsonElement>();

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                if (property.Name != "@odata.context")
                {
                    result[property.Name] = property.Value.Clone();
                }
            }

            return JsonSerializer.Serialize(result);
        }

        public async Task<string> GetPostChatSurveyConfigAsync(string widgetId, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(widgetId))
            {
                throw new ArgumentException("Widget id is required.", nameof(widgetId));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", nameof(accessToken));
            }

            var widgetFilterValue = Guid.TryParse(widgetId, out var parsedWidgetId)
                ? parsedWidgetId.ToString()
                : $"'{widgetId.Replace("'", "''")}'";

            var configQuery =
                $"/api/data/v9.2/msdyn_livechatconfigs" +
                $"?$select=" +
                    "_msdyn_postconversationsurvey_value," +
                    "msdyn_postconversationsurveymessagetext," +
                    "msdyn_postconversationsurveyenable," +
                    "msdyn_postconversationsurveymode," +
                    "msdyn_postconversationsurveybotsurveymode," +
                    "_msdyn_postconversationsurveyseparatebotsurvey_value," +
                    "msdyn_postconversationsurveybotsurvey," +
                    "msdyn_postconversationsurveybotsurveymessagetext" +
                $"&$filter=msdyn_widgetappid eq {widgetFilterValue}";

            var configJson = await SendDataverseGetAsync(configQuery, accessToken);

            using var configDoc = JsonDocument.Parse(configJson);
            if (!configDoc.RootElement.TryGetProperty("value", out var configs) ||
                configs.ValueKind != JsonValueKind.Array ||
                configs.GetArrayLength() == 0)
            {
                throw new KeyNotFoundException($"No live chat config found for widget '{widgetId}'.");
            }

            var config = configs[0];

            return JsonSerializer.Serialize(new
            {
                msdyn_postconversationsurveyenable = GetJsonValue(config, "msdyn_postconversationsurveyenable"),
                msdyn_postconversationsurveymode = GetJsonValue(config, "msdyn_postconversationsurveymode"),
                msdyn_postconversationsurveymessagetext = GetJsonValue(config, "msdyn_postconversationsurveymessagetext"),
                msdyn_postconversationsurveybotsurveymode = GetJsonValue(config, "msdyn_postconversationsurveybotsurveymode"),
                msdyn_postconversationsurveybotsurvey = GetJsonValue(config, "msdyn_postconversationsurveybotsurvey"),
                msdyn_postconversationsurveybotsurveymessagetext = GetJsonValue(config, "msdyn_postconversationsurveybotsurveymessagetext")
            });
        }

        public async Task<string> GetPostConversationSurveyQuestionsAsync(string widgetId, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(widgetId))
            {
                throw new ArgumentException("Widget id is required.", nameof(widgetId));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", nameof(accessToken));
            }

            var widgetFilterValue = Guid.TryParse(widgetId, out var parsedWidgetId)
                ? parsedWidgetId.ToString()
                : $"'{widgetId.Replace("'", "''")}'";

            var configQuery =
                $"/api/data/v9.2/msdyn_livechatconfigs" +
                $"?$select=_msdyn_postconversationsurvey_value" +
                $"&$filter=msdyn_widgetappid eq {widgetFilterValue}";

            var configJson = await SendDataverseGetAsync(configQuery, accessToken);

            using var configDoc = JsonDocument.Parse(configJson);
            if (!configDoc.RootElement.TryGetProperty("value", out var configs) ||
                configs.ValueKind != JsonValueKind.Array ||
                configs.GetArrayLength() == 0)
            {
                throw new KeyNotFoundException($"No live chat config found for widget '{widgetId}'.");
            }

            if (!configs[0].TryGetProperty("_msdyn_postconversationsurvey_value", out var surveyValueElement) ||
                surveyValueElement.ValueKind != JsonValueKind.String)
            {
                throw new KeyNotFoundException($"No post-conversation survey is configured for widget '{widgetId}'.");
            }

            var surveyValue = surveyValueElement.GetString();
            if (string.IsNullOrWhiteSpace(surveyValue) ||
                !Guid.TryParse(surveyValue, out var parsedSurveyId))
            {
                throw new InvalidOperationException("Live chat config did not contain a valid _msdyn_postconversationsurvey_value.");
            }

            var questionsQuery =
             $"/api/data/v9.2/msfp_questions" +
             $"?$select=msfp_questiontext,msfp_questionchoices,msfp_responserequired" +
             $"&$filter=_msfp_survey_value eq {parsedSurveyId} and statuscode eq 1";

            var questionsJson = await SendDataverseGetAsync(questionsQuery, accessToken);

            using var questionsDoc = JsonDocument.Parse(questionsJson);
            var questions = questionsDoc.RootElement.TryGetProperty("value", out var questionsValue)
                ? questionsValue
                : questionsDoc.RootElement;

            return JsonSerializer.Serialize(new
            {
                _msdyn_postconversationsurvey_value = parsedSurveyId.ToString(),
                value = questions
            });
        }

        public async Task<string> SubmitSurveyResponseAsync(SurveyResponseRequest payload, string accessToken)
        {
            ValidateSurveyResponse(payload);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", nameof(accessToken));
            }

            var dataversePayload = MapToDataverseSurveyResponse(payload);
            var body = JsonSerializer.Serialize(dataversePayload, SurveyResponseSerializerOptions);

            var responseJson = await SendDataversePostAsync(
                "/api/data/v9.2/msfp_surveyresponses",
                body,
                accessToken);

            using var responseDoc = JsonDocument.Parse(responseJson);
            var cleaned = responseDoc.RootElement.EnumerateObject()
                .Where(p => !p.Name.StartsWith("@odata.", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(p => p.Name, p => p.Value);

            return JsonSerializer.Serialize(cleaned);
        }

        private static void ValidateSurveyResponse(SurveyResponseRequest payload)
        {
            if (payload is null)
            {
                throw new ArgumentException("Request body is required.");
            }

            if (payload.SurveyId == Guid.Empty)
            {
                throw new ArgumentException("surveyId is required.");
            }

            if (payload.Answers is null || payload.Answers.Count == 0)
            {
                throw new ArgumentException("At least one answer is required.");
            }

            for (var i = 0; i < payload.Answers.Count; i++)
            {
                var answer = payload.Answers[i];
                if (answer is null)
                {
                    throw new ArgumentException($"answers[{i}] is required.");
                }

                if (answer.QuestionId == Guid.Empty)
                {
                    throw new ArgumentException($"answers[{i}].questionId is required.");
                }
            }
        }

        private static DataverseSurveyResponse MapToDataverseSurveyResponse(SurveyResponseRequest payload)
        {
            var submittedAt = DateTimeOffset.UtcNow;
            var answers = payload.Answers!;

            var answered = answers
                .Where(answer => answer.Response is not null)
                .Select(answer => new DataverseQuestionResponseListItem
                {
                    QuestionId = answer.QuestionId.ToString(),
                    Response = answer.Response
                })
                .ToList();

            return new DataverseSurveyResponse
            {
                SurveyBind = $"/msfp_surveies({payload.SurveyId})",
                RespondentEmailAddress = NormalizeOptionalText(payload.RespondentEmail),
                Respondent = NormalizeOptionalText(payload.RespondentName),
                SubmitDate = submittedAt,
                ActualStart = payload.StartedAt ?? submittedAt,
                QuestionResponsesList = JsonSerializer.Serialize(answered, SurveyResponseSerializerOptions),
                QuestionResponses = answers
                    .Select((answer, index) => new DataverseQuestionResponse
                    {
                        Name = $"Q{index + 1} Response",
                        Response = answer.Response,
                        QuestionBind = $"/msfp_questions({answer.QuestionId})"
                    })
                    .ToList()
            };
        }

        private static string? NormalizeOptionalText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private async Task<string> SendDataverseGetAsync(string relativeQuery, string accessToken)
        {
            var baseUrl = _config["D365:BaseUrl"]?.TrimEnd('/');
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{relativeQuery}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("OData-MaxVersion", "4.0");
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("Prefer", "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"D365 request failed: {response.StatusCode} - {responseString}");
            }

            return responseString;
        }

        private async Task<string> SendDataversePostAsync(string relativePath, string jsonBody, string accessToken)
        {
            var baseUrl = _config["D365:BaseUrl"]?.TrimEnd('/');
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}{relativePath}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("OData-MaxVersion", "4.0");
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("Prefer", "return=representation");
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"D365 request failed: {response.StatusCode} - {responseString}");
            }

            return responseString;
        }

        private static object? GetJsonValue(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) ||
                property.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                return null;
            }

            return property.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => property.TryGetInt64(out var number) ? number : property.GetDouble(),
                JsonValueKind.String => property.GetString(),
                _ => property.GetRawText()
            };
        }

        //private bool IsCustomerTypeAllowed(int customerType, bool isAuthenticated)
        //{
        //    // Your Dataverse option-set values
        //    const int Registered = 934730000;
        //    const int Guest = 934730001;
        //    const int Both = 934730002;

        //    if (customerType == Both)
        //    {
        //        return true;
        //    }

        //    if (isAuthenticated && customerType == Registered)
        //    {
        //        return true;
        //    }

        //    if (!isAuthenticated && customerType == Guest)
        //    {
        //        return true;
        //    }

        //    return false;
        //}
    }
}
