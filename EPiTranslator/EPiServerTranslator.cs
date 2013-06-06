using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using EPiServer.Framework.Localization;
using Perks;
using Perks.Data;
using Perks.Data.Xml;

namespace EPiTranslator
{
    /// <summary>
    /// Implementation of <see cref="ITranslator"/> service, using EPiServer localization services to retrieve translations.
    /// </summary>
    public class EPiServerTranslator : ITranslator
    {
        // TODO: should be configurable
        public const string TranslationsFileName = "{0}_website.xml";

        private readonly LocalizationService _localizer;
        private readonly HttpContextBase _httpContext;
        private readonly IFileStorage _storage;
        private readonly XmlService _xml;

        /// <summary>
        /// Initializes a new instance of the <see cref="EPiServerTranslator"/> class.
        /// </summary>
        public EPiServerTranslator(LocalizationService localizer, HttpContextBase httpContext, IFileStorage storage, XmlService xml)
        {
            Ensure.ArgumentNotNull(localizer, "localizer");
            Ensure.ArgumentNotNull(httpContext, "httpContext");
            Ensure.ArgumentNotNull(storage, "storage");
            Ensure.ArgumentNotNull(xml, "xml");

            _localizer = localizer;
            _httpContext = httpContext;
            _storage = storage;
            _xml = xml;
        }

        /// <summary>
        /// Gets the current language configured for retrieving translations.
        /// </summary>
        public string CurrentLanguage
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name; }
        }

        /// <summary>
        /// Returns translated text by its key.
        /// </summary>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text if translation was not found.</param>
        /// <param name="args">Format arguments for the translated text.</param>
        /// <remarks>Uses current thread culture language to find translations.</remarks>
        public virtual string Text(string key, string fallback = null, object[] args = null)
        {
            return Text(CurrentLanguage, key, fallback, args);
        }

        /// <summary>
        /// Returns translated text by its key, in specific language.
        /// </summary>
        /// <param name="language">The language for translation.</param>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text if translation was not found.</param>
        /// <param name="args">Format arguments for the translated text.</param>
        public virtual string TextIn(string language, string key, string fallback = null, object[] args = null)
        {
            return Text(language, key, fallback, args);
        }

        private string Text(string language, string key, string fallback, object[] args)
        {
            Ensure.ArgumentNotNullOrEmpty(language, "language");
            Ensure.ArgumentNotNullOrEmpty(key, "key");

            // TODO: this is for standard keys pattern (EPiServer keys start from '/', in Sitecore - not).
            key = key.StartsWith("/") ? key : "/" + key;

            var translated = _localizer.GetStringByCulture(key, fallback: null, culture: new CultureInfo(language));

            if (translated == null)
            {
                if (fallback == null)
                {
                    fallback = "[" + key + "]";
                }

                // TODO: hardcoded 'en' fallback language
                if (language == "en")
                {
                    AddFallbackTranslation(language, key, fallback);
                    translated = fallback;
                }
                else
                {
                    return Text("en", key, fallback, args);
                }
            }

            if (args != null)
            {
                return FormatTextSafe(language, translated, args);
            }

            return translated;
        }

        private string FormatTextSafe(string language, string text, object[] args)
        {
            try
            {
                return string.Format(text, args);
            }
            catch (FormatException ex)
            {
                return string.Format("[Missing format args for '{0}' text in '{1}' language]", text, language);
            }
        }

        /// <summary>
        /// Saves the fallback translation text in the storage.
        /// </summary>
        /// <param name="language">The language of translation fallback.</param>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text to save.</param>
        protected virtual void AddFallbackTranslation(string language, string key, string fallback)
        {
            // Open language file. // TODO: '~/lang/' folder should be configurable
            var physicalPath = _httpContext.Server.MapPath("~/lang/" + string.Format(TranslationsFileName, language));

            // If language file isn't exists, create it.
            if (!_storage.FileExists(physicalPath))
            {
                CreateLanguageFile(language, physicalPath);
            }

            var doc = _xml.Load(physicalPath);

            // Extract XML element names.
            var keyPathElements = key.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var lastPathElementName = keyPathElements.Last();
            var currentNode = doc.Root.Element("language");
            var fallbackInserted = false;

            // If not EPiServer language file structure - throw exception.
            if (currentNode == null)
            {
                throw new ArgumentException(string.Format("'{0}' is not a language file.", physicalPath));
            }

            // For each XML element in search key.
            foreach (var pathElementName in keyPathElements)
            {
                // Try to get element by name.
                var element = currentNode.Element(pathElementName);

                // If not found, add it.
                if (element == null)
                {
                    var children = currentNode.Elements();
                    XElement elementAfter = null;

                    // Try to find the alphabetical place for the element.
                    foreach (var child in children)
                    {
                        elementAfter = child;

                        if (child.Name.ToString().CompareTo(pathElementName) > 0)
                        {
                            break;
                        }
                    }

                    // If there are some children.
                    if (elementAfter != null)
                    {
                        // Add the element in alphabetical order.
                        if (elementAfter.Name.ToString().CompareTo(pathElementName) > 0)
                        {
                            elementAfter.AddBeforeSelf(new XElement(pathElementName));
                        }
                        else
                        {
                            elementAfter.AddAfterSelf(new XElement(pathElementName));
                        }
                    }
                    else
                    {
                        // Otherwise, just add the element.
                        currentNode.Add(new XElement(pathElementName));
                    }

                    currentNode = currentNode.Element(pathElementName);
                }
                else
                {
                    // If last element found and we don't want to override existing translations - skip adding fallback to it.
                    if (pathElementName == lastPathElementName)
                    {
                        continue;
                    }

                    currentNode = element;
                }

                // If it is a last element of the path, insert a text and break the cycle.
                if (currentNode.Name == lastPathElementName)
                {
                    currentNode.Value = fallback;
                    fallbackInserted = true;
                    break;
                }
            }

            // Save results.
            if (fallbackInserted)
            {
                var file = new FileInfo(physicalPath);

                if (file.IsReadOnly)
                {
                    file.IsReadOnly = false;
                }

                // TODO: this could throw a 'file is busy' exceptions sometimes..
                _xml.Save(doc, physicalPath);
            }
        }

        private void CreateLanguageFile(string language, string physicalPath)
        {
            var doc = new XDocument(
                new XDeclaration(version: "1.0", encoding: "utf-8", standalone: "yes"),
                new XElement("languages",
                    new XElement("language",
                        new XAttribute("name", new CultureInfo(language).EnglishName),
                        new XAttribute("id", language.ToLower())))
                );

            _storage.CreateDirectory(Path.GetDirectoryName(physicalPath));
            _xml.Save(doc, physicalPath);
        }
    }
}
