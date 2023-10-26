using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Moq;
using VirtoCommerce.AiDocumentParser.Core;
using VirtoCommerce.AiDocumentParser.Data.Services;
using VirtoCommerce.AiDocumentParser.Tests.Helpers;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.AiDocumentParser.Tests;

[Trait("Category", "Unit")]
public class DocumentScenarios
{
    private readonly Mock<IPlatformMemoryCache> _memoryCasheMock;

    public DocumentScenarios()
    {
        _memoryCasheMock = new Mock<IPlatformMemoryCache>();

        //  setup cache mocks
        var cacheEntry = new Mock<ICacheEntry>();
        cacheEntry.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
        var cacheKey = CacheKey.With(typeof(GraphController), "token");
        _memoryCasheMock.Setup(pmc => pmc.CreateEntry(cacheKey)).Returns(cacheEntry.Object);
        _memoryCasheMock.Setup(x => x.GetDefaultCacheEntryOptions()).Returns(() => new MemoryCacheEntryOptions());
    }

    [Fact]
    public async Task Add_Item_To_Cart()
    {
        var controller = new GraphController("https://localhost:5001", "", "", _memoryCasheMock.Object);

        var result = await controller.AddItemToCart("test", "B2B-store", "e0a441cb-efe7-4d9a-927d-1dabf23db1ff", 1);

        // Assert
        var cartId = result.Data["addItem"]["id"];
        Assert.NotNull(cartId);

        var quote = await controller.CreateQuote((string)cartId, "automatic quote");
    }

    //[Fact]
    //public async Task Add_Items_To_Cart()
    //{
    //    var controller = new GraphController("https://localhost:5001/graphql");

    //    var result = await controller.AddItemsToCart("B2B-store", "test", new string[] { "122718", "102909" });

    //    // Assert
    //    var cartId = result.Data["addBulkItemsCart"]["cart"]["id"];
    //    Assert.NotNull(cartId);

    //    var quote = await controller.CreateQuote((string)cartId, "automatic quote");
    //}


    /// <summary>
    /// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/formrecognizer/Azure.AI.FormRecognizer/samples/Sample_AnalyzeWithCustomModel.md
    /// </summary>
    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async void Process_PDF_Create_Quote()
    {
        string endpoint = "https://westus2.api.cognitive.microsoft.com/";
        string apiKey = Env.Var("AZURE_COGNITIVE_API_KEY");

        var mockSettingsManager = new Mock<ISettingsManager>();
        mockSettingsManager.Setup(sm => sm.GetObjectSettingAsync(ModuleConstants.Settings.General.Endpoint.Name, null, null)).ReturnsAsync(new ObjectSettingEntry() { Value = endpoint });
        mockSettingsManager.Setup(sm => sm.GetObjectSettingAsync(ModuleConstants.Settings.General.ApiKey.Name, null, null)).ReturnsAsync(new ObjectSettingEntry() { Value = apiKey });

        var parser = new IntelligentDocumentParser(mockSettingsManager.Object);

        string filePath = "data/purchase-order3.pdf";

        using var stream = new FileStream(filePath, FileMode.Open);
        var po = await parser.ParsePurchaseOrderDocument(stream, "PO4");

        var controller = new GraphController("https://localhost:5001", "", "", _memoryCasheMock.Object);
        var result2 = await controller.CreateQuoteFromPO("B2B-store", po);
    }
}
