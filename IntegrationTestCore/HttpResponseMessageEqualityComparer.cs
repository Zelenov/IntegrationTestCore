using System.Collections.Generic;
using System.Net.Http;

namespace IntegrationTestCore
{
    public class HttpResponseMessageEqualityComparer : IEqualityComparer<HttpResponseMessage>
    {
        public bool CompareHeaders { get; set; } = true;
        public bool CompareReasonPhrase { get; set; } = true;
        public bool CompareStatusCode { get; set; } = true;
        public bool CompareVersion { get; set; } = true;
        public bool CompareContent { get; set; } = true;
        public bool Equals(HttpResponseMessage x, HttpResponseMessage y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            if (x.GetType() != y.GetType())
                return false;

            return (!CompareHeaders || Equals(x.Headers, y.Headers)) &&
                (!CompareReasonPhrase || x.ReasonPhrase == y.ReasonPhrase) &&
                (!CompareStatusCode || x.StatusCode == y.StatusCode) &&
                (!CompareVersion || Equals(x.Version, y.Version)) &&
                (!CompareContent || HttpContentEqualityComparer.Default.Equals(x.Content, y.Content));
        }

        public int GetHashCode(HttpResponseMessage obj)
        {
            unchecked
            {
                var hashCode = (obj.Content != null && CompareContent ? obj.Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Headers != null && CompareHeaders ? obj.Headers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.ReasonPhrase != null && CompareReasonPhrase ? obj.ReasonPhrase.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CompareStatusCode ? (int)obj.StatusCode : 0);
                hashCode = (hashCode * 397) ^ (obj.Version != null && CompareVersion ? obj.Version.GetHashCode() : 0);
                return hashCode;
            }
        }
        public static IEqualityComparer<HttpResponseMessage> Default { get; } = new HttpResponseMessageEqualityComparer
        {
            CompareHeaders = true,
            CompareReasonPhrase = true,
            CompareStatusCode = true,
            CompareVersion = true,
            CompareContent = true,
        };
        public static IEqualityComparer<HttpResponseMessage> StatusCodeAndContent { get; } = new HttpResponseMessageEqualityComparer
        {
            CompareHeaders = false,
            CompareReasonPhrase = false,
            CompareStatusCode = true,
            CompareVersion = false,
            CompareContent = true,
        };
        public static IEqualityComparer<HttpResponseMessage> StatusCode { get; } = new HttpResponseMessageEqualityComparer()
        {
            CompareHeaders = false,
            CompareReasonPhrase = false,
            CompareStatusCode = true,
            CompareVersion = false,
            CompareContent = false,
        };

    }
}