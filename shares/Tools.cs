using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace constants
{
    public static class Tools
    {
        public static string GetServiceName(string serviceName) {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"tools.config.xml")!;
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            string pattern = $@"{serviceName}Id\s+=\s+""([^""]*)""";
            Match m = Regex.Match(streamReader.ReadToEnd(), pattern, RegexOptions.None);
            if (!m.Success)
                throw new ArgumentException($"{serviceName}Id not found in config file");
            
            return m.Groups[1].ToString();
        }
    }
}