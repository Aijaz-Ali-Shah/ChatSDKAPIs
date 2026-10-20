using System.Text.Json.Serialization;

namespace LiveChatWidgetAPI.Models
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class SurveyResponseRequest
    {
        [JsonPropertyName("surveyId")]
        public Guid SurveyId { get; set; }

        [JsonPropertyName("respondentEmail")]
        public string? RespondentEmail { get; set; }

        [JsonPropertyName("respondentName")]
        public string? RespondentName { get; set; }

        [JsonPropertyName("startedAt")]
        public DateTimeOffset? StartedAt { get; set; }

        [JsonPropertyName("answers")]
        public List<SurveyAnswerRequest>? Answers { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class SurveyAnswerRequest
    {
        [JsonPropertyName("questionId")]
        public Guid QuestionId { get; set; }

        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }
}
