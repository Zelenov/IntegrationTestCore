using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace IntegrationTestCore
{
    public class WebApplicationFactoryHelper<TStartup>: WebApplicationFactory<TStartup> where TStartup : class
    {
        protected readonly TestCore TestCore;

        public WebApplicationFactoryHelper(TestCore testCore)
        {
            TestCore = testCore;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config => TestCore.ConfigureAppConfiguration(config));
            builder.ConfigureTestServices(services =>
            {
                TestCore.ConfigureTestServices(services);
            });
            builder.ConfigureLogging(services =>
            {
                TestCore.ConfigureLogging(services);
            });
        }
    }
}
