using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntegrationTestCore
{
    public static class HttpRequestMessageExtensions
    {

        private static readonly Regex XmlMethodRegex =
            new Regex(@"\s*<!--\s*?METHOD:\s*?(?<method>.+?)\s*?-->\s*", RegexOptions.Compiled);

        private static readonly Regex XmlUrlRegex = new Regex(@"<!--\s*URL:\s*(?<url>.+?)\s*-->", RegexOptions.Compiled);

        private static readonly Regex XmlStatusRegex =
            new Regex(@"\s*<!--\s*?(STATUS CODE|STATUS):\s*?(?<statusCode>.+?)\s*?-->\s*", RegexOptions.Compiled);

        public static string GetUrl(this HttpRequestMessage self)
        {
            return self?.RequestUri?.ToString();
        }
        public static string GetBody(this HttpRequestMessage self)
        {
            return self?.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        public static string GetNormalizedBody(this HttpRequestMessage self)
        {
            var body = GetBody(self);
            if (body == null)
                return null;
            var mediaType = self.Content?.Headers?.ContentType?.MediaType;
            return body.Normalize(mediaType);
        }
        public static KeyValuePair<string, IEnumerable<string>>[] GetHeaders(this HttpRequestMessage self)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = self.Headers;
            if (self.Content != null)
            {
                headers = headers.Concat(self.Content.Headers);
            }

            return headers.ToArray();
        }
        public static HttpRequestMessage FromXmlResource(this HttpRequestMessage self, string resourceName)
        {
            var request = Resource.ReadRelativeResource(resourceName);
            return FromXmlString(self, request);
        }
        public static HttpRequestMessage FromJsonResource(this HttpRequestMessage self, string resourceName)
        {
            var request = Resource.ReadRelativeResource(resourceName);
            return FromJsonString(self, request);
        }
        public static HttpRequestMessage FromJsonFile(this HttpRequestMessage self, string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"File {fileName} not found");
            var request = File.ReadAllText(fileName);
            return FromJsonString(self, request);
        }
        public static HttpRequestMessage FromXmlFile(this HttpRequestMessage self, string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"File {fileName} not found");
            var request = File.ReadAllText(fileName);
            return FromXmlString(self, request);
        }
        public static HttpRequestMessage FromXmlString(this HttpRequestMessage self, string request)
        {
            var methodMatch = XmlMethodRegex.Match(request);
            if (!methodMatch.Success)
                throw new ArgumentException($"No METHOD found in the request xml string");

            var method = new HttpMethod(methodMatch.Groups["method"].Value);
            var urlMatch = XmlUrlRegex.Match(request);
            if (!urlMatch.Success)
                throw new ArgumentException($"No URL found in the request xml string");

            var url = urlMatch.Groups["url"].Value;
            var res = new HttpRequestMessage().WithXmlBody(request).WithUrl(url).WithMethod(method);
            return res;
        }

        public static HttpRequestMessage FromJsonString(this HttpRequestMessage self, string request)
        {
            JObject jObject = JObject.Parse(request);

            var body = !jObject.TryGetValue("body", out var bodyMatch) ? null : bodyMatch.ToString(Formatting.Indented);

            var method = !jObject.TryGetValue("method", out var methodMatch) ? "GET" : methodMatch.Value<string>();

            if (!jObject.TryGetValue("url", out var urlMatch))
                throw new ArgumentException($"No URL found in the request body string");

            var url = urlMatch.Value<string>();

            var res = new HttpRequestMessage().WithUrl(url).WithMethod(method);
            if (body != null)
                res.WithJsonBody(body);
            return res;
        }
        
        public static HttpRequestMessage WithMethod(this HttpRequestMessage self, string methodName)
        {
            var method = new HttpMethod(methodName);
            self.Method = method;
            return self;
        }
        public static HttpRequestMessage WithVersion(this HttpRequestMessage self, Version version)
        {
            self.Version = version;
            return self;
        }
        public static HttpRequestMessage WithVersion(this HttpRequestMessage self, string versionStr)
        {
            return self.WithVersion(new Version(versionStr));
        }
        public static HttpRequestMessage WithHeaders(this HttpRequestMessage self, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var header in headers)
            {
                self.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return self;
        }
        public static HttpRequestMessage WithHeaders(this HttpRequestMessage self, IEnumerable<KeyValuePair<string, string>> headers)
        {
            foreach (var header in headers)
            {
                self.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return self;
        }
        public static HttpRequestMessage WithMethod(this HttpRequestMessage self, HttpMethod method)
        {
            self.Method = method;
            return self;
        }
        public static HttpRequestMessage WithUrl(this HttpRequestMessage self, string url)
        {
          self.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);
            return self;
        }
        public static HttpRequestMessage WithJsonBody(this HttpRequestMessage self, string body)
        {
            self.Content = new StringContent(body, Encoding.UTF8, "application/json");
            return self;
        }
        public static HttpRequestMessage WithXmlBody(this HttpRequestMessage self, string body)
        {
            self.Content = new StringContent(body, Encoding.UTF8, "application/xml");
            return self;
        }
        public static string Serialize(this HttpRequestMessage self, bool normalized = false)
        {
            return JsonConvert.SerializeObject(new SerializableHttpRequestMessage(self, normalized));
        }

        internal sealed class SerializableHttpRequestMessage
        {
            public Version Version { get; }
            public string Content { get; }
            public HttpMethod Method { get; }
            public Uri RequestUri { get; }
            public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; }

            public SerializableHttpRequestMessage(HttpRequestMessage message, bool normalized)
            {
                Version = message.Version;
                Content = normalized ? message.GetNormalizedBody() : message.GetBody();
                Method = message.Method;
                RequestUri = message.RequestUri;
                Headers = message.Headers;
            }
        }
    }
}