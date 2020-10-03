using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using IServiceCollection = Microsoft.Extensions.DependencyInjection.IServiceCollection;

namespace IntegrationTestCore
{
    public class TestCore<TStartup> : TestCore where TStartup : class
    {
        public virtual WebApplicationFactory<TStartup> Factory => new WebApplicationFactoryHelper<TStartup>(this);

        public TestCore(Action<IServiceCollection> testServices = null, Action<IConfigurationBuilder> config = null) : base(testServices, config)
        {
        }
        public virtual async Task RunAsync(ITestRunData data)
        {
            using var client = Factory.CreateClient();
            using IServiceScope scope = Factory.Server.Host.Services.CreateScope();
            await data.RunAsync(client, scope);
            await data.CheckAsync();
        }
    }


    public class TestCore
    {
        protected Action<IServiceCollection> TestServices;
        protected Action<IConfigurationBuilder> TestConfig;

        public TestCore(Action<IServiceCollection> testServices = null, Action<IConfigurationBuilder> config = null)
        {
            TestServices = testServices;
            TestConfig = config;
        }

        public void ConfigureTestServices(IServiceCollection services)
        {
            TestServices?.Invoke(services);
        }
        public virtual void ConfigureAppConfiguration(IConfigurationBuilder config)
        {
            TestConfig?.Invoke(config);
        }


    }
}