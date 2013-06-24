using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Perks;

namespace EPiTranslator.Mvc
{
    /// <summary>
    /// Provides localized model metadata.
    /// </summary>
    public class LocalizableModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        private readonly ITranslator _translator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableModelMetadataProvider" /> class.
        /// </summary>
        public LocalizableModelMetadataProvider(ITranslator translator)
        {
            Ensure.ArgumentNotNull(translator, "translator");

            _translator = translator;
        }

        /// <summary>
        /// Gets the metadata for the specified property.
        /// </summary>
        /// <param name="attributes">The attributes applied to the property.</param>
        /// <param name="containerType">The type of the container.</param>
        /// <param name="modelAccessor">Function to access the model.</param>
        /// <param name="modelType">The type of the model.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>
        /// Localized metadata for the property.
        /// </returns>
        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes,
            Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            var attributesWithoutDisplay = attributes.Where(a => a.GetType() != typeof(DisplayAttribute));
            var metadata = base.CreateMetadata(attributesWithoutDisplay, containerType, modelAccessor, modelType, propertyName);

            var display = attributes.OfType<DisplayAttribute>().FirstOrDefault();

            if (display != null)
            {
                LocalizeDisplayAttribute(display, metadata);
            }

            return metadata;
        }

        /// <summary>
        /// Localizes the display attribute metadata.
        /// </summary>
        /// <param name="display">The display attribute.</param>
        public virtual void LocalizeDisplayAttribute(DisplayAttribute display, ModelMetadata metadata)
        {
            if (display == null)
            {
                return;
            }

            metadata.DisplayName = GetTranslation(display.Name);
            metadata.Description = GetTranslation(display.Description);
            metadata.ShortDisplayName = GetTranslation(display.ShortName);
            metadata.Watermark = GetTranslation(display.Prompt);
            metadata.Order = display.GetOrder() ?? ModelMetadata.DefaultOrder;
        }

        private string GetTranslation(string text)
        {
            // TODO: duplicate with 'LocalizableDataAnnotationsModelValidator', refactor and reuse
            if (text.IsNullOrEmpty())
            {
                return text;
            }

            if (!text.StartsWith("$"))
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