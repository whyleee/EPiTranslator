namespace EPiTranslator
{
    /// <summary>
    /// The service for retrieving translated text, with support for fallbacks and format args.
    /// </summary>
    public interface ITranslator
    {
        /// <summary>
        /// Returns translated text by its key.
        /// </summary>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text if translation was not found.</param>
        /// <param name="args">Format arguments for the translated text.</param>
        /// <remarks>Uses current thread culture language to find translations.</remarks>
        string Text(string key, string fallback = null, object[] args = null);

        /// <summary>
        /// Returns translated text by its key, in specific language.
        /// </summary>
        /// <param name="language">The language for translation.</param>
        /// <param name="key">The key to find a translation.</param>
        /// <param name="fallback">Fallback text if translation was not found.</param>
        /// <param name="args">Format arguments for the translated text.</param>
        string TextIn(string language, string key, string fallback = null, object[] args = null);
    }
}