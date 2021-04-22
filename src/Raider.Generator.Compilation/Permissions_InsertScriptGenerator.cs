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
    
    #line 1 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class Permissions_InsertScriptGenerator : GeneratorBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("\r\n");
            
            #line 11 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

	SetGenerationEnvironment(this.GenerationEnvironment, false);
	
	string targetFolder = GetParam("TargetFolder");
	var permissions = GetParam<List<Raider.Generator.Compilation.IPermission>>("Permissions");
	var rolePermissions = GetParam<Dictionary<int, List<int>>>("RolePermissions");
	var withDescription = GetParam<bool>("WithDescription");
	
	string permissionsVersionFileName = GetParam("PermissionsVersionFileName");
	bool onlyInsert = !string.IsNullOrWhiteSpace(permissionsVersionFileName);

	var fileName = onlyInsert
		? permissionsVersionFileName + ".sql"
		: "Permissions_InsertScript.sql";

	if (onlyInsert)
		targetFolder = Path.Combine(targetFolder, "DBPatch");

	StartNewFile(targetFolder, fileName);

	if (!onlyInsert)
	{

            
            #line default
            #line hidden
            this.Write("DELETE FROM mbs.\"RolePermission\";\r\n\r\nDELETE FROM mbs.\"Permission\";\r\n\r\n");
            
            #line 38 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
	 
	}
	else
	{

            
            #line default
            #line hidden
            this.Write("--Add new permissions\r\n\r\n");
            
            #line 45 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

	}

            
            #line default
            #line hidden
            this.Write("INSERT INTO mbs.\"Permission\"\r\n\t(\"IdPermission\", \"Name\"");
            
            #line 49 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(withDescription ? ", \"Description\"" : ""));
            
            #line default
            #line hidden
            this.Write(")\r\nVALUES\r\n");
            
            #line 51 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

	int idx = 0;
	foreach (var permisison in permissions.OrderBy(x => x.IdPermission))
	{
		idx++;
		if (withDescription)
		{

            
            #line default
            #line hidden
            this.Write("\t (");
            
            #line 59 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(permisison.IdPermission));
            
            #line default
            #line hidden
            this.Write(", \'");
            
            #line 59 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(permisison.Name));
            
            #line default
            #line hidden
            this.Write("\', ");
            
            #line 59 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(string.IsNullOrWhiteSpace(permisison.Description) ? "null" : ("'" + permisison.Description + "'")));
            
            #line default
            #line hidden
            this.Write(")");
            
            #line 59 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(permissions.Count == idx ? "" : ","));
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 60 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

		}
		else
		{

            
            #line default
            #line hidden
            this.Write("\t (");
            
            #line 65 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(permisison.IdPermission));
            
            #line default
            #line hidden
            this.Write(", \'");
            
            #line 65 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(permisison.Name));
            
            #line default
            #line hidden
            this.Write("\')");
            
            #line 65 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(permissions.Count == idx ? "" : ","));
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 66 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

		}
	}

            
            #line default
            #line hidden
            this.Write("--ON CONFLICT (\"IdPermission\") DO NOTHING\r\n;\r\n");
            
            #line 72 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

	if (0 < rolePermissions.Count)
	{

            
            #line default
            #line hidden
            this.Write("\r\n\r\nINSERT INTO mbs.\"RolePermission\"\r\n(\r\n\t\"IdRole\", \"IdPermission\", \"AuditCreated" +
                    "Time\", \"ConcurrencyToken\"\r\n)\r\nVALUES\r\n");
            
            #line 83 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

	int i = 0;
	foreach (var kvp in rolePermissions)
	{
		foreach (var idPermission in kvp.Value)
		{

            
            #line default
            #line hidden
            this.Write("\t ");
            
            #line 90 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(i == 0 ? "" : ","));
            
            #line default
            #line hidden
            this.Write("(");
            
            #line 90 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(kvp.Key));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 90 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(idPermission.ToString()));
            
            #line default
            #line hidden
            this.Write(", \'2021-01-01\', \'");
            
            #line 90 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Guid.Empty.ToString("D")));
            
            #line default
            #line hidden
            this.Write("\')\r\n");
            
            #line 91 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

			i++;
		}
	}

            
            #line default
            #line hidden
            this.Write("--ON CONFLICT (\"IdRole\", \"IdPermission\") DO NOTHING\r\n;\r\n\r\n");
            
            #line 99 "C:\Code\GitLab\H\FWK\src\Raider.Generator.Compilation\Permissions_InsertScriptGenerator.tt"

	}
	Process();

            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
