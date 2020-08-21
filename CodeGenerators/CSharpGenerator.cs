using System.CodeDom.Compiler;
using System.IO;
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

        public void Generate(TextWriter textWriter)
        {
            using var writer = new IndentedTextWriter(textWriter);

            writer.WriteLine("using System;");
            writer.WriteLine("using System.CodeDom.Compiler;");
            writer.WriteLine("using System.ComponentModel;");
            writer.WriteLine("using System.Diagnostics;");
            writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
            writer.WriteLine("using System.Runtime.Serialization;");
            writer.WriteLine("using Microsoft.Xrm.Sdk;");
            writer.WriteLine("using Microsoft.Xrm.Sdk.Client;");
            writer.WriteLineNoTabs("");
            writer.WriteLine("// ReSharper disable All");
            writer.WriteLineNoTabs("");
            writer.WriteLine($"namespace {model.Namespace}");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine("[DataContract]");
            writer.WriteLine("[ExcludeFromCodeCoverage]");
            writer.WriteLine($@"[EntityLogicalName(""{model.LogicalName}"")]");
            writer.WriteLine(@"[GeneratedCode(""Akzin.Crm.EarlyBoundGenerator"", ""1.0"")]");
            writer.WriteLine($@"[DisplayName(""{model.DisplayName}"")]");
            writer.WriteLine($@"[DebuggerDisplay(""{model.LogicalName} {{Id}} {{{model.Name?.LogicalName.ToPascalCase()}}}"")]");
            writer.WriteLine($@"public partial class {model.LogicalName.ToPascalCase()} : Entity");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLineNoTabs("");
            writer.WriteLine($@"public const string EntityLogicalName = ""{model.LogicalName}"";");

            writer.WriteLineNoTabs("");
            writer.WriteLine($@"public const string DisplayName = ""{model.DisplayName}"";");

            writer.WriteLineNoTabs("");
            writer.WriteLine($@"public {model.LogicalName.ToPascalCase()}() :  base(EntityLogicalName) {{ }}");

            WriteIdAttribute(writer);
            foreach (var attribute in model.Attributes)
            {
                WriteAttribute(writer, attribute);
            }

            writer.Indent--;
            writer.WriteLine("}");

            writer.Indent--;
            writer.WriteLine("}");
        }

        private static void WriteAttribute(IndentedTextWriter writer, Attribute attribute)
        {
            writer.WriteLineNoTabs("");
            writer.WriteLine($@"public const string {attribute.LogicalName.ToPascalCase()}Field = ""{attribute.LogicalName}"";");

            writer.WriteLineNoTabs("");
            writer.WriteLine($@"[AttributeLogicalName(""{attribute.LogicalName}"")]");

            if (attribute.IsEnum)
            {
                writer.WriteLine($@"public {attribute.TypeNameNullable} {attribute.LogicalName.ToPascalCase()}");
                writer.WriteLine($@"{{");
                writer.Indent++;

                writer.WriteLine($@"get");
                writer.WriteLine($@"{{");
                writer.Indent++;
                writer.WriteLine($@"var optionSet = GetAttributeValue<OptionSetValue>(""{attribute.LogicalName}"");");
                writer.WriteLine($@"if (optionSet != null)");
                writer.Indent++;
                writer.WriteLine($@"return ({attribute.TypeName})Enum.ToObject(typeof({attribute.TypeName}), optionSet.Value);");
                writer.Indent--;
                writer.WriteLine($@"return null;");
                writer.Indent--;
                writer.WriteLine($@"}}");

                writer.WriteLine($@"set");
                writer.WriteLine($@"{{");
                writer.Indent++;

                writer.WriteLine($@"if (value == null)");
                writer.Indent++;
                writer.WriteLine($@"SetAttributeValue(""{attribute.LogicalName}"", null);");
                writer.Indent--;
                writer.WriteLine($@"else");
                writer.Indent++;
                writer.WriteLine($@"SetAttributeValue(""{attribute.LogicalName}"", new OptionSetValue((int)value));");
                writer.Indent--;

                writer.Indent--;
                writer.WriteLine($@"}}");

                writer.Indent--;
                writer.WriteLine($@"}}");

                // enum
                writer.WriteLineNoTabs("");
                writer.WriteLine("[DataContract]");
                writer.WriteLine(@"[GeneratedCode(""Akzin.Crm.EarlyBoundGenerator"", ""1.0"")]");
                writer.WriteLine($@"public enum {attribute.LogicalName.ToPascalCase()}Enum");
                writer.WriteLine("{");
                writer.Indent++;
                foreach (var option in attribute.Options)
                {
                    writer.WriteLine($@"[EnumMember] {option.Value.ToPascalCase()} = {option.Key},");
                }
                writer.Indent--;
                writer.WriteLine("}");
            }
            else if (attribute.IsMoney)
            {
                writer.WriteLine($@"public decimal? {attribute.LogicalName.ToPascalCase()}");
                writer.WriteLine($@"{{");
                writer.Indent++;

                writer.WriteLine($@"get => GetAttributeValue<{attribute.TypeNameNullable}>(""{attribute.LogicalName}"")?.Value;");
                writer.WriteLine($@"set => SetAttributeValue(""{attribute.LogicalName}"", value == null ? null : new Money(value.Value));");

                writer.Indent--;
                writer.WriteLine($@"}}");
            }
            else
            {
                writer.WriteLine($@"public {attribute.TypeNameNullable} {attribute.LogicalName.ToPascalCase()}");
                writer.WriteLine($@"{{");
                writer.Indent++;

                writer.WriteLine($@"get => GetAttributeValue<{attribute.TypeNameNullable}>(""{attribute.LogicalName}"");");
                writer.WriteLine($@"set => SetAttributeValue(""{attribute.LogicalName}"", value);");

                writer.Indent--;
                writer.WriteLine($@"}}");
            }
        }

        private void WriteIdAttribute(IndentedTextWriter writer)
        {
            writer.WriteLineNoTabs("");
            writer.WriteLine($@"public const string IdField = ""{model.Id.LogicalName}"";");

            // ID
            writer.WriteLineNoTabs("");
            writer.WriteLine($@"[AttributeLogicalName(""{model.Id.LogicalName}"")]");
            writer.WriteLine($@"public override Guid Id");
            writer.WriteLine($@"{{");
            writer.Indent++;
            writer.WriteLine($@"get => base.Id;");
            writer.WriteLine($@"set => {model.Id.LogicalName.ToPascalCase()} = value;");
            writer.Indent--;
            writer.WriteLine($@"}}");

            writer.WriteLineNoTabs("");
            writer.WriteLine($@"public const string {model.Id.LogicalName.ToPascalCase()}Field = ""{model.Id.LogicalName}"";");

            writer.WriteLineNoTabs("");
            writer.WriteLine($@"[AttributeLogicalName(""{model.Id.LogicalName}"")]");
            writer.WriteLine($@"public Guid? {model.Id.LogicalName.ToPascalCase()}");
            writer.WriteLine($@"{{");
            writer.Indent++;
            writer.WriteLine($@"get => GetAttributeValue<Guid?>(""{model.Id.LogicalName}"");");
            writer.WriteLine($@"set");
            writer.WriteLine($@"{{");
            writer.Indent++;
            writer.WriteLine($@"SetAttributeValue(""{model.Id.LogicalName}"", value);");
            writer.WriteLine($@"base.Id = value ?? Guid.Empty;");
            writer.Indent--;
            writer.WriteLine($@"}}");
            writer.Indent--;
            writer.WriteLine($@"}}");
        }
    }
}