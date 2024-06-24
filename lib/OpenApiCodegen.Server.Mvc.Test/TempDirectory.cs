using System;
using System.IO;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
	public class TempDirectory : IDisposable
	{
		public TempDirectory()
		{
			DirectoryPath = Path.Combine(
				Path.GetTempPath(),
				"PS-openapicodegen-tests",
				DateTime.Now.ToString("yyyy-MM-dd-hhmmss-") + Path.GetRandomFileName()
			);
			Directory.CreateDirectory(DirectoryPath);
		}

		public string DirectoryPath { get; private set; }

		void IDisposable.Dispose()
		{
			try
			{
				System.IO.Directory.Delete(DirectoryPath, true);
			}
			catch { }
		}
	}
}
