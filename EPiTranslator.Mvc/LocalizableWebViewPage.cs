using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.ServiceLocation;

namespace EPiTranslator.Mvc
{
    /// <summary>
    /// Represents the properties and methods that are needed in order to render a view that uses
    /// ASP.NET Razor syntax. Supports localizable strings.
    /// </summary>
    /// <typeparam name="TModel">The type of the view model.</typeparam>
    public abstract class LocalizableWebViewPage<TModel> : WebViewPage<TModel>
    {
        /// <summary>
        /// Gets or sets the translation service.
        /// </summary>
        /// <value>The translation service.</value>
        public virtual Injected<ITranslator> Translator { get; set; }

        /// <summary>
        /// Returns translated text by its key.
        /// </summary>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text if translation was not found.</param>
        /// <param name="args">Format arguments for the translated text.</param>
        /// <remarks>Uses current thread culture language to find translations.</remarks>
        public virtual string L(string key, string fallback = null, object[] args = null)
        {
            return Translator.Service.Text(key, fallback, args);
        }

        /// <summary>
        /// Returns translated text by its key, in specific language.
        /// </summary>
        /// <param name="language">The language for translation.</param>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text if translation was not found.</param>
        /// <param name="args">Format arguments for the translated text.</param>
        public virtual string LIn(string language, string key, string fallback = null, object[] args = null)
        {
            return Translator.Service.TextIn(language, key, fallback, args);
        }
    }

    /// <summary>
    /// Represents the properties and methods that are needed in order to render a view that uses
    /// ASP.NET Razor syntax. Supports localizable strings.
    /// </summary>
    public abstract class LocalizableWebViewPage : WebViewPage
    {
        /// <summary>
        /// Gets or sets the translation service.
        /// </summary>
        /// <value>The translation service.</value>
        public virtual Injected<ITranslator> Translator { get; set; }

        /// <summary>
        /// Returns translated text by its key.
        /// </summary>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text if translation was not found.</param>
        /// <param name="args">Format arguments for the translated text.</param>
        /// <remarks>Uses current thread culture language to find translations.</remarks>
        public virtual string L(string key, string fallback = null, object[] args = null)
        {
            return Translator.Service.Text(key, fallback, args);
        }

        /// <summary>
        /// Returns translated text by its key, in specific language.
        /// </summary>
        /// <param name="language">The language for translation.</param>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text if translation was not found.</param>
        /// <param name="args">Format arguments for the translated text.</param>
        public virtual string LIn(string language, string key, string fallback = null, object[] args = null)
        {
            return Translator.Service.TextIn(language, key, fallback, args);
        }
    }
}
