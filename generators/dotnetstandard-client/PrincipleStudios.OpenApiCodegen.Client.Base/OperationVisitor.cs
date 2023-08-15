using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.CSharp.Templates;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.OpenApi.CSharp
{
	class OperationVisitor : OpenApiDocumentVisitor<OperationVisitor.Argument>
	{
		private readonly ISchemaSourceResolver<InlineDataType> csharpSchemaResolver;
		private readonly CSharpSchemaOptions options;
		private readonly string controllerClassName;

		public record Argument(OpenApiTransformDiagnostic Diagnostic, RegisterControllerOperation RegisterControllerOperation, OperationBuilder? Builder = null);
		public delegate void RegisterControllerOperation(Templates.Operation operation);

		public class OperationBuilder
		{
			public OperationBuilder(OpenApiOperation operation)
			{
				Operation = operation;
			}

			public List<Func<OperationParameter[], OperationRequestBody>> RequestBodies { get; } = new();

			public OperationResponse? DefaultResponse { get; set; }
			public Dictionary<int, OperationResponse> StatusResponses { get; } = new();
			public List<OperationSecurityRequirement> SecurityRequirements { get; } = new();
			public List<OperationParameter> SharedParameters { get; } = new();
			public OpenApiOperation Operation { get; }
		}

		public OperationVisitor(ISchemaSourceResolver<InlineDataType> csharpSchemaResolver, CSharpSchemaOptions options, string controllerClassName)
		{
			this.csharpSchemaResolver = csharpSchemaResolver;
			this.options = options;
			this.controllerClassName = controllerClassName;
		}

		public override void Visit(OpenApiOperation operation, OpenApiContext context, Argument argument)
		{
			var httpMethod = context.GetLastKeyFor(operation);
			if (httpMethod == null)
				throw new ArgumentException("Expected HTTP method from context", nameof(context));
			var path = context.Where(c => c.Element is OpenApiPathItem).Last().Key;
			if (path == null)
				throw new ArgumentException("Context is not initialized properly - key expected for path items", nameof(context));

			var builder = new OperationBuilder(operation);

			var noBody = OperationRequestBodyFactory(operation.OperationId, null, Enumerable.Empty<OperationParameter>(), false);
			if (operation.RequestBody == null || !operation.RequestBody.Required)
				builder.RequestBodies.Add(noBody);

			base.Visit(operation, context, argument with { Builder = builder });

			var sharedParameters = builder.SharedParameters.ToArray();
			var requestBodies = builder.RequestBodies.DefaultIfEmpty(noBody).Select(transform => transform(sharedParameters))
				.Where(body => body.RequestBodyType != "application/xml") // exclude xml, since we don't support it
				.ToArray();
			if (requestBodies.Any())
			{
				argument.RegisterControllerOperation(
					new Templates.Operation(
					 HttpMethod: httpMethod,
					 Summary: operation.Summary,
					 Description: operation.Description,
					 Name: CSharpNaming.ToTitleCaseIdentifier(operation.OperationId, options.ReservedIdentifiers("ControllerBase", controllerClassName)),
					 Path: path,
					 RequestBodies: requestBodies,
					 HasQueryStringEmbedded: path.Contains("?"),
					 Responses: new Templates.OperationResponses(
						 DefaultResponse: builder.DefaultResponse,
						 StatusResponse: new(builder.StatusResponses)
					 ),
					 SecurityRequirements: builder.SecurityRequirements.ToArray()
				 ));
			}
		}

		public override void Visit(OpenApiParameter param, OpenApiContext context, Argument argument)
		{
			var dataType = csharpSchemaResolver.ToInlineDataType(param.Schema)();
			argument.Builder?.SharedParameters.Add(new Templates.OperationParameter(
				RawName: param.Name,
				ParamName: CSharpNaming.ToParameterName(param.Name, options.ReservedIdentifiers()),
				Description: param.Description,
				DataType: dataType.text,
				DataTypeNullable: dataType.nullable,
				DataTypeEnumerable: dataType.isEnumerable,
				IsPathParam: param.In == ParameterLocation.Path,
				IsQueryParam: param.In == ParameterLocation.Query,
				IsHeaderParam: param.In == ParameterLocation.Header,
				IsCookieParam: param.In == ParameterLocation.Cookie,
				IsBodyParam: false,
				IsFormParam: false,
				Required: param.Required,
				Pattern: param.Schema.Pattern,
				MinLength: param.Schema.MinLength,
				MaxLength: param.Schema.MaxLength,
				Minimum: param.Schema.Minimum,
				Maximum: param.Schema.Maximum
			));
		}
		public override void Visit(OpenApiResponse response, OpenApiContext context, Argument argument)
		{
			var responseKey = context.GetLastKeyFor(response);
			int? statusCode = int.TryParse(responseKey, out var s) ? s : null;
			if (!statusCode.HasValue || !HttpStatusCodes.StatusCodeNames.TryGetValue(statusCode.Value, out var statusCodeName))
				statusCodeName = "other status code";
			if (statusCodeName == responseKey)
				statusCodeName = $"status code {statusCode}";

			var result = new OperationResponse(
				Description: response.Description,
				Content: (from entry in response.Content.DefaultIfEmpty(new("", new OpenApiMediaType()))
						  let entryContext = context.Append(nameof(response.Content), entry.Key, entry.Value)
						  let dataType = entry.Value.Schema != null ? csharpSchemaResolver.ToInlineDataType(entry.Value.Schema)() : null
						  where entry.Key != "application/xml" // exclude xml, since we don't support it
						  select new OperationResponseContentOption(
							  MediaType: entry.Key,
							  ResponseMethodName: CSharpNaming.ToTitleCaseIdentifier($"{(response.Content.Count > 1 ? entry.Key : "")} {statusCodeName}", options.ReservedIdentifiers()),
							  DataType: dataType?.text
						  )).ToArray(),
				Headers: (from entry in response.Headers
						  let entryContext = context.Append(nameof(response.Headers), entry.Key, entry.Value)
						  let required = entry.Value.Required
						  let dataType = csharpSchemaResolver.ToInlineDataType(entry.Value.Schema)()
						  select new Templates.OperationResponseHeader(
							  RawName: entry.Key,
							  ParamName: CSharpNaming.ToParameterName("header " + entry.Key, options.ReservedIdentifiers()),
							  Description: entry.Value.Description,
							  DataType: dataType.text,
							  DataTypeNullable: dataType.nullable,
							  Required: entry.Value.Required,
							  Pattern: entry.Value.Schema.Pattern,
							  MinLength: entry.Value.Schema.MinLength,
							  MaxLength: entry.Value.Schema.MaxLength,
							  Minimum: entry.Value.Schema.Minimum,
							  Maximum: entry.Value.Schema.Maximum
						  )).ToArray()
			);

			if (statusCode.HasValue)
				argument.Builder?.StatusResponses.Add(statusCode.Value, result);
			else if (responseKey == "default" && argument.Builder != null)
				argument.Builder.DefaultResponse = result;
			else if (context.Entries[1].Property != "Components")
				argument.Diagnostic.Errors.Add(new OpenApiTransformError(context, $"Unknown response status: {responseKey}"));
		}
		//public override void Visit(OpenApiRequestBody requestBody, OpenApiContext context, Argument argument)
		//{
		//}
		public override void Visit(OpenApiMediaType mediaType, OpenApiContext context, Argument argument)
		{
			// All media type visitations should be for request bodies
			var mimeType = context.GetLastKeyFor(mediaType);

			var isForm = mimeType == "application/x-www-form-urlencoded";

			var singleContentType = argument.Builder?.Operation.RequestBody.Content.Count <= 1;

			argument.Builder?.RequestBodies.Add(OperationRequestBodyFactory(argument.Builder?.Operation.OperationId + (singleContentType ? "" : mimeType), mimeType, isForm ? GetFormParams() : GetStandardParams(), isForm));

			IEnumerable<OperationParameter> GetFormParams() =>
				from param in mediaType.Schema.Properties
				let required = mediaType.Schema.Required.Contains(param.Key)
				let dataType = csharpSchemaResolver.ToInlineDataType(param.Value)()
				select new Templates.OperationParameter(
					RawName: param.Key,
					ParamName: CSharpNaming.ToParameterName(param.Key, options.ReservedIdentifiers()),
					Description: null,
					DataType: dataType.text,
					DataTypeNullable: dataType.nullable,
					DataTypeEnumerable: dataType.isEnumerable,
					IsPathParam: false,
					IsQueryParam: false,
					IsHeaderParam: false,
					IsCookieParam: false,
					IsBodyParam: false,
					IsFormParam: true,
					Required: mediaType.Schema.Required.Contains(param.Key),
					Pattern: param.Value.Pattern,
					MinLength: param.Value.MinLength,
					MaxLength: param.Value.MaxLength,
					Minimum: param.Value.Minimum,
					Maximum: param.Value.Maximum
				);
			IEnumerable<OperationParameter> GetStandardParams() =>
				from ct in new[] { mediaType }
				let dataType = csharpSchemaResolver.ToInlineDataType(ct.Schema)()
				select new Templates.OperationParameter(
				   RawName: null,
				   ParamName: CSharpNaming.ToParameterName(argument.Builder?.Operation.OperationId + " body", options.ReservedIdentifiers()),
				   Description: null,
				   DataType: dataType.text,
				   DataTypeNullable: dataType.nullable,
				   DataTypeEnumerable: dataType.isEnumerable,
				   IsPathParam: false,
				   IsQueryParam: false,
				   IsHeaderParam: false,
				   IsCookieParam: false,
				   IsBodyParam: true,
				   IsFormParam: false,
				   Required: true,
				   Pattern: mediaType.Schema.Pattern,
				   MinLength: mediaType.Schema.MinLength,
				   MaxLength: mediaType.Schema.MaxLength,
				   Minimum: mediaType.Schema.Minimum,
				   Maximum: mediaType.Schema.Maximum
			   );
		}

		private Func<OperationParameter[], OperationRequestBody> OperationRequestBodyFactory(string operationName, string? requestBodyMimeType, IEnumerable<OperationParameter> parameters, bool isForm)
		{
			return sharedParams => new Templates.OperationRequestBody(
				 Name: CSharpNaming.ToTitleCaseIdentifier(operationName, options.ReservedIdentifiers()),
				 IsForm: isForm,
				 IsFile: parameters.Any(t => t.IsFile),
				 HasQueryParam: sharedParams.Concat(parameters).Any(p => p.IsQueryParam),
				 RequestBodyType: requestBodyMimeType,
				 AllParams: sharedParams.Concat(parameters)
			 );
		}

		public override void Visit(OpenApiSecurityRequirement securityRequirement, OpenApiContext context, Argument argument)
		{
			argument.Builder?.SecurityRequirements.Add(new OperationSecurityRequirement(
								 (from scheme in securityRequirement
								  select new Templates.OperationSecuritySchemeRequirement(scheme.Key.Reference.Id, scheme.Value.ToArray())).ToArray())
							 );
		}
	}
}
