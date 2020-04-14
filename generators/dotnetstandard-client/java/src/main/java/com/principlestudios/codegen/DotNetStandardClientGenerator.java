package com.principlestudios.codegen;

import com.samskivert.mustache.Mustache;
import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.responses.ApiResponse;
import io.swagger.v3.oas.models.responses.ApiResponses;

import org.openapitools.codegen.*;
import org.openapitools.codegen.languages.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.*;
import java.io.File;

public class DotNetStandardClientGenerator extends AbstractCSharpCodegen {
    protected Logger LOGGER = LoggerFactory.getLogger(DotNetStandardClientGenerator.class);

  // source folder where to write the files
  protected String sourceFolder = "src";
  protected String apiVersion = "1.0.0";

  public DotNetStandardClientGenerator() {
    super();

    outputFolder = "generated-code" + File.separator + getName();

    modelTemplateFiles.put("model.mustache", ".cs");
    apiTemplateFiles.put("client.mustache", ".cs");
    instantiationTypes.put("array", "IReadOnlyList");
    instantiationTypes.put("list", "IReadOnlyList");
    instantiationTypes.put("map", "IDictionary");

    typeMapping.put("array", "IReadOnlyList");
    typeMapping.put("list", "IReadOnlyList");

    embeddedTemplateDir = templateDir = "dotnetstandard-client";

    // contextually reserved words
    // NOTE: C# uses camel cased reserved words, while models are title cased. We don't want lowercase comparisons.
    reservedWords.addAll(
            Arrays.asList("var", "async", "await", "dynamic", "yield")
    );
  }

  @Override
  public CodegenType getTag() {
    return CodegenType.SERVER;
  }

  @Override
  public String getName() {
    return "dotnetcore-interfaces";
  }

  @Override
  public String getHelp() {
    return "Generates interfaces for an ASP.NET Core Web API server.";
  }

  @Override
  public void preprocessOpenAPI(OpenAPI openAPI) {
    super.preprocessOpenAPI(openAPI);
  }

  @Override
  public void processOpts() {
    super.processOpts();

    setReturnICollection(true);
    useDateTimeOffset(true);
  }

  @Override
  public String apiFileFolder() {
    return outputFolder;
  }

  @Override
  public String modelFileFolder() {
    return outputFolder;
  }

  @Override
  public Map<String, Object> postProcessSupportingFileData(Map<String, Object> objs) {
    generateJSONSpecFile(objs);
    return super.postProcessSupportingFileData(objs);
  }

  @Override
  protected void processOperation(CodegenOperation operation) {
    super.processOperation(operation);

    // HACK: Unlikely in the wild, but we need to clean operation paths for MVC Routing
    if (operation.path != null) {
      String original = operation.path;
      operation.path = operation.path.replace("?", "/");
      if (!original.equals(operation.path)) {
        LOGGER.warn("Normalized " + original + " to " + operation.path + ". Please verify generated source.");
      }
    }

    // Converts, for example, PUT to HttpPut for controller attributes
    operation.httpMethod = "Http" + operation.httpMethod.substring(0, 1) + operation.httpMethod.substring(1).toLowerCase(Locale.ROOT);
  }

  @Override
  public Mustache.Compiler processCompiler(Mustache.Compiler compiler) {
    // To avoid unexpected behaviors when options are passed programmatically such as { "useCollection": "" }
    return super.processCompiler(compiler).emptyStringIsFalse(true);
  }

  @Override
  public String toRegularExpression(String pattern) {
    return escapeText(pattern);
  }

  @Override
  protected ApiResponse findMethodResponse(ApiResponses responses) {
    for (String responseCode : responses.keySet()) {
      if (responseCode.equals("default")) {
        return responses.get(responseCode);
      }
    }
    return null;
  }
}