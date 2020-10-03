using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTestCore
{
    public abstract class ApiRunData : ITestRunData
    {
        public HttpRequestMessage Request { get; set; }
        public HttpResponseMessage ExpectedResponse { get; set; }
        public HttpResponseMessage ActualResponse { get; protected set; }

        public async Task RunAsync(HttpClient client, IServiceScope scope)
        {
            LogRequest();
            ActualResponse = await client.SendAsync(Request);
            LogResponse();
        }

        public abstract Task CheckAsync();

        protected  virtual void LogRequest()
        {
            Console.WriteLine("REQUEST");
            SerializeHttpRequestMessage(Request);
        }

        protected virtual void LogResponse()
        {
            Console.WriteLine(Diff.Text(ExpectedResponse.GetNormalizedBody() ?? "-", ActualResponse.GetNormalizedBody()?? "-"));
            Console.WriteLine("RESPONSE");
            Console.WriteLine("ACTUAL");
            Console.WriteLine(SerializeHttpResponseMessage(ActualResponse));
            Console.WriteLine("EXPECTED");
            Console.WriteLine(SerializeHttpResponseMessage(ExpectedResponse));
        }

        public virtual string SerializeHttpResponseMessage(HttpResponseMessage httpResponseMessage)
        {
            return 
                string.Join(Environment.NewLine, 
                    $"STATUS: {httpResponseMessage.StatusCode}",
                    $"HEADERS: {string.Join(Environment.NewLine, httpResponseMessage.GetHeaders().Select(h => $"{h.Key}: {string.Join((string) ", ", (IEnumerable<string>) h.Value)}"))}",
                    $"{httpResponseMessage.GetBody()}");
        }
        public virtual string SerializeHttpRequestMessage(HttpRequestMessage httpRequestMessage)
        {
            return string.Join(Environment.NewLine, $"URL: {httpRequestMessage.GetUrl()}",
                $"HEADERS: {string.Join(Environment.NewLine, httpRequestMessage.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}",
                httpRequestMessage.GetBody());
        }
      

     
    }
}