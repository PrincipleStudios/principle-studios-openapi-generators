using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript
{
    public static class SolutionConfiguration
    {
        private const string solutionRootToTypeScriptPackage = "generators/typescript/npm";

        public static string SolutionRoot
        {
            get
            {
                var dir = Directory.GetCurrentDirectory();
                var beforeArtifacts = dir.Substring(0, dir.IndexOf(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar));
                return beforeArtifacts;
            }
        }

        public static string TypeScriptPackagePath
        {
            get
            {
                return Path.Combine(SolutionRoot, solutionRootToTypeScriptPackage);
            }
        }
    }
}
