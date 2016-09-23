using System;
using System.Configuration;

namespace Trurl.Configuration
{
    public class Configuration
    {
        public static string GetConfigurationValue(string name, bool required = true)
        {
            var env = Environment.GetEnvironmentVariable(name);
            var config = ConfigurationManager.AppSettings.Get(name);
            var value = env ?? config;
            if (required && string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException($"'{ name }' is unspecified");
            }

            return value;
        }
    }
}
