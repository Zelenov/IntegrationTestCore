using System.Collections.Generic;
using System.Net.Http;

namespace IntegrationTestCore
{
    public class HttpContentEqualityComparer : IEqualityComparer<HttpContent>
    {
        public bool Equals(HttpContent x, HttpContent y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            if (x.GetType() != y.GetType())
                return false;

            var strX = x.ReadAsStringAsync().GetAwaiter().GetResult();
            var strY = y.ReadAsStringAsync().GetAwaiter().GetResult();
            return Equals(strX, strY);
        }

        public int GetHashCode(HttpContent obj)
        {
            var str = obj.ReadAsStringAsync().GetAwaiter().GetResult();
            var hashCode = str.GetHashCode();
            return hashCode;
        }

        public static IEqualityComparer<HttpContent> Default { get; } = new HttpContentEqualityComparer();
    }
}