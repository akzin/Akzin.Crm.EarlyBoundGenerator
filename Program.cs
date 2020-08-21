using Akzin.Crm.EarlyBoundGenerator.CodeGenerators;
using Akzin.Crm.EarlyBoundGenerator.Helpers;
using Akzin.Crm.EarlyBoundGenerator.ObjectModel;
using CommandLine;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Akzin.Crm.EarlyBoundGenerator
{
    class Program
    {
        private static void Init()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        static void Main(string[] args)
        {
            Init();
            Run(args);
        }
        private static void Run(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<GenerateOptions>(args)
                    .WithParsed<GenerateOptions>(Generate);

            if (parseResult.Tag == ParserResultType.NotParsed)
            {
                //Exit with error code when input arguments are invalid
                Environment.Exit(-1);
            }
        }

        private static void Generate(GenerateOptions options)
        {
            var client = new CrmServiceClient(options.CrmConnectionString);

            foreach (var entityName in options.EntitiesList)
            {
                var entityMetadata = ((RetrieveEntityResponse)client.Execute(
                    new RetrieveEntityRequest { EntityFilters = EntityFilters.All, LogicalName = entityName })).EntityMetadata;

                var model = new Model(entityMetadata, options.Namespace);
                var generator = new Generator(model);
                options.Directory.Create();
                var outputFile = new FileInfo(Path.Combine(options.Directory.FullName, $"{model.LogicalName.ToPascalCase()}.Generated.cs"));
                using var streamWriter = outputFile.CreateText();
                generator.Generate(streamWriter);
            }
        }

        [Verb("generate", HelpText = "Generates early-bound c# classes")]
        public class GenerateOptions
        {
            [Option(shortName: 'c', longName: "crm", Required = true, HelpText = "CRM Connectionstring: AuthType=Office365;Url=http://contoso:8080/Test;UserName=jsmith@contoso.onmicrosoft.com;Password=passcode")]
            public string CrmConnectionString { get; set; }

            [Option(shortName: 'e', longName: "entities", Required = true)]
            public string Entities { get; set; }

            public string[] EntitiesList => Entities.Split(',', ';');

            [Option(shortName: 'd', longName: "directory", Required = true)]
            public DirectoryInfo Directory { get; set; }

            [Option(shortName: 'n', longName: "namespace", Required = true)]
            public string Namespace { get; set; }
        }
    }
}
