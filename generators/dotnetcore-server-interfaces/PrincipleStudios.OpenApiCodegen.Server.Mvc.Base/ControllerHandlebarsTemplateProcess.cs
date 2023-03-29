using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PrincipleStudios.OpenApi.CSharp
{
    public static class ControllerHandlebarsTemplateProcess
    {
        public static IHandlebars CreateHandlebars()
        {
            var result = HandlebarsTemplateProcess.CreateHandlebars();

            foreach (var resourceName in typeof(ControllerHandlebarsTemplateProcess).Assembly.GetManifestResourceNames().Where(n => n.EndsWith(".handlebars")))
                result.AddTemplate(typeof(ControllerHandlebarsTemplateProcess).Assembly, resourceName);

            return result;
        }

        public static string ProcessController(this IHandlebars handlebars, Templates.ControllerTemplate controllerTemplate)
        {
            var template = handlebars.Configuration.RegisteredTemplates["controller"];

            using var sr = new StringWriter();
            var dict = HandlebarsTemplateProcess.ToDictionary<Templates.ControllerTemplate>(controllerTemplate);
            template(sr, dict);
            return sr.ToString();
        }

        public static string ProcessAddServices(this IHandlebars handlebars, Templates.AddServicesModel addServices)
        {
            var template = handlebars.Configuration.RegisteredTemplates["addServices"];

            using var sr = new StringWriter();
            var dict = HandlebarsTemplateProcess.ToDictionary<Templates.AddServicesModel>(addServices);
            template(sr, dict);
            return sr.ToString();
        }
    }
}
