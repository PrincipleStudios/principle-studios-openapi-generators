using HandlebarsDotNet;
using PrincipleStudios.OpenApi.TypeScript.templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PrincipleStudios.OpenApi.TypeScript
{
    public static class HandlebarsTemplateProcess
    {
        public static IHandlebars CreateHandlebars()
        {
            var result = Handlebars.Create();

            result.RegisterHelper(
                "linewrap", 
                (context, parameters) => 
                    parameters[0] is string s 
                        ? s.ToString().Replace("\r", "").Replace("\n", parameters[1].ToString())
                        : parameters[0]
            );

            foreach (var resourceName in typeof(HandlebarsTemplateProcess).Assembly.GetManifestResourceNames().Where(n => n.EndsWith(".handlebars")))
                result.AddTemplate(typeof(HandlebarsTemplateProcess).Assembly, resourceName);

            return result;
        }

        public static void AddTemplate(this IHandlebars result, System.Reflection.Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var templateName = Path.GetFileNameWithoutExtension(resourceName).Split('.').Last();
            result.RegisterTemplate(templateName: templateName, template: reader.ReadToEnd());
        }

        public static string ProcessModel(
            PartialHeader header,
            string packageName,
            Model model,
            IHandlebars? handlebars = null
        )
        {
            handlebars ??= CreateHandlebars();
            var (templateName, dict) = model switch
            {
                ObjectModel m => ("objectmodel", ToTemplate(m)),
                EnumModel m => ("enummodel", ToTemplate(m)),
                _ => throw new NotImplementedException()
            };
            var template = handlebars.Configuration.RegisteredTemplates[templateName];

            using var sr = new StringWriter();
            template(sr, dict);
            return sr.ToString();

            IDictionary<string, object?> ToTemplate<TModel>(TModel m)
                where TModel : Model
            {
                return ToDictionary<ModelTemplate<TModel>>(new(header: header, packageName: packageName, model: m));
            }
        }

        public static IDictionary<string, object?> ToDictionary<T>(T model)
        {
            var result = model == null ? JValue.CreateNull() : JToken.FromObject(model);

            return (IDictionary<string, object?>)FromElement(result)!;
        }

        private static object? FromElement(JToken result)
        {
            return result switch
            {
                { Type: JTokenType.Undefined } => null,
                { Type: JTokenType.Null } => null,
                { Type: JTokenType.Boolean } => result.ToObject<bool>(),
                { Type: JTokenType.Float } => result.ToObject<double>(),
                { Type: JTokenType.String } => result.ToObject<string>(),
                JArray array => (from item in array
                                 select FromElement(item)).ToArray(),
                JObject obj => (from prop in obj.Properties()
                                let Value = FromElement(prop.Value)
                                where Value != null
                                select (prop.Name, Value)).ToDictionary(kvp => kvp.Name, kvp => kvp.Value),
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
