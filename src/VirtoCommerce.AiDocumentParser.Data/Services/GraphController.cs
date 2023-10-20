using System;
using System.Linq;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;
using VirtoCommerce.AiDocumentParser.Core.Models;
using VirtoCommerce.Platform.Core.Caching;
using IdentityModel.Client;
using Azure.Core;

namespace VirtoCommerce.AiDocumentParser.Data.Services
{
    public class GraphController
    {
        readonly HttpClient _client = null;
        private readonly IPlatformMemoryCache _memoryCache;
        readonly string _ServiceUrl;
        readonly string _ClientId;
        readonly string _ClientSecret;


        public GraphController(string serviceUrl, string clientId, string clientSecret, IPlatformMemoryCache memoryCache)
        {
            _client = new HttpClient();
            _ServiceUrl = serviceUrl;
            _memoryCache = memoryCache;
            _ClientId = clientId;
            _ClientSecret = clientSecret;   
        }

        private async Task<HttpClient> GetAuthenticatedHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetOrRenewTokenAsync());
            return httpClient;
        }

        private async Task<string> GetOrRenewTokenAsync()
        {
            var cacheKey = CacheKey.With(GetType(), "token");

            var token = await _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                using (var client = new HttpClient())
                {
                    var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = $"{_ServiceUrl.TrimEnd('/')}/connect/token",
                        ClientId = _ClientId,
                        ClientSecret = _ClientSecret
                    });

                    if (!response.IsError)
                    {
                        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(response.ExpiresIn);
                        return response.AccessToken;
                    }

                    return null;
                }
            }, cacheNullValue: false);
            return token;
        }

        public async Task<GraphQLResponse> AddAddressToCart(string cartId, string storeId, string userId, string addressName, POAddress address)
        {
            // Arrange
            var query = @"
                mutation addCartAddress($command: InputAddOrUpdateCartAddressType!) {
                    addCartAddress(command: $command) {
                        id
                    }
                }";

            var variables = new
            {
                command = new
                {
                    storeId,
                    userId,
                    cartId,
                    address = new
                    {
                        addressType = address.AddressType,
                        firstName = address.FirstName,
                        lastName = address.LastName,
                        line1 = address.Line1,
                        city = address.City,
                        regionName = address.RegionName,
                        countryName = address.CountryName,
                        postalCode = address.PostalCode,
                        name = addressName
                    }
                }
            };

            var request = new GraphQLRequest { Query = query, Variables = variables };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            using (var client = await GetAuthenticatedHttpClient())
            {
                var response = await client.PostAsync($"{_ServiceUrl}/graphql", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GraphQLResponse>(responseContent);

                return result;
            }
        }

        public async Task<GraphQLResponse> AddItemToCart(string userId, string storeId, string productId, int quantity)
        {
            // Arrange
            var query = @"
                mutation AddItem($command: InputAddItemType!) {
                    addItem(command: $command) {
                        id
                    }
                }";

            var variables = new
            {
                command = new
                {
                    storeId,
                    userId,
                    productId,
                    quantity,
                }
            };

            var request = new GraphQLRequest { Query = query, Variables = variables };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            using (var client = await GetAuthenticatedHttpClient())
            {
                var response = await client.PostAsync($"{_ServiceUrl}/graphql", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GraphQLResponse>(responseContent);

                return result;
            }
        }

        public async Task<GraphQLResponse> AddItemsToCart(string userId, string storeId, PurchaseOrderLineItem[] skus)
        {
            // Arrange
            var query = @"
                mutation addBulkItemsCart($command: InputAddBulkItemsType!) {
                    addBulkItemsCart(command: $command) {
                        cart {
                            id
                            items
                            {
                                id
                            }
                        }
                    }
                }";

            var variables = new
            {
                command = new
                {
                    storeId,
                    cartName = "default",
                    userId,
                    cartType = "Bulk",
                    cartItems = skus.Select(x=>new { productSku = x.ProductCode, quantity = x.Quantity }).ToArray()
                }
            };

            var request = new GraphQLRequest { Query = query, Variables = variables };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            using (var client = await GetAuthenticatedHttpClient())
            {
                var response = await client.PostAsync($"{_ServiceUrl}/graphql", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GraphQLResponse>(responseContent);

                return result;
            }
        }

        //

        public async Task<GraphQLResponse> CreateQuote(string cartId, string comment)
        {
            // Arrange
            var query = @"
                mutation createQuoteFromCart($command: CreateQuoteFromCartCommandType!) {
                    createQuoteFromCart(command: $command) {
                        id
                    }
                }";

            var variables = new
            {
                command = new
                {
                    cartId,
                    comment
                }
            };

            var request = new GraphQLRequest { Query = query, Variables = variables };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            using (var client = await GetAuthenticatedHttpClient())
            {
                var response = await client.PostAsync($"{_ServiceUrl}/graphql", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GraphQLResponse>(responseContent);

                return result;
            }
        }

        public async Task<GraphQLResponse> CreateQuoteFromPO(string storeId, PurchaseOrderDocument po)
        {
            var result = await AddItemsToCart(po.CustomerNumber, storeId, po.LineItems.ToArray());

            // TODO: better handle exceptions here
            if (result.Data["addBulkItemsCart"] == null)
            {
                throw new ApplicationException(result.Errors.ToString());
            }

            var cartId = result.Data["addBulkItemsCart"]["cart"]["id"];

            // add addresses
            if (po.ShippingAddress != null)
            {
                var addressResult = await AddAddressToCart((string)cartId, storeId, po.CustomerNumber, "Shipping", po.ShippingAddress);
                if (addressResult.Data["addCartAddress"] == null)
                {
                    throw new ApplicationException(addressResult.Errors.ToString());
                }
            }


            var quote = await CreateQuote((string)cartId, po.Comments);
            return quote;
        }

    }
}
