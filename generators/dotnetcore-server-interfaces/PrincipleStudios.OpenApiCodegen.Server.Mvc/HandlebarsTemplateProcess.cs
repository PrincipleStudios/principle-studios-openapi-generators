using HandlebarsDotNet;
using PrincipleStudios.OpenApiCodegen.Server.Mvc.templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public static class HandlebarsTemplateProcess
    {
        public static IHandlebars CreateHandlebars()
        {
            var result = Handlebars.Create();

            foreach (var resourceName in typeof(HandlebarsTemplateProcess).Assembly.GetManifestResourceNames().Where(n => n.EndsWith(".handlebars")))
                AddTemplate(resourceName, result);

            return result;
        }

        private static void AddTemplate(string resourceName, IHandlebars result)
        {
            using var stream = typeof(HandlebarsTemplateProcess).Assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var templateName = Path.GetFileNameWithoutExtension(resourceName).Split('.').Last();
            result.RegisterTemplate(templateName: templateName, template: reader.ReadToEnd());
        }

        public static string ProcessController(ControllerTemplate controllerTemplate, IHandlebars? handlebars = null)
        {
            handlebars ??= CreateHandlebars();
            var template = handlebars.Configuration.RegisteredTemplates["controller"];

            using var sr = new StringWriter();
            var dict = ToDictionary<templates.ControllerTemplate>(controllerTemplate);
            template(sr, dict);
            return sr.ToString();
        }

        public static string ProcessModel(
            PartialHeader header,
            string packageName,
            Model model,
            IHandlebars? handlebars = null
        ) {
            handlebars ??= CreateHandlebars();
            var (templateName, dict) = model switch
            {
                ObjectModel m => ("objectmodel", ToTemplate(m)),
                _ => throw new NotImplementedException()
            };
            var template = handlebars.Configuration.RegisteredTemplates[templateName];

            using var sr = new StringWriter();
            template(sr, dict);
            return sr.ToString();

            IDictionary<string, object?> ToTemplate<TModel>(TModel m)
                where TModel : Model
            {
                return ToDictionary<ModelTemplate<TModel>>(new (header: header, packageName: packageName, model: m));
            }
        }

        private static IDictionary<string, object?> ToDictionary<T>(T model)
        {
            var serialized = JsonSerializer.Serialize(model, typeof(T));
            var result = JsonSerializer.Deserialize<JsonElement>(serialized)!;

            return (IDictionary<string, object?>)FromElement(result)!;
        }

        private static object? FromElement(JsonElement result)
        {
            return result switch
            {
                { ValueKind: JsonValueKind.Undefined } => null,
                { ValueKind: JsonValueKind.Null } => null,
                { ValueKind: JsonValueKind.False } => false,
                { ValueKind: JsonValueKind.True } => true,
                { ValueKind: JsonValueKind.Number } => result.GetDouble(),
                { ValueKind: JsonValueKind.String } => result.GetString(),
                { ValueKind: JsonValueKind.Array } => (from item in result.EnumerateArray()
                                                       select FromElement(item)).ToArray(),
                { ValueKind: JsonValueKind.Object } => (from prop in result.EnumerateObject()
                                                        let Value = FromElement(prop.Value)
                                                        where Value != null
                                                        select (prop.Name, Value)).ToDictionary(kvp => kvp.Name, kvp => kvp.Value),
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
