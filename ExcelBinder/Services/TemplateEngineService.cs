using System;
using System.Collections.Generic;
using Scriban;

namespace ExcelBinder.Services
{
    public class TemplateEngineService
    {
        public string Render(string templateContent, object context)
        {
            var template = Template.Parse(templateContent);
            if (template.HasErrors)
            {
                throw new Exception($"Template Error: {string.Join(", ", template.Messages)}");
            }
            return template.Render(context);
        }
    }
}
