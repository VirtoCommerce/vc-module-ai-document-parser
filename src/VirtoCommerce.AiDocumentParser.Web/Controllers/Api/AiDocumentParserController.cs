using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    }
}
