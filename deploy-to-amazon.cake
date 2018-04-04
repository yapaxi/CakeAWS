#addin "Cake.AWS.Lambda"
#load "module.cake"
#addin nuget:http://localhost:8624/nuget/Nuget/?package=AWSSDK.Core&version=3.3.21.20
#addin nuget:http://localhost:8624/nuget/Nuget/?package=AWSSDK.APIGateway&version=3.3.16.3
#addin nuget:http://localhost:8624/nuget/Nuget/?package=Cake.AWS.APIGateway&version=1.0.4.0

var amazonModule = MODULE(() => 
{
  var state = new Dictionary<string, object>();

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

    PublishToAmazon = METHOD((string outputDir, string zipFile, string amazonAK, string amazonSK) => UniqueTask("publish-to-amazon").Does(async (ctx) => 
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

    PublishSwaggerApiFile = METHOD((string generatedSwaggerApiPath, string amazonAK, string amazonSK, string amazonApiId) => UniqueTask("publish-swagger-api-file").Does(async (ctx) => 
    {
      var model = new PublishApiGatewayConfig() {
        AccessKey = amazonAK,
        SecretKey = amazonSK,
        RegionEndpoint = RegionEndpoint.USEast2
      };

      await PublishRestApi(generatedSwaggerApiPath, model, restApiId: amazonApiId);
    }))
  };
})();