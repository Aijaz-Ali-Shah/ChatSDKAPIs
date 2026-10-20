using System.Text.Json.Serialization;

namespace LiveChatWidgetAPI.Models
{
    internal sealed class DataverseSurveyResponse
    {
        [JsonPropertyName("msfp_surveyid_msfp_surveyresponse@odata.bind")]
        public string? SurveyBind { get; set; }

        [JsonPropertyName("msfp_respondentemailaddress")]
        public string? RespondentEmailAddress { get; set; }

        [JsonPropertyName("msfp_respondent")]
        public string? Respondent { get; set; }

        [JsonPropertyName("msfp_submitdate")]
        public DateTimeOffset SubmitDate { get; set; }

        [JsonPropertyName("actualstart")]
        public DateTimeOffset ActualStart { get; set; }

        [JsonPropertyName("msfp_questionresponseslist")]
        public string? QuestionResponsesList { get; set; }

        [JsonPropertyName("msfp_msfp_surveyresponse_msfp_questionresponse_surveyresponseid")]
        public List<DataverseQuestionResponse>? QuestionResponses { get; set; }
    }

    internal sealed class DataverseQuestionResponse
    {
        [JsonPropertyName("msfp_name")]
        public string? Name { get; set; }

        [JsonPropertyName("msfp_response")]
        public string? Response { get; set; }

        [JsonPropertyName("msfp_questionid@odata.bind")]
        public string? QuestionBind { get; set; }
    }

    internal sealed class DataverseQuestionResponseListItem
    {
        [JsonPropertyName("questionId")]
        public string? QuestionId { get; set; }

        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }
}
