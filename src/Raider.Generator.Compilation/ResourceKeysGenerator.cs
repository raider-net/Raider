﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 16.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Raider.Generator.Compilation
{
    using Raider.Extensions;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Text;
    using System.Collections.Generic;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class ResourceKeysGenerator : GeneratorBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("\r\n");
            
            #line 11 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

	SetGenerationEnvironment(this.GenerationEnvironment);
	
	string targetProject = GetParam("TargetProject");
	string nmspace = GetParam("RootNamespace");
	var resFiles = GetParam<List<Raider.Localization.ResourceFile>>("ResFiles");
	
	foreach (var resFile in resFiles)
	{
		var resStructure = resFile.GetConfigurationFolderStructure(targetProject);
		var resPath = resFile.GetConfigurationFolderPath(null, targetProject);
		StartNewFile(resPath, resFile.Name + "ResourceKeys.cs");
		
		int ident = 0;


            
            #line default
            #line hidden
            this.Write(@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Extensions.Localization;
using Raider.Localization;
using System;

namespace ");
            
            #line 40 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(nmspace));
            
            #line default
            #line hidden
            this.Write("\r\n{\r\n");
            
            #line 42 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

		foreach (var parentClass in resStructure)
		{

            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 46 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("public partial class ");
            
            #line 46 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(parentClass));
            
            #line default
            #line hidden
            this.Write("\r\n\t");
            
            #line 47 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("{\r\n");
            
            #line 48 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

			ident++;
		}
		string className = resFile.Name.ToCammelCase(removeUnderscores: false);

            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 53 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("public partial class ");
            
            #line 53 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(className));
            
            #line default
            #line hidden
            this.Write("Keys\r\n\t");
            
            #line 54 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("{\r\n\t");
            
            #line 55 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident + 1)));
            
            #line default
            #line hidden
            this.Write("public const string __BaseName = \"");
            
            #line 55 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(resPath.TrimPrefix(targetProject).Replace(System.IO.Path.DirectorySeparatorChar, '.')));
            
            #line default
            #line hidden
            this.Write(".");
            
            #line 55 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(resFile.Name));
            
            #line default
            #line hidden
            this.Write("\";\r\n\r\n");
            
            #line 57 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

				int resCount = 0;
				foreach (var resource in resFile)
				{
					if (string.IsNullOrWhiteSpace(resource.Value))
					{
						throw new InvalidOperationException("Resource " + resFile.FullPath + " has invalid value for name " + resource.Name);
					}

					if (0 < resCount)
					{

            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 70 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

					}

            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 73 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident + 1)));
            
            #line default
            #line hidden
            this.Write("public const string ");
            
            #line 73 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(resource.Name.ToCammelCase(removeUnderscores: false)));
            
            #line default
            #line hidden
            this.Write(" = \"");
            
            #line 73 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(resource.Name));
            
            #line default
            #line hidden
            this.Write("\";\r\n");
            
            #line 74 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

					resCount++;
				}

            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 78 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("}\r\n\r\n\t");
            
            #line 80 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("public partial class ");
            
            #line 80 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(className));
            
            #line default
            #line hidden
            this.Write("LocalizerFactory : ResourceLocalizer<");
            
            #line 80 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(resPath.TrimPrefix(targetProject).Replace(System.IO.Path.DirectorySeparatorChar, '.')));
            
            #line default
            #line hidden
            this.Write(".");
            
            #line 80 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(resFile.Name));
            
            #line default
            #line hidden
            this.Write("Keys>\r\n\t");
            
            #line 81 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("{\r\n\t");
            
            #line 82 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("\tpublic ");
            
            #line 82 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(className));
            
            #line default
            #line hidden
            this.Write("LocalizerFactory(IServiceProvider serviceProvider)\r\n\t");
            
            #line 83 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("\t\t: base(serviceProvider,\r\n\t");
            
            #line 84 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("\t\t\t");
            
            #line 84 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(className));
            
            #line default
            #line hidden
            this.Write("Keys.__BaseName,\r\n\t");
            
            #line 85 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("\t\t\tnew System.Reflection.AssemblyName(typeof(");
            
            #line 85 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(className));
            
            #line default
            #line hidden
            this.Write("LocalizerFactory).Assembly.FullName).Name)\r\n\t");
            
            #line 86 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("\t{\r\n\t");
            
            #line 87 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("\t}\r\n\t");
            
            #line 88 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(ident)));
            
            #line default
            #line hidden
            this.Write("}\r\n");
            
            #line 89 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"


		for (int i = resStructure.Count - 1; 0 <= i; i--)
		{

            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 94 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIdent(i)));
            
            #line default
            #line hidden
            this.Write("}\r\n");
            
            #line 95 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

		}

            
            #line default
            #line hidden
            this.Write("}\r\n");
            
            #line 99 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

	}
	Process();

            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
        
        #line 103 "C:\Code\GitLab\Raider\src\Raider.Generator.Compilation\ResourceKeysGenerator.tt"

	private string GetIdent(int count)
	{
		return GetIdent(null, count);
	}

	private string GetIdent(string baseIdent, int count)
	{
		StringBuilder sb = new StringBuilder(baseIdent ?? "");
		for (int i = 0; i < count; i++)
		{
			sb.Append("\t");
		}
		return sb.ToString();
	}

	private IEnumerable<string> Lines(string value)
	{
		return value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
	}

	private string Xml(string value)
	{
		return value.Replace("<", "&lt;").Replace(">", "&gt;");
	}

	private string List(IEnumerable<string> items)
	{
		return List(null, items);
	}

	private string List(string prefix, IEnumerable<string> items, string suffix = null)
	{
		return string.Join(", ", items.Select(i => prefix + i + suffix));
	}

        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
}
