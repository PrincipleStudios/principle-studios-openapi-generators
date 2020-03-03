package org.openapitools.codegen;

import com.samskivert.mustache.Mustache;
import io.swagger.v3.oas.models.OpenAPI;
import org.openapitools.codegen.*;
import org.openapitools.codegen.languages.*;
import io.swagger.models.properties.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.*;
import java.io.File;

public class DotNetCoreInterfacesGenerator extends AbstractCSharpCodegen {
    protected Logger LOGGER = LoggerFactory.getLogger(AspNetCoreServerCodegen.class);

  // source folder where to write the files
  protected String sourceFolder = "src";
  protected String apiVersion = "1.0.0";

  public DotNetCoreInterfacesGenerator() {
    super();

    outputFolder = "generated-code" + File.separator + getName();

    modelTemplateFiles.put("model.mustache", ".cs");
    apiTemplateFiles.put("controller.mustache", ".cs");

    embeddedTemplateDir = templateDir = "dotnetcore-interfaces";

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
}