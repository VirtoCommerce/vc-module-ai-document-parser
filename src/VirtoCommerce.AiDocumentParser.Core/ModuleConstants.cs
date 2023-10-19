using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.AiDocumentParser.Core;

public static class ModuleConstants
{
    public static class Security
    {
        public static class Permissions
        {
            public const string Access = "AiDocumentParser:access";
            public const string Create = "AiDocumentParser:create";
            public const string Read = "AiDocumentParser:read";
            public const string Update = "AiDocumentParser:update";
            public const string Delete = "AiDocumentParser:delete";

            public static string[] AllPermissions { get; } =
            {
                Access,
                Create,
                Read,
                Update,
                Delete,
            };
        }
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor XApiEndpoint { get; } = new SettingDescriptor
            {
                Name = "Ai.DocumentParser.XApiEndpoint",
                GroupName = "XAPI",
                ValueType = SettingValueType.ShortText,
                DefaultValue = "https://localhost:5001/graphql",
            };

            public static SettingDescriptor Endpoint { get; } = new SettingDescriptor
            {
                Name = "Ai.DocumentParser.Endpoint",
                GroupName = "Service",
                ValueType = SettingValueType.ShortText,
                DefaultValue = "https://westus2.api.cognitive.microsoft.com/",
            };

            public static SettingDescriptor ModelId { get; } = new SettingDescriptor
            {
                Name = "Ai.DocumentParser.ModelId",
                GroupName = "Service",
                ValueType = SettingValueType.ShortText,
                DefaultValue = "PO",
            };

            public static SettingDescriptor ApiKey { get; } = new SettingDescriptor
            {
                Name = "Ai.DocumentParser.ApiKey",
                GroupName = "Service",
                ValueType = SettingValueType.SecureString,
                DefaultValue = "",
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return Endpoint;
                    yield return ApiKey;
                    yield return ModelId;
                    yield return XApiEndpoint;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings
        {
            get
            {
                return General.AllGeneralSettings;
            }
        }
    }
}
