using Akzin.Crm.EarlyBoundGenerator.Helpers;
using Akzin.Crm.EarlyBoundGenerator.ObjectModel;

namespace Akzin.Crm.EarlyBoundGenerator.CodeGenerators
{
    class Generator
    {
        private readonly Model model;
        public Generator(Model model)
        {
            this.model = model;

        }

        public string Generate()
        {
            var writer = new Writer();

            writer.AppendLine("using System;");
            writer.AppendLine("using System.CodeDom.Compiler;");
            writer.AppendLine("using System.ComponentModel;");
            writer.AppendLine("using System.Diagnostics;");
            writer.AppendLine("using System.Diagnostics.CodeAnalysis;");
            writer.AppendLine("using System.Runtime.Serialization;");
            writer.AppendLine("using Microsoft.Xrm.Sdk;");
            writer.AppendLine("using Microsoft.Xrm.Sdk.Client;");
            writer.AppendLine();
            writer.AppendLine("// ReSharper disable All");
            writer.AppendLine();
            writer.AppendLine($"namespace {model.Namespace}");
            writer.AppendLine("{");
            writer.TabPush();

            writer.AppendLine("[DataContract]");
            writer.AppendLine("[ExcludeFromCodeCoverage]");
            writer.AppendLine($@"[EntityLogicalName(""{model.LogicalName}"")]");
            writer.AppendLine(@"[GeneratedCode(""Akzin.Crm.EarlyBoundGenerator"", ""1.0"")]");
            writer.AppendLine($@"[DisplayName(""{model.DisplayName}"")]");
            writer.AppendLine($@"[DebuggerDisplay(""{model.LogicalName} {{Id}} {{{model.Name?.LogicalName.ToPascalCase()}}}"")]");
            writer.AppendLine($@"public partial class {model.LogicalName.ToPascalCase()} : Entity");
            writer.AppendLine("{");
            writer.TabPush();

            writer.AppendLine();
            writer.AppendLine($@"public const string EntityLogicalName = ""{model.LogicalName}"";");

            writer.AppendLine();
            writer.AppendLine($@"public const string DisplayName = ""{model.DisplayName}"";");

            writer.AppendLine();
            writer.AppendLine($@"public {model.LogicalName.ToPascalCase()}() :  base(EntityLogicalName) {{ }}");

            WriteIdAttribute(writer);
            foreach (var attribute in model.Attributes)
            {
                WriteAttribute(writer, attribute);
            }

            writer.TabPop();
            writer.AppendLine("}");

            writer.TabPop();
            writer.AppendLine("}");
            return writer.ToString();
        }

        private static void WriteAttribute(Writer writer, Attribute attribute)
        {
            writer.AppendLine();
            writer.AppendLine($@"public const string {attribute.LogicalName.ToPascalCase()}Field = ""{attribute.LogicalName}"";");

            writer.AppendLine();
            writer.AppendLine($@"[AttributeLogicalName(""{attribute.LogicalName}"")]");

            if (attribute.IsEnum)
            {
                writer.AppendLine($@"public {attribute.TypeNameNullable} {attribute.LogicalName.ToPascalCase()}");
                writer.AppendLine($@"{{");
                writer.TabPush();

                writer.AppendLine($@"get");
                writer.AppendLine($@"{{");
                writer.TabPush();
                writer.AppendLine($@"var optionSet = GetAttributeValue<OptionSetValue>(""{attribute.LogicalName}"");");
                writer.AppendLine($@"if (optionSet != null)");
                writer.TabPush();
                writer.AppendLine($@"return ({attribute.TypeName})Enum.ToObject(typeof({attribute.TypeName}), optionSet.Value);");
                writer.TabPop();
                writer.AppendLine($@"return null;");
                writer.TabPop();
                writer.AppendLine($@"}}");

                writer.AppendLine($@"set");
                writer.AppendLine($@"{{");
                writer.TabPush();

                writer.AppendLine($@"if (value == null)");
                writer.TabPush();
                writer.AppendLine($@"SetAttributeValue(""{attribute.LogicalName}"", null);");
                writer.TabPop();
                writer.AppendLine($@"else");
                writer.TabPush();
                writer.AppendLine($@"SetAttributeValue(""{attribute.LogicalName}"", new OptionSetValue((int)value));");
                writer.TabPop();

                writer.TabPop();
                writer.AppendLine($@"}}");

                writer.TabPop();
                writer.AppendLine($@"}}");

                // enum
                writer.AppendLine();
                writer.AppendLine("[DataContract]");
                writer.AppendLine(@"[GeneratedCode(""Akzin.Crm.EarlyBoundGenerator"", ""1.0"")]");
                writer.AppendLine($@"public enum {attribute.LogicalName.ToPascalCase()}Enum");
                writer.AppendLine("{");
                writer.TabPush();
                foreach (var option in attribute.Options)
                {
                    writer.AppendLine($@"[EnumMember] {option.Value.ToPascalCase()} = {option.Key},");
                }
                writer.TabPop();
                writer.AppendLine("}");
            }
            else if (attribute.IsMoney)
            {
                writer.AppendLine($@"public decimal? {attribute.LogicalName.ToPascalCase()}");
                writer.AppendLine($@"{{");
                writer.TabPush();

                writer.AppendLine($@"get => GetAttributeValue<{attribute.TypeNameNullable}>(""{attribute.LogicalName}"")?.Value;");
                writer.AppendLine($@"set => SetAttributeValue(""{attribute.LogicalName}"", value == null ? null : new Money(value.Value));");

                writer.TabPop();
                writer.AppendLine($@"}}");
            }
            else
            {
                writer.AppendLine($@"public {attribute.TypeNameNullable} {attribute.LogicalName.ToPascalCase()}");
                writer.AppendLine($@"{{");
                writer.TabPush();

                writer.AppendLine($@"get => GetAttributeValue<{attribute.TypeNameNullable}>(""{attribute.LogicalName}"");");
                writer.AppendLine($@"set => SetAttributeValue(""{attribute.LogicalName}"", value);");

                writer.TabPop();
                writer.AppendLine($@"}}");
            }
        }

        private void WriteIdAttribute(Writer writer)
        {
            writer.AppendLine();
            writer.AppendLine($@"public const string IdField = ""{model.Id.LogicalName}"";");

            // ID
            writer.AppendLine();
            writer.AppendLine($@"[AttributeLogicalName(""{model.Id.LogicalName}"")]");
            writer.AppendLine($@"public override Guid Id");
            writer.AppendLine($@"{{");
            writer.TabPush();
            writer.AppendLine($@"get => base.Id;");
            writer.AppendLine($@"set => {model.Id.LogicalName.ToPascalCase()} = value;");
            writer.TabPop();
            writer.AppendLine($@"}}");

            writer.AppendLine();
            writer.AppendLine($@"public const string {model.Id.LogicalName.ToPascalCase()}Field = ""{model.Id.LogicalName}"";");

            writer.AppendLine();
            writer.AppendLine($@"[AttributeLogicalName(""{model.Id.LogicalName}"")]");
            writer.AppendLine($@"public Guid? {model.Id.LogicalName.ToPascalCase()}");
            writer.AppendLine($@"{{");
            writer.TabPush();
            writer.AppendLine($@"get => GetAttributeValue<Guid?>(""{model.Id.LogicalName}"");");
            writer.AppendLine($@"set");
            writer.AppendLine($@"{{");
            writer.TabPush();
            writer.AppendLine($@"SetAttributeValue(""{model.Id.LogicalName}"", value);");
            writer.AppendLine($@"base.Id = value ?? Guid.Empty;");
            writer.TabPop();
            writer.AppendLine($@"}}");
            writer.TabPop();
            writer.AppendLine($@"}}");
        }
    }
}