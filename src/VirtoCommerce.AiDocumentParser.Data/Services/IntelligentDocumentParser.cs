using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using USAddress;
using VirtoCommerce.AiDocumentParser.Core;
using VirtoCommerce.AiDocumentParser.Core.Models;
using VirtoCommerce.AiDocumentParser.Core.Services;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.AiDocumentParser.Data.Services
{
    public class IntelligentDocumentParser : IDocumentParser
    {
        private readonly ISettingsManager _settingsManager;

        public IntelligentDocumentParser(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public async Task<PurchaseOrderDocument> ParsePurchaseOrderDocument(Stream stream, string modelId)
        {
            var addressParser = new AddressParser();
            var po = new PurchaseOrderDocument();

            var endpoint = _settingsManager.GetValue<string>(ModuleConstants.Settings.General.Endpoint);
            var apiKey = _settingsManager.GetValue<string>(ModuleConstants.Settings.General.ApiKey);
            
            var credential = new AzureKeyCredential(apiKey);
            var client = new DocumentAnalysisClient(new Uri(endpoint), credential);

            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, modelId, stream);
            AnalyzeResult result = operation.Value;

            // To see the list of all the supported fields returned by service and its corresponding types for the
            // prebuilt-invoice model, consult:
            // https://aka.ms/azsdk/formrecognizer/invoicefieldschema

            for (var i = 0; i < result.Documents.Count; i++)
            {
                Console.WriteLine($"Document {i}:");

                AnalyzedDocument document = result.Documents[i];

                if (document.Fields.TryGetValue("VendorName", out DocumentField vendorNameField))
                {
                    if (vendorNameField.FieldType == DocumentFieldType.String)
                    {
                        string vendorName = vendorNameField.Value.AsString();
                        po.VendorAddressRecipient = vendorName;
                        //Console.WriteLine($"Vendor Name: '{vendorName}', with confidence {vendorNameField.Confidence}");
                    }
                }


                if (document.Fields.TryGetValue("CustomerAddressRecipient", out DocumentField customerAddressRecipientField))
                {
                    if (customerAddressRecipientField.FieldType == DocumentFieldType.String)
                    {
                        var customerName = customerAddressRecipientField.Value.AsString();
                        po.CustomerAddressRecipient = customerName;
                    }
                }

                if (document.Fields.TryGetValue("ShippingAddress", out DocumentField customerShippingAddressField))
                {
                    if (customerShippingAddressField.FieldType == DocumentFieldType.String)
                    {
                        var addressResult = addressParser.ParseAddress(customerShippingAddressField.Value.AsString());

                        NameParser.ParseName(po.CustomerAddressRecipient, out var firstName, out var lastName);

                        po.ShippingAddress = new POAddress()
                        {
                            AddressType = AddressType.Shipping,
                            Line1 = addressResult.StreetLine,
                            FirstName = firstName,
                            LastName = lastName,
                            CountryName = "USA",
                            City = addressResult.City,
                            RegionName = addressResult.State,
                            PostalCode = addressResult.Zip
                        };
                    }
                }

                if (document.Fields.TryGetValue("CustomerName", out DocumentField customerNameField))
                {
                    if (customerNameField.FieldType == DocumentFieldType.String)
                    {
                        string customerName = customerNameField.Value.AsString();
                        po.CustomerName = customerName;
                        Console.WriteLine($"Customer Name: '{customerName}', with confidence {customerNameField.Confidence}");
                    }
                }

                if (document.Fields.TryGetValue("CustomerNumber", out DocumentField customerIdField))
                {
                    if (customerIdField.FieldType == DocumentFieldType.String)
                    {
                        var customerId = customerIdField.Value.AsString();
                        po.CustomerNumber = customerId;
                    }
                }

                #region Items Section
                if (document.Fields.TryGetValue("Items", out DocumentField itemsField))
                {
                    if (itemsField.FieldType == DocumentFieldType.List)
                    {
                        po.LineItems = new List<PurchaseOrderLineItem>();
                        foreach (DocumentField itemField in itemsField.Value.AsList())
                        {
                            Console.WriteLine("Item:");

                            var lineItem = new PurchaseOrderLineItem();

                            if (itemField.FieldType == DocumentFieldType.Dictionary)
                            {
                                IReadOnlyDictionary<string, DocumentField> itemFields = itemField.Value.AsDictionary();

                                if (itemFields.TryGetValue("Description", out DocumentField itemDescriptionField))
                                {
                                    if (itemDescriptionField.FieldType == DocumentFieldType.String)
                                    {
                                        string itemDescription = itemDescriptionField.Value.AsString();
                                        lineItem.Description = itemDescription;

                                        //Console.WriteLine($"  Description: '{itemDescription}', with confidence {itemDescriptionField.Confidence}");
                                    }
                                }

                                if (itemFields.TryGetValue("Amount", out DocumentField itemAmountField))
                                {
                                    if (itemAmountField.FieldType == DocumentFieldType.Double)
                                    {
                                        var itemAmount = itemAmountField.Value.AsDouble();
                                        lineItem.Amount = itemAmount;

                                        //Console.WriteLine($"  Amount: '{itemAmount.Symbol}{itemAmount.Amount}', with confidence {itemAmountField.Confidence}");
                                    }
                                }

                                if (itemFields.TryGetValue("ProductCode", out DocumentField itemProductCodeField))
                                {
                                    if (itemProductCodeField.FieldType == DocumentFieldType.String)
                                    {
                                        string itemProductCode = itemProductCodeField.Value.AsString();
                                        lineItem.ProductCode = itemProductCode;

                                        //Console.WriteLine($"  Description: '{itemDescription}', with confidence {itemDescriptionField.Confidence}");
                                    }
                                }

                                if (itemFields.TryGetValue("Quantity", out DocumentField itemQuantityField))
                                {
                                    if (itemQuantityField.FieldType == DocumentFieldType.Int64)
                                    {
                                        long itemQuantity = itemQuantityField.Value.AsInt64();
                                        lineItem.Quantity = itemQuantity;

                                        //Console.WriteLine($"  Description: '{itemDescription}', with confidence {itemDescriptionField.Confidence}");
                                    }
                                }
                            }

                            po.LineItems.Add(lineItem);
                        }
                    }
                }
                #endregion

                if (document.Fields.TryGetValue("Comments", out DocumentField commentsField))
                {
                    if (commentsField.FieldType == DocumentFieldType.String)
                    {
                        string comments = commentsField.Value.AsString();
                        po.Comments = comments;
                    }
                }

                if (document.Fields.TryGetValue("SubTotal", out DocumentField subTotalField))
                {
                    if (subTotalField.FieldType == DocumentFieldType.Currency)
                    {
                        CurrencyValue subTotal = subTotalField.Value.AsCurrency();
                        Console.WriteLine($"Sub Total: '{subTotal.Symbol}{subTotal.Amount}', with confidence {subTotalField.Confidence}");
                    }
                }

                if (document.Fields.TryGetValue("TotalTax", out DocumentField totalTaxField))
                {
                    if (totalTaxField.FieldType == DocumentFieldType.Currency)
                    {
                        CurrencyValue totalTax = totalTaxField.Value.AsCurrency();
                        Console.WriteLine($"Total Tax: '{totalTax.Symbol}{totalTax.Amount}', with confidence {totalTaxField.Confidence}");
                    }
                }

                if (document.Fields.TryGetValue("InvoiceTotal", out DocumentField invoiceTotalField))
                {
                    if (invoiceTotalField.FieldType == DocumentFieldType.Currency)
                    {
                        CurrencyValue invoiceTotal = invoiceTotalField.Value.AsCurrency();
                        Console.WriteLine($"Invoice Total: '{invoiceTotal.Symbol}{invoiceTotal.Amount}', with confidence {invoiceTotalField.Confidence}");
                    }
                }
            }

            return po;
        }
    }
}
