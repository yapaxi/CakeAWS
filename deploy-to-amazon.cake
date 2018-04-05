#addin "Cake.AWS.Lambda"
#load "module.cake"
#addin nuget:http://localhost:8624/nuget/Nuget/?package=AWSSDK.Core&version=3.3.21.20
#addin nuget:http://localhost:8624/nuget/Nuget/?package=AWSSDK.APIGateway&version=3.3.16.3
#addin nuget:http://localhost:8624/nuget/Nuget/?package=Cake.AWS.APIGateway&version=1.0.8.0

class CreateApiResult {public string ApiId {get;set;}}

var amazonModule = MODULE(() => 
{
  var state = new Dictionary<string, Dictionary<string, string>>();

  return new 
  {
    RestoreSolution = METHOD((string sln) => UniqueTask("restore").Does((ctx) => 
    {
      NuGetRestore(sln);
    })),

    PublishSolution = METHOD((string sln, string publishDir, string outputDir) => UniqueTask("publish").Does((ctx) => 
    {
      if (DirectoryExists(publishDir))
      {
        DeleteDirectory(publishDir, recursive: true);
      }
      CreateDirectory(publishDir);
      CreateDirectory(outputDir);
      DotNetCorePublish(sln, new DotNetCorePublishSettings()
      {
        NoRestore = true,
        Configuration = "Release",
        OutputDirectory = outputDir
      });
    })),

    ZipPublishResult = METHOD((string outputDir, string zipFile) => UniqueTask("zip").Does((ctx) => 
    {
      Zip(outputDir, zipFile);
    })),

    PublishLambdaToAWS = METHOD((string outputDir, string zipFile, string amazonAK, string amazonSK) => UniqueTask("publish-to-amazon").Does(async (ctx) => 
    {
        var settings = new UpdateFunctionCodeSettings()
        {
            AccessKey = amazonAK ?? throw new ArgumentNullException(nameof(amazonAK)),
            SecretKey = amazonSK ?? throw new ArgumentNullException(nameof(amazonSK)),
            Region = RegionEndpoint.USEast2,
            ZipPath = zipFile
        };

        var version = await UpdateLambdaFunctionCode("TestFunction", settings);
    })),

    GenerateSwaggerApiFile = METHOD((string swaggerApiGeneratorPath, string generatedSwaggerApiPath, string apiName, string apiVersion) => UniqueTask("generate-swagger-api-file").Does(async (ctx) => 
    {
      DotNetCoreExecute(swaggerApiGeneratorPath, ProcessArgumentBuilder.FromString($"\"{generatedSwaggerApiPath}\" \"{apiName}\" \"{apiVersion}\""));
    })),

    CreateOrChangeApi = METHOD((PublishApiGatewayConfig config, Action<CreateApiResult> output) => UniqueTask("create-or-change-api-aws-api-gateway").Does(async (ctx) => 
    {
      var apiId = await CreateOrChangeRestApi(config);
      output?.Invoke(new CreateApiResult() { ApiId = apiId });
    })),

    DeployApi = METHOD((DeployApiGatewayConfig config) => UniqueTask("deploy-api-aws-api-gateway").Does(async (ctx) => 
    {
      await DeployRestApi(config);
    }))
  };
})();