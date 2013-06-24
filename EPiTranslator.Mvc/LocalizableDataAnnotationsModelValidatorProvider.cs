using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiTranslator.Mvc
{
    public class LocalizableDataAnnotationsModelValidatorProvider : DataAnnotationsModelValidatorProvider
    {
        private readonly ITranslator _translator;

        public LocalizableDataAnnotationsModelValidatorProvider(ITranslator translator)
        {
            _translator = translator;
        }

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata,
            ControllerContext context, IEnumerable<Attribute> attributes)
        {
            var validators = base.GetValidators(metadata, context, attributes)
                .Select(x => new LocalizableDataAnnotationsModelValidator(x, metadata, context, _translator))
                .ToList();

            return validators;
        }
    }
}