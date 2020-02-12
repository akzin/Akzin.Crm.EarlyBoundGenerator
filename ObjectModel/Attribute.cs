using System;
using System.Collections.Generic;
using System.Linq;
using Akzin.Crm.EarlyBoundGenerator.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Akzin.Crm.EarlyBoundGenerator.ObjectModel
{
    class Attribute
    {
        private readonly AttributeMetadata attributeMetadata;

        public Attribute(AttributeMetadata attributeMetadata)
        {
            this.attributeMetadata = attributeMetadata;

        }

        public string LogicalName => attributeMetadata.LogicalName;

        private string GetLabel(Label label, string defaultValue = null)
        {
            var result = label.LocalizedLabels
                .OrderBy(x => x.LanguageCode == 1043 ? 1 : 2) // give priority to dutch labels. 1043 == Dutch
                .Select(x => x.Label).FirstOrDefault();
            return result ?? defaultValue;
        }

        public string TypeName
        {
            // Rationale: No simpler alternative for converting types. Also, this code is part of a code generator tool.
#pragma warning disable S1541 // Methods and properties should not be too complex
            get
#pragma warning restore S1541 // Methods and properties should not be too complex
            {
                switch (attributeMetadata.AttributeType)
                {
                    case AttributeTypeCode.Uniqueidentifier:
                        return "Guid";
                    case AttributeTypeCode.Integer:
                        return "int";
                    case AttributeTypeCode.BigInt:
                        return "long";
                    case AttributeTypeCode.Double:
                        return "double";
                    case AttributeTypeCode.Memo:
                    case AttributeTypeCode.String:
                        return "string";
                    case AttributeTypeCode.DateTime:
                        return "DateTime";
                    case AttributeTypeCode.Decimal:
                        return "decimal";
                    case AttributeTypeCode.Boolean:
                        return "bool";
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner: // systemuser
                    case AttributeTypeCode.Customer: // account/contact
                    case AttributeTypeCode.PartyList: // account/contact
                        return "EntityReference";
                    case AttributeTypeCode.Picklist:
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                        return $"{LogicalName.ToPascalCase()}Enum";
                    case AttributeTypeCode.Virtual:
                        return "System.String";
                    case AttributeTypeCode.EntityName:
                        return "System.String";
                    case AttributeTypeCode.Money:
                        return "Money";
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public string TypeNameNullable
        {
            // Rationale: No simpler alternative for converting types. Also, this code is part of a code generator tool.
#pragma warning disable S1541 // Methods and properties should not be too complex
            get
#pragma warning restore S1541 // Methods and properties should not be too complex
            {
                switch (attributeMetadata.AttributeType)
                {
                    case AttributeTypeCode.Uniqueidentifier:
                    case AttributeTypeCode.Integer:
                    case AttributeTypeCode.BigInt:
                    case AttributeTypeCode.DateTime:
                    case AttributeTypeCode.Boolean:
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                    case AttributeTypeCode.Double:
                    case AttributeTypeCode.Decimal:
                    case AttributeTypeCode.Picklist:
                        return TypeName + "?";
                    case AttributeTypeCode.Money:
                    case AttributeTypeCode.Memo:
                    case AttributeTypeCode.String:
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner: // systemuser
                    case AttributeTypeCode.Customer: // account/contact
                    case AttributeTypeCode.Virtual:
                    case AttributeTypeCode.EntityName:
                    case AttributeTypeCode.PartyList:
                        return TypeName;
                    case AttributeTypeCode.ManagedProperty:
                        if (attributeMetadata is ManagedPropertyAttributeMetadata metadata)
                        {
                            switch (metadata.ValueAttributeTypeCode)
                            {
                                case AttributeTypeCode.Boolean:
                                    return "bool?";
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public bool IsEnum
        {
            get
            {
                switch (attributeMetadata.AttributeType)
                {
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                    case AttributeTypeCode.Picklist:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsMoney => attributeMetadata.AttributeType == AttributeTypeCode.Money;

        public Dictionary<int, string> Options
        {
            get
            {
                if (attributeMetadata is EnumAttributeMetadata enumAttributeMetadata)
                {
                    var q = from item in enumAttributeMetadata.OptionSet.Options
                            where item.Value != null
                            let id = (int)item.Value
                            let label = GetLabel(item.Label)
                            select new
                            {
                                Id = id,
                                Label = label
                            };

                    var dict = q.ToDictionary(x => x.Id, x => x.Label);
                    return dict;
                }
                else
                {
                    return new Dictionary<int, string>();
                }
            }
        }
    }
}