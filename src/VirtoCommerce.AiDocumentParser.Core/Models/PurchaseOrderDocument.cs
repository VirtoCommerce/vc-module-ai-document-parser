using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.AiDocumentParser.Core.Models
{
    // A class that represents a purchase order
    public class PurchaseOrderDocument
    {
        // Properties of the purchase order
        public string CustomerName { get; set; }

        public POAddress VendorAddress { get; set; }

        public string VendorAddressRecipient { get; set; }


        public string CustomerAddressRecipient { get; set; }
        public POAddress BillingAddress { get; set; }
        public string BillingAddressRecipient { get; set; }
        public POAddress ShippingAddress { get; set; }
        public string ShippingAddressRecipient { get; set; }

        public DateTime PurchaseOrderDate { get; set; }


        public string PurchaseOrderNumber { get; set; }

        public List<PurchaseOrderLineItem> LineItems { get; set; }

        public int SubTotal { get; set; }

        public int Total { get; set; }
        public string Comments { get; set; }
        public string CustomerNumber { get; set; }
    }

    // A class that represents a line item
    public class PurchaseOrderLineItem
    {
        public decimal UnitPrice { get; set; }

        public double Amount { get; set; }

        public string Description { get; set; }

        public string ProductCode { get; set; }

        public long Quantity { get; set; }
    }

    [Flags]
    public enum AddressType
    {
        Undefined = 0,
        Billing = 1,
        Shipping = 2,
        Pickup = 4,
        BillingAndShipping = Billing | Shipping
    }

    public class POAddress
    {
        public AddressType AddressType { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Zip { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string RegionId { get; set; }
        public string RegionName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
