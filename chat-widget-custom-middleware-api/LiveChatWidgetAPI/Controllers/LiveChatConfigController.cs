using LiveChatWidgetAPI.Middleware;
using LiveChatWidgetAPI.Models;
using LiveChatWidgetAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveChatWidgetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveChatConfigController : ControllerBase
    {
        private readonly D365Service _d365Service;
        private readonly ICurrentTokenAccessor _tokenAccessor;

        public LiveChatConfigController(D365Service d365Service, ICurrentTokenAccessor tokenAccessor)
        {
            _d365Service = d365Service;
            _tokenAccessor = tokenAccessor;
        }

        [HttpGet(nameof(GetPinnedMessages) + "/{workstreamId}")]
        public async Task<IActionResult> GetPinnedMessages(
            string workstreamId,
            [FromQuery] bool isAuthenticated)
        {
            var result = await _d365Service.GetPinnedMessagesAsync(_tokenAccessor.AccessToken!, workstreamId, isAuthenticated);
            return Content(result, "application/json");
        }

        [HttpGet(nameof(GetSystemMessages) + "/{id}")]
        public async Task<IActionResult> GetSystemMessages(string id)
        {
            var result = await _d365Service.GetSystemMessagesByWorkstreamAsync(id, _tokenAccessor.AccessToken!);
            return Content(result, "application/json");
        }

        [HttpGet(nameof(GetPostChatSurveyConfig) + "/{widgetId}")]
        public async Task<IActionResult> GetPostChatSurveyConfig(string widgetId)
        {
            var result = await _d365Service.GetPostChatSurveyConfigAsync(widgetId, _tokenAccessor.AccessToken!);
            return Content(result, "application/json");
        }

        [HttpGet(nameof(GetPostConversationSurveyQuestions))]
        public async Task<IActionResult> GetPostConversationSurveyQuestions([FromQuery] string widgetId)
        {
            var result = await _d365Service.GetPostConversationSurveyQuestionsAsync(widgetId, _tokenAccessor.AccessToken!);
            return Content(result, "application/json");
        }

        [HttpPost(nameof(SubmitSurveyResponse))]
        public async Task<IActionResult> SubmitSurveyResponse([FromBody] SurveyResponseRequest payload)
        {
            var result = await _d365Service.SubmitSurveyResponseAsync(payload, _tokenAccessor.AccessToken!);
            return Content(result, "application/json");
        }
    }
}
