using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace IntegrationTestCore.Parsers
{
    public abstract class HttpRequestMessageParser
    {
        public abstract HttpRequestMessage Parse(string input);
    }

    public class JsonRequestParser
    {

    }
}
