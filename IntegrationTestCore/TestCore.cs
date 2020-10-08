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

        public virtual async Task RunAsync(ITestRunData data)
        {
            using var client = Factory.CreateClient();
            using IServiceScope scope = Factory.Server.Host.Services.CreateScope();
            await data.RunAsync(client, scope);
            await data.CheckAsync();
        }
        public new TestCore<TStartup> WithTestServices(Action<IServiceCollection> action)
        {
            base.WithTestServices(action);
            return this;
        }
        public TestCore WithConfiguration(Action<IConfigurationBuilder> action)
        {
            base.WithConfiguration(action);
            return this;
        }
    }


    public class TestCore
    {
        protected readonly List<Action<IServiceCollection>> TestServices = new List<Action<IServiceCollection>>();
        protected readonly List<Action<IConfigurationBuilder>> TestConfig = new List<Action<IConfigurationBuilder>>();
        
        public TestCore WithTestServices(Action<IServiceCollection> action)
        {
            TestServices.Add(action);
            return this;
        }
        public TestCore WithConfiguration(Action<IConfigurationBuilder> action)
        {
            TestConfig.Add(action);
            return this;
        }
        internal void ConfigureTestServices(IServiceCollection services)
        {
            foreach (var testService in TestServices)
            {
                testService?.Invoke(services);
            }
        }
        internal void ConfigureAppConfiguration(IConfigurationBuilder config)
        {
            foreach (var testConfig in TestConfig)
            {
                testConfig?.Invoke(config);
            }
        }


    }
}