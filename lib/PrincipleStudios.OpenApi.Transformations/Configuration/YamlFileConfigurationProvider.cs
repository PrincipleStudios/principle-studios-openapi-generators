using Microsoft.Extensions.Configuration;
using SharpYaml;
using System;
using System.IO;

namespace PrincipleStudios.OpenApi.Transformations.Configuration
{
	/// <summary>
	/// A YAML file based <see cref="FileConfigurationProvider"/>.
	/// </summary>
	public class YamlFileConfigurationProvider : FileConfigurationProvider
	{
		public YamlFileConfigurationProvider(YamlFileConfigurationSource source) : base(source) { }

		public override void Load(Stream stream)
		{
			var parser = new YamlConfigurationFileParser();
			try
			{
				Data = parser.Parse(stream);
			}
			catch (YamlException e)
			{
				throw new FormatException(Resources.FormatError_YamlParseError(e.Message), e);
			}
		}
	}
}