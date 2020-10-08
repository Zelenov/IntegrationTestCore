using System;
using System.IO;
using System.Linq;
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
        public static IConfigurationBuilder Remove(this IConfigurationBuilder builder, Type sourceType)
        {
            var sources = builder.Sources.Where(s => s.GetType() == sourceType).ToArray();
            foreach (var source in sources)
            {
                builder.Sources.Remove(source);
            }
            return builder;
        }
        public static IConfigurationBuilder RemoveWhere(this IConfigurationBuilder builder, Func<IConfigurationSource, bool> filter)
        {
            var sources = builder.Sources.Where(filter).ToArray();
            foreach (var source in sources)
            {
                builder.Sources.Remove(source);
            }
            return builder;
        }
    }
}