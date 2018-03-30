#addin "Cake.AWS.Lambda"


CakeTaskBuilder<ActionTask> RestoreSolution(string sln) => Task("restore").Does((ctx) => 
{
  NuGetRestore(sln);
});

CakeTaskBuilder<ActionTask> PublishSolution(string sln, string publishDir, string outputDir) => Task("publish").Does((ctx) => 
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
});

CakeTaskBuilder<ActionTask> ZipPublishResult(string outputDir, string zipFile) => Task("zip").Does((ctx) => 
{
  Zip(outputDir, zipFile);
});

CakeTaskBuilder<ActionTask> PublishToAmazon(string outputDir, string zipFile, string amazonAK, string amazonSK) => Task("publish-to-amazon").Does(async (ctx) => 
{
    var settings = new UpdateFunctionCodeSettings()
    {
        AccessKey = amazonAK ?? throw new ArgumentNullException("Access key is not provided"),
        SecretKey = amazonSK ?? throw new ArgumentNullException("Secret key is not provided"),
        Region = RegionEndpoint.USEast2,
        ZipPath = zipFile
    };

    var version = await UpdateLambdaFunctionCode("TestFunction", settings);
});