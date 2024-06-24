using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.CSharp;

public class CSharpServerSchemaOptions : CSharpSchemaOptions
{
	public string PathPrefix { get; set; } = "";
}
