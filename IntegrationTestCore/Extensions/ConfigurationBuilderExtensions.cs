using System.IO;
using Microsoft.Extensions.Configuration;

namespace IntegrationTestCore
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds .json file in <see cref="Directory.GetCurrentDirectory" folder/>
        /// </summary>
        public static IConfigurationBuilder WithLocalJsonFile(this IConfigurationBuilder config, string localFileName)
        {
            var fullFileName = Path.Combine(Directory.GetCurrentDirectory(), localFileName);
            if (!File.Exists(fullFileName))
                throw new FileNotFoundException($"file {fullFileName} not found");
            return config.AddJsonFile(fullFileName);
        }
    }
}