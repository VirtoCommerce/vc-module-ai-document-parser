using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.AiDocumentParser.Core.Models;

namespace VirtoCommerce.AiDocumentParser.Core.Services
{
    public interface IDocumentParser
    {
        public Task<PurchaseOrderDocument> ParsePurchaseOrderDocument(Stream document, string modelId);
    }
}
