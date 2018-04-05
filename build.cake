#load "deploy-to-amazon.cake"
#load "nuget.cake"
#addin "Cake.Http"


// =================   ARGUMENTS  =================

string NIW(string str) => string.IsNullOrWhiteSpace(str) ? null : str;

var publishDir = Directory(Argument<string>("publishDir")) 
               + Directory(DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff"));

var outputDir = Directory(publishDir) 
              + Directory("bin");

var swaggerGenPath = outputDir 
                   + File("ApiGen.dll");

var apiPath = outputDir 
            + File("api.json");

var zipFile = Directory(publishDir) 
            + File(Argument<string>("lambdaName") + ".zip");

var sln = Argument<string>("sln");

var apiHost = Argument<string>("amazonApi", null) 
              ?? EnvironmentVariable("MY_AMAZON_API") 
              ?? throw new Exception("MY_AMAZON_API env variable or amazonApi param is not provided");

var amazonAK = EnvironmentVariable("MY_AMAZON_AK") ?? throw new Exception("MY_AMAZON_AK env variable is not provided");

var amazonSK = EnvironmentVariable("MY_AMAZON_SK") ?? throw new Exception("MY_AMAZON_SK env variable is not provided");

var amazonApiId = Argument<string>("forceNew", null) == "true" 
                  ? null 
                  : (Argument<string>("amazonApiId", null) ?? NIW(EnvironmentVariable("MY_AMAZON_API_ID")));

var deploymentStage = Argument<string>("deploymentStage", null);
var enableDeployment = !string.IsNullOrWhiteSpace(deploymentStage);

if (enableDeployment)
{
  Information($"API {amazonApiId} will be deployed to stage {deploymentStage}");
}

// =================   CODE  =================


CakeTaskBuilder<ActionTask> TestPost(string email) => Task("test-post").Does(async (ctx) => 
{
    Information("Waiting for 3 seconds...");
    await System.Threading.Tasks.Task.Delay(3000);
    var method = $"{apiHost}/{deploymentStage}/azaza";
    Information($"Calling {method}");
    var xx = HttpPost(method, new HttpSettings() 
    {
      RequestBody = System.Text.Encoding.UTF8.GetBytes($"{{ \"email\": \"{email}\" }}"),
      Headers = { ["Content-Type"] = "application/json" }
    });
    Information(xx);
});


// =================   WORKFLOW  =================

var createApiConfig = new PublishApiGatewayConfig() 
{
  AccessKey = amazonAK,
  SecretKey = amazonSK,
  RegionEndpoint = RegionEndpoint.USEast2,
  SwaggerApiFilePath = apiPath,
  RestApiId = amazonApiId,
  FailOnWarnings = true,
  PutMode = Amazon.APIGateway.PutMode.Overwrite
};

var deploymentConfig = new DeployApiGatewayConfig()
{
  AccessKey = amazonAK,
  SecretKey = amazonSK,
  RegionEndpoint = RegionEndpoint.USEast2,
  StageName = deploymentStage
};

var task = Task($"build-cake-root")
  .IsDependentOn(amazonModule.RestoreSolution(sln))
  .IsDependentOn(amazonModule.PublishSolution(sln, publishDir, outputDir))
  .IsDependentOn(amazonModule.ZipPublishResult(outputDir, zipFile))
  .IsDependentOn(amazonModule.GenerateSwaggerApiFile(swaggerGenPath, apiPath, "SomeApi2", "1.0.0"))
  .IsDependentOn(amazonModule.PublishLambdaToAWS(outputDir, zipFile, amazonAK, amazonSK))
  .IsDependentOn(amazonModule.CreateOrChangeApi(
    createApiConfig, 
    (result) => deploymentConfig.RestApiId = result.ApiId
   ))
  .IsDependentOn(amazonModule.DeployApi(deploymentConfig))
  .IsDependentOn(TestPost("azaza@asasd.dd"))
;

RunTarget(task.Task.Name);