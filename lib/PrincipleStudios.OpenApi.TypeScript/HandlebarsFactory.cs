using HandlebarsDotNet;
using System;

namespace PrincipleStudios.OpenApi.TypeScript
{
	public class HandlebarsFactory
	{
		private readonly Lazy<IHandlebars> handlebars;

		public HandlebarsFactory(Func<IHandlebars> innerFactory)
		{
			this.handlebars = new Lazy<IHandlebars>(innerFactory);
		}

		public IHandlebars Handlebars => handlebars.Value;
	}
}