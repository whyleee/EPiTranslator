using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Perks;

namespace EPiTranslator.Mvc
{
    public class LocalizableDataAnnotationsModelValidator : ModelValidator
    {
        private readonly ModelValidator _innerValidator;
        private readonly ITranslator _translator;

        public LocalizableDataAnnotationsModelValidator(ModelValidator innerValidator, ModelMetadata metadata,
            ControllerContext context, ITranslator translator) : base(metadata, context)
        {
            _innerValidator = innerValidator;
            _translator = translator;
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return _innerValidator.GetClientValidationRules()
                .Select(rule =>
                    {
                        rule.ErrorMessage = GetTranslation(rule.ErrorMessage);
                        return rule;
                    })
                .ToList();
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            return _innerValidator.Validate(container)
                .Select(result =>
                    {
                        result.Message = GetTranslation(result.Message);
                        return result;
                    })
                .ToList();
        }

        private string GetTranslation(string text)
        {
            if (text != null && !text.StartsWith("$"))
            {
                return text;
            }

            if (_innerValidator.IsRequired)
            {
                // TODO: this is a hardcode
                if (text.IsNullOrEmpty() || (!text.StartsWith("$Errors/") && !text.StartsWith("$Forms/")))
                {
                    return _translator.Text("Errors/Required", "{0} is required", new object[] {Metadata.DisplayName});
                }
            }

            // TODO: duplicate with 'LocalizableModelMetadataProvider', refactor and reuse
            if (text.IsNullOrEmpty())
            {
                return text;
            }

            var parts = text.Substring(1).Split('|');

            if (parts.Length == 1)
            {
                return _translator.Text(parts.First());
            }
            else if (parts.Length == 2)
            {
                return _translator.Text(parts.First(), parts.Last());
            }

            return text;
        }
    }
}