namespace VirtoCommerce.AiDocumentParser.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NameParser
    {
        private static readonly HashSet<string> Suffixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Jr.", "Sr.", "II", "III", "IV", "V", "Ph.D.", "MD", "Esq."
    };

        public static void ParseName(string fullName, out string firstName, out string lastName)
        {
            var nameParts = fullName.Split(' ');

            firstName = nameParts[0];

            // If there's a recognized suffix at the end
            if (nameParts.Length > 1 && Suffixes.Contains(nameParts[nameParts.Length - 1]))
            {
                lastName = nameParts.Length > 2 ? nameParts[nameParts.Length - 2] : "";
            }
            else
            {
                lastName = nameParts.Length > 1 ? nameParts[nameParts.Length - 1] : "";
            }
        }
    }
}
