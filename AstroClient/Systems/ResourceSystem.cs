using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Systems
{
    internal class ResourceSystem
    {
        public static string ReadEmbeddedResource(string resourceName)
        {
            var assembly = typeof(ResourceSystem).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                return null;
            }
            using (var reader = new StreamReader(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
