
/* This File was taken (and slightly modified) from https://github.com/emipa606/RimworldModdingHelpers/blob/b5034f21c871244d2ff44a31f1598bbe6ee7694e/ModMenu/VersionFromManifest.cs
Because of this the license for this file is:
MIT License
Copyright (c) 2020 Mlie
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.IO;

namespace CM_Semi_Random_Research {
    public class VersionFromManifest
    {
        private const string ManifestFileName = "Manifest.xml";

        private List<string> dependencies;
        private string downloadUri;
        private string identifier;
        private List<string> incompatibleWith;
        private List<string> loadAfter;
        private List<string> loadBefore;
        private string manifestUri;
        private bool showCrossPromotions;
        public string version;

        private static string AboutDir(ModMetaData mod)
        {
            return Path.Combine(mod.RootDir.FullName, "About");
        }

        public static string GetVersionFromModMetaData(ModMetaData modMetaData)
        {
            var manifestPath = Path.Combine(AboutDir(modMetaData), ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                return null;
            }

            try
            {
                var manifest = DirectXmlLoader.ItemFromXmlFile<VersionFromManifest>(manifestPath, false);
                return manifest.version;
            }
            catch (Exception e)
            {
                Log.ErrorOnce($"Error loading manifest for '{modMetaData.Name}':\n{e.Message}\n\n{e.StackTrace}",
                    modMetaData.Name.GetHashCode());
            }

            return null;
        }
    }
}
