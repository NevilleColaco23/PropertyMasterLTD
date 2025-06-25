
using Microsoft.Extensions.Configuration;

namespace MyWarehouse.Infrastructure
{
    /// <summary>
    /// Class containing helper functions to read from config files.
    /// </summary>
    public static class Config
    {
        private static IConfiguration _configuration;

        public static void Init(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Read a value specified by keyName and returns it after converting it to boolean.
        /// </summary>
        /// <param name="keyName">The settings key whose value to return</param>
        /// <param name="sectionName">Optional, section Name</param>
        /// <returns>False if value not found, else the value converted to boolean</returns>
        public static bool GetValueBool(string keyName, string sectionName = "AppSettings")
        {
            string value = GetValueString(keyName, sectionName);

            return value != null && Convert.ToBoolean(value);
        }

        /// <summary>
        /// Read a value specified by keyName and returns it after converting it to Int32.
        /// </summary>
        /// <param name="keyName">The settings key whose value to return</param>
        /// <param name="sectionName">Optional, section Name</param>
        /// <param name="valueIfNull">The value to return if the specified key is not found.</param>
        /// <returns>Setting value if found, else the value given by valueIfNull parameter.</returns>
        public static int GetValueInt32(string keyName, int valueIfNull = 0, string sectionName = "AppSettings")
        {
            string value = GetValueString(keyName, sectionName);

            return value == null ? valueIfNull : Convert.ToInt32(value);
        }

        /// <summary>
        /// Read a value specified by keyName and returns it after converting it to Float.
        /// </summary>
        /// <param name="keyName">The settings key whose value to return</param>
        /// <param name="sectionName">Optional, section Name</param>
        /// <param name="valueIfNull">The value to return if the specified key is not found.</param>
        /// <returns>Setting value if found, else the value given by valueIfNull parameter.</returns>
        public static float GetValueFloat(string keyName, float valueIfNull = 0, string sectionName = "AppSettings")
        {
            string value = GetValueString(keyName, sectionName);

            return value == null ? valueIfNull : (float)Convert.ToDouble(value);
        }

        /// <summary>
        /// Read a value specified by keyName.
        /// </summary>
        /// <param name="keyName">The settings key whose value to return</param>
        /// <param name="sectionName">Optional, section Name</param>
        /// <returns>Setting value if found, else null</returns>
        public static string GetValueString(string keyName, string sectionName = "AppSettings")
        {
            return sectionName == null ? _configuration[keyName] : _configuration.GetSection(sectionName)?[keyName];
        }

        /// <summary>
        /// Reads the default database connection string specified in the configuration file.
        /// </summary>
        public static string DefaultDbConnectionString => _configuration.GetConnectionString(GetValueString("DefaultDb"));

        public static string PricingDbConnectionString => _configuration.GetConnectionString(GetValueString("PricingDb"));

        public static string MongoDbConnectionString => _configuration.GetConnectionString(GetValueString("MongoDb"));

        public static string CacheConnectionString => _configuration.GetConnectionString(GetValueString("Redis"));

        public static T GetOptions<T>(string sectionName)
        {
            return _configuration.GetSection(sectionName).Get<T>();
        }
    }
}
