#addin "Cake.AWS.Lambda"
#load "module.cake"

var amazonModule = MODULE(() => 
{
  var state = new Dictionary<string, string>();

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
    }))
  };
})();