using System;
using System.IO;
using Newtonsoft.Json;

namespace Blogfolio_CORE.Common.Services
{
    /// <summary>
    /// Json IO implementation of <see cref="ISettingsService" />
    /// </summary>
    public class JsonSettingsService : ISettingsService
    {
        /// <summary>
        /// De-serializes and returns the content of the
        /// specified json settings file
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="fileName"></param>
        /// <returns>A <see cref="TEntity" /> settings class</returns>
        public TEntity GetByName<TEntity>(string fileName) where TEntity : class
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var fileContent = File.ReadAllText(fileName);
            var json = JsonConvert.DeserializeObject<TEntity>(fileContent);

            return json;
        }

        /// <summary>
        /// Serializes and writes given values to specified
        /// json settings file
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="settings"></param>
        /// <param name="fileName"></param>
        public void Save<TEntity>(TEntity settings, string fileName) where TEntity : class
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            File.WriteAllText(fileName, json);
        }
    }
}