using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTestCore
{
    public abstract class ServiceRunData : ITestRunData
    {
        public abstract Task RunAsync(HttpClient client, IServiceScope scope);

        public virtual Task CheckAsync() => Task.CompletedTask;
    }

    public class ServiceRunData<T> : ServiceRunData
    {
        public Func<T, Task> Func;

        public override async Task RunAsync(HttpClient client, IServiceScope scope)
        {
            var p1 = (T)scope.ServiceProvider.GetService<T>();
            if (Func != null)
                await Func.Invoke(p1);
        }
    }
    public class ServiceRunData<T1, T2> : ServiceRunData
    {
        public Func<T1, T2, Task> Func;

        public override async Task RunAsync(HttpClient client, IServiceScope scope)
        {
            var p1 = (T1)scope.ServiceProvider.GetService<T1>();
            var p2 = (T2)scope.ServiceProvider.GetService<T2>();
            if (Func != null)
                await Func.Invoke(p1, p2);
        }
    }

}