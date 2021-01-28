//TODO: not supported in .NET Standard 2.0
#if NETFULL
namespace Raider.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.CodeDom.Compiler;

    public class CodeInvoker
    {
        private Assembly assembly;
        public Assembly Assembly
        {
            get { return assembly; }
        }

        private List<string> errors;
        public List<string> Errors
        {
            get { return errors; }
            set { errors = value; }
        }

        public CodeInvoker(List<string> sourceFilePaths, List<string> assemblies, bool supportRoslynCSharp6, string outputAssemblyName = null, bool deleteSourceFiles = false, bool generateInMemory = true, int warningLevel = 3)
        {
            if (sourceFilePaths == null)
            {
                throw new ArgumentNullException(nameof(sourceFilePaths));
            }

            List<string> sources = new List<string>();
            foreach (string sourceFilePath in sourceFilePaths)
            {
                if (File.Exists(sourceFilePath))
                {
                    string source = File.ReadAllText(sourceFilePath);
                    if (!string.IsNullOrWhiteSpace(source))
                    {
                        sources.Add(source);
                    }
                }
            }

            if (0 < sources.Count)
            {
                Invoke(sources.ToArray(), assemblies, supportRoslynCSharp6, outputAssemblyName, generateInMemory, warningLevel);
            }

            if (deleteSourceFiles)
            {
                foreach (string sourceFilePath in sourceFilePaths)
                {
                    if (File.Exists(sourceFilePath))
                    {
                        File.Delete(sourceFilePath);
                    }
                }
            }
        }

        public CodeInvoker(string[] sources, List<string> assemblies, bool supportRoslynCSharp6, string outputAssemblyName = null, bool deleteSourceFile = false, bool generateInMemory = true, int warningLevel = 3)
        {
            Invoke(sources, assemblies, supportRoslynCSharp6, outputAssemblyName, generateInMemory, warningLevel);
        }

        private void Invoke(string[] sources, List<string> assemblies, bool supportRoslynCSharp6, string outputAssemblyName = null, bool generateInMemory = true, int warningLevel = 3)
        {
            if (sources == null || sources.Length == 0)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            errors = new List<string>();

            CodeDomProvider compiler;
            if (supportRoslynCSharp6)
            {
                compiler = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider(); //based on Roslyn and supports the C# 6 features.
            }
            else
            {
                compiler = CodeDomProvider.CreateProvider("CSharp"); //doesn't support C# 6
            }

            CompilerParameters cp = new CompilerParameters();

            //cp.ReferencedAssemblies.Add("mscorlib.dll");
            //cp.ReferencedAssemblies.Add("CSharpScripter.exe");

            if (assemblies != null)
            {
                foreach (string assemb in assemblies)
                {
                    cp.ReferencedAssemblies.Add(assemb);
                }
            }

            if (!string.IsNullOrWhiteSpace(outputAssemblyName)) cp.OutputAssembly = outputAssemblyName;
            cp.GenerateInMemory = generateInMemory;

            cp.WarningLevel = warningLevel;

            cp.CompilerOptions = "/target:library";  //"/target:library /optimize";
            cp.GenerateExecutable = false;

            CompilerResults cr = compiler.CompileAssemblyFromSource(cp, sources);

            if (0 < cr.Errors.Count)
            {
                foreach (CompilerError ce in cr.Errors)
                {
                    string level = ce.IsWarning ? "Warning" : "Error";
                    errors.Add(string.Format("{0}: File {1}: Ln {2} Col {3}: {4}: {5}", level, ce.FileName, ce.Line, ce.Column, ce.ErrorNumber, ce.ErrorText));
                }
                assembly = null;
            }
            else
            {
                assembly = cr.CompiledAssembly;
            }
        }
    }
}


#endif
