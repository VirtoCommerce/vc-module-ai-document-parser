using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostmarkDotNet.Webhooks;
using VirtoCommerce.AiDocumentParser.Core;
using VirtoCommerce.AiDocumentParser.Core.Services;
using VirtoCommerce.AiDocumentParser.Data.Services;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.AiDocumentParser.Web.Controllers.Api
{
    [Route("api/ai/document-parser")]
    public class AiDocumentParserController : Controller
    {

        private readonly ISettingsManager _settingsManager;
        private readonly IDocumentParser _aiDocumentParser;

        public AiDocumentParserController(
            ISettingsManager settingsManager,
            IDocumentParser aiDocumentParser)
        {
            _settingsManager = settingsManager;
            _aiDocumentParser = aiDocumentParser;
        }

        /// <summary>
        /// Parse document
        /// </summary>
        /// <remarks>Return document</remarks>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> ParseDocument(string store, IFormFile file)
        {
            var modelId = _settingsManager.GetValue<string>(ModuleConstants.Settings.General.ModelId);
            var po = await _aiDocumentParser.ParsePurchaseOrderDocument(file.OpenReadStream(), modelId);

            if (po != null)
            {
                var graphUrl = $"{Request.Scheme}://{Request.Host}/graphql";
                var controller = new GraphController(graphUrl);
                var result2 = await controller.CreateQuoteFromPO(store, po);
            }

            return Ok();
        }

        [HttpPost]
        [Route("~/api/ai/document-parser/{store}/postmark-webhook")]
        public async Task<ActionResult> PostInboundWebhook([FromRoute] string store, [FromBody]PostmarkInboundWebhookMessage message)
        {
            if (message == null || message.Attachments == null || message.Attachments.Count == 0)
            {
                return BadRequest("Invalid message or no attachments found in the message.");
            }

            var fileContent = message.Attachments[0].Content;

            if (string.IsNullOrEmpty(fileContent))
            {
                return BadRequest("Attachment content is empty.");
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(fileContent)))
                {
                    var modelId = _settingsManager.GetValue<string>(ModuleConstants.Settings.General.ModelId);
                    var po = await _aiDocumentParser.ParsePurchaseOrderDocument(stream, modelId);
                    var xapiEndPoint = _settingsManager.GetValue<string>(ModuleConstants.Settings.General.XApiEndpoint);

                    if (po != null)
                    {
                        var graphUrl = xapiEndPoint ?? $"{Request.Scheme}://{Request.Host}/graphql";
                        var controller = new GraphController(graphUrl);
                        var result2 = await controller.CreateQuoteFromPO(store, po);
                    }
                }

                // Process the inbound hook data
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while processing the attachment: {ex.Message}");
            }
        }
    }
}
