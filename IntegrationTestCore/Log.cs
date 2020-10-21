using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace IntegrationTestCore
{
    public static class Resource
    {
        public static string ReadRelativeResource(string relativePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullResourceName = string.Join(".", assembly.GetName().Name, relativePath.Replace("/", "."));
            using Stream stream = assembly.GetManifestResourceStream(fullResourceName);
            if (stream == null)
                throw new ArgumentException(
                    $"Resource {fullResourceName} not found, available resources:]\n{string.Join("\n", assembly.GetManifestResourceNames())}");

            using StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();
            return result;
        }
    }

    public static class Diff
    {
        public static string Text(string expected, string actual, Func<string, string> normalize = null)
        {
            var c1 = expected ?? "-";
            var c2 = actual ?? "-";
            var c1Normalized = normalize?.Invoke(c1) ?? c1;
            var c2Normalized = normalize?.Invoke(c2) ?? c2;
            var c0 = LineByLineOutput.DiffTexts(c1Normalized, c2Normalized);
            var columns = new[] { c0, c1, c2 };
            var s = LineByLineOutput.CombineLineByLine(" ", columns);
            if (string.IsNullOrEmpty(s))
                s = "-";
            return s;
        }
        public static string Json<T>(T expected, T actual)
        {
            var serialize =
                new Func<T, string>(o => o == null ? "-" : JsonConvert.SerializeObject(o, Formatting.Indented));

            var c1 = serialize(expected);
            var c2 = serialize(actual);
            var c0 = LineByLineOutput.DiffTexts(c1, c2);
            var columns = new[] { c0, c1, c2 };
            var s = LineByLineOutput.CombineLineByLine(" ", columns);
            if (string.IsNullOrEmpty(s))
                s = "-";
            return s;
        }
        public static string List<T>(IList<T> expected, IList<T> actual)
        {
            expected ??= new T[0];
            var mx = Math.Max(expected.Count, actual.Count);
            var serialize = new Func<T, string>(o => o == null ? "-" : JsonConvert.SerializeObject(o, Formatting.Indented));
            var sb = new StringBuilder();
            for (int i = 0; i < mx; i++)
            {
                var a = i >= actual.Count ? default(T) : actual[i];
                var e = i >= expected.Count ? default(T) : expected[i];
                var c1 = serialize(e);
                var c2 = serialize(a);
                var c0 = LineByLineOutput.DiffTexts(c1, c2);
                var columns = new[] { c0, c1, c2 };
                var s = LineByLineOutput.CombineLineByLine(" ", columns);
                if (string.IsNullOrEmpty(s))
                    s = "-";
                sb.AppendLine(s);
            }

            return sb.ToString();
        }
    }
}
