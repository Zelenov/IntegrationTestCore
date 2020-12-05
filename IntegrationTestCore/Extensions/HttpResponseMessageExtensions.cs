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
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;
#pragma warning disable 1591

namespace IntegrationTestCore
{
    public static class StringExtensions
    {

        public static string NormalizeJson(this string str)
        {
            return string.IsNullOrEmpty(str) ? null :
                !str.StartsWith("[") ? JObject.Parse(str).ToString(Formatting.Indented) :
                JArray.Parse(str).ToString(Formatting.Indented);
        }

        public static string Normalize(this string body, string mediaType)
        {
            switch (mediaType)
            {
                case "application/json": return body.NormalizeJson();
                case "application/xml": return body.NormalizeXml();
                default: return body;
            }
        }

        public static string NormalizeXml(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            XmlDocument xmlDocument = new XmlDocument();
            using var stringReader = new StringReader(str);
            using var xmlReader = XmlReader.Create(stringReader);
            xmlDocument.Load(xmlReader);
            xmlDocument.Normalize();
            using var stringWriter = new StringWriter();
            using var xmlTextWriter = XmlWriter.Create(stringWriter);
            xmlDocument.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            return stringWriter.GetStringBuilder().ToString();
        }
    }

    public static class HttpResponseMessageExtensions
    {
        
        private static readonly Regex XmlMethodRegex =
            new Regex(@"\s*<!--\s*?METHOD:\s*?(?<method>.+?)\s*?-->\s*", RegexOptions.Compiled);

        private static readonly Regex XmlUrlRegex = new Regex(@"<!--\s*URL:\s*(?<url>.+?)\s*-->", RegexOptions.Compiled);

        private static readonly Regex XmlStatusRegex =
            new Regex(@"\s*<!--\s*?(STATUS CODE|STATUS):\s*?(?<statusCode>.+?)\s*?-->\s*", RegexOptions.Compiled);


       
        public static KeyValuePair<string, IEnumerable<string>>[] GetHeaders(this HttpResponseMessage self)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = self.Headers;
            if (self.Content != null)
            {
                headers = headers.Concat(self.Content.Headers);
            }

            return headers.ToArray();
        }
        public static string GetBody(this HttpResponseMessage self)
        {
            return self?.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        public static string Serialize(this HttpResponseMessage self, bool normalized = false)
        {
            return JsonConvert.SerializeObject(new SerializableHttpResponseMessage(self, normalized));
        }

        internal sealed class SerializableHttpResponseMessage
        {
            public string Content { get; }
            public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; }
            public string ReasonPhrase { get; }
            public HttpStatusCode StatusCode { get; }
            public Version Version { get; }

            public SerializableHttpResponseMessage(HttpResponseMessage message, bool normalized)
            {
                Content = normalized ? message.GetNormalizedBody() : message.GetBody();
                Headers = message.Headers;
                ReasonPhrase = message.ReasonPhrase;
                StatusCode = message.StatusCode;
                Version = message.Version;
            }
        }

        public static string GetNormalizedBody(this HttpResponseMessage self)
        {
            var body = GetBody(self);
            if (body == null)
                return null;
            var mediaType = self.Content?.Headers?.ContentType?.MediaType;
            return body.Normalize(mediaType);
        }
        public static HttpResponseMessage FromXmlResource(this HttpResponseMessage self, string resourceName)
        {
            var response = Resource.ReadRelativeResource(resourceName);
            return FromXmlString(self, response);
        }
        public static HttpResponseMessage FromJsonResource(this HttpResponseMessage self, string resourceName)
        {
            var response = Resource.ReadRelativeResource(resourceName);
            return FromJsonString(self, response);
        }
        public static HttpResponseMessage FromJsonFile(this HttpResponseMessage self, string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"File {fileName} not found");
            var request = File.ReadAllText(fileName);
            return FromJsonString(self, request);
        }
        public static HttpResponseMessage FromXmlFile(this HttpResponseMessage self, string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"File {fileName} not found");
            var request = File.ReadAllText(fileName);
            return FromXmlString(self, request);
        }
        public static HttpResponseMessage FromXmlString(this HttpResponseMessage self, string response)
        {
            var methodMatch = XmlMethodRegex.Match(response);
            if (!methodMatch.Success)
                throw new ArgumentException($"No METHOD found in the response xml string");

            var method = new HttpMethod(methodMatch.Groups["method"].Value);
            var statusCodeMatch = XmlStatusRegex.Match(response);
            if (!statusCodeMatch.Success)
                throw new ArgumentException($"No URL found in the response xml string");

            var statusCode = statusCodeMatch.Groups["statusCode"].Value;
            var res = new HttpResponseMessage().WithXmlBody(response).WithStatusCode(statusCode);
            return res;
        }
        public static HttpResponseMessage FromJsonString(this HttpResponseMessage self, string response)
        {
            JObject jObject = JObject.Parse(response);
            var body = !jObject.TryGetValue("body", out var methodMatch) ? null : methodMatch.ToString(Formatting.Indented);
            if (!jObject.TryGetValue("httpStatusCode", out var statusCodeMatch))
                throw new ArgumentException($"No httpStatusCode found in the response json string");

            var statusCode = statusCodeMatch.Value<string>();

            var res = new HttpResponseMessage().WithStatusCode(statusCode); 
            if (body != null)
                res.WithJsonBody(body);
            return res;
        }
        public static HttpResponseMessage WithStatusCode(this HttpResponseMessage self, int statusCode)
        {
            return self.WithStatusCode((HttpStatusCode)statusCode);
        }
        public static HttpResponseMessage WithReasonPhrase(this HttpResponseMessage self, string reasonPhrase)
        {
            self.ReasonPhrase = reasonPhrase;
            return self;
        }
        public static HttpRequestMessage WithHeaders(this HttpRequestMessage self, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var header in headers)
            {
                self.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
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
        public static HttpRequestMessage WithHeaders(this HttpRequestMessage self, IEnumerable<KeyValuePair<string, string>> headers)
        {
            foreach (var header in headers)
            {
                self.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return self;
        }
        public static HttpResponseMessage WithStatusCode(this HttpResponseMessage self, string statusCode)
        {
            if (Enum.TryParse<HttpStatusCode>(statusCode, true, out var sc))
                return self.WithStatusCode((HttpStatusCode)sc);


            if(int.TryParse(statusCode, out var scInt))
                return self.WithStatusCode(scInt);

            throw new ArgumentException(nameof(statusCode));
        }
        public static HttpResponseMessage WithStatusCode(this HttpResponseMessage self, HttpStatusCode statusCode)
        {
            self.StatusCode = statusCode;
            return self;
        }
        public static HttpResponseMessage WithJsonBody(this HttpResponseMessage self, string body)
        {
            self.Content = new StringContent(body, Encoding.UTF8, "application/json");
            return self;
        }
        public static HttpResponseMessage WithXmlBody(this HttpResponseMessage self, string body)
        {
            self.Content = new StringContent(body, Encoding.UTF8, "application/xml");
            return self;
        }
    }
}