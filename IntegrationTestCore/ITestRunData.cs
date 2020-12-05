using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTestCore
{
    public interface ITestRunData
    {
        Task CheckAsync();
        Task RunAsync(ITestStage client);
    }
}