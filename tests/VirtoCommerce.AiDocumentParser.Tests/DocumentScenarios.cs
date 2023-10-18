using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using System.Collections.Generic;
using System.IO;
using System;
using Xunit;
using System.Threading.Tasks;
using VirtoCommerce.AiDocumentParser.Tests.Helpers;
using VirtoCommerce.AiDocumentParser.Core.Models;
using VirtoCommerce.AiDocumentParser.Data.Services;
using VirtoCommerce.Platform.Core.Settings;
using Moq;
using VirtoCommerce.AiDocumentParser.Core;

namespace VirtoCommerce.AiDocumentParser.Tests;

[Trait("Category", "Unit")]
public class DocumentScenarios
{
    [Fact]
    public async Task Add_Item_To_Cart()
    {
        var controller = new GraphController("https://localhost:5001/graphql");

        var result = await controller.AddItemToCart("B2B-store", "test", "e0a441cb-efe7-4d9a-927d-1dabf23db1ff", 1);

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

        var controller = new GraphController("https://localhost:5001/graphql");
        var result2 = await controller.CreateQuoteFromPO("B2B-store", po);      
    }
}
