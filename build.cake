#load "deploy-to-amazon.cake"
#load "nuget.cake"
#addin "Cake.Http"


// =================   ARGUMENTS  =================


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
var apiHost = EnvironmentVariable("MY_AMAZON_API") ?? throw new Exception("MY_AMAZON_API env variable is not provided");
var amazonAK = EnvironmentVariable("MY_AMAZON_AK") ?? throw new Exception("MY_AMAZON_AK env variable is not provided");
var amazonSK = EnvironmentVariable("MY_AMAZON_SK") ?? throw new Exception("MY_AMAZON_SK env variable is not provided");


// =================   CODE  =================


CakeTaskBuilder<ActionTask> TestPost(string apiHost, string email) => Task("test-post").Does((ctx) => 
{
    var xx = HttpPost($"{apiHost}/XXX/xxx", new HttpSettings() 
    {
      RequestBody = System.Text.Encoding.UTF8.GetBytes($"{{ \"email\": \"{email}\" }}"),
      Headers = { ["Content-Type"] = "application/json" }
    });
    Information(xx);
});


// =================   WORKFLOW  =================

var task = Task($"build-cake-root")
  .IsDependentOn(amazonModule.RestoreSolution(sln))
  .IsDependentOn(amazonModule.PublishSolution(sln, publishDir, outputDir))
  .IsDependentOn(amazonModule.ZipPublishResult(outputDir, zipFile))
  .IsDependentOn(amazonModule.GenerateSwaggerApiFile(swaggerGenPath, apiPath))
  .IsDependentOn(amazonModule.PublishSwaggerApiFile(apiPath, amazonAK, amazonSK))
 // .IsDependentOn(amazonModule.PublishToAmazon(outputDir, zipFile, amazonAK, amazonSK))
 // .IsDependentOn(TestPost(apiHost, "azaza@asasd.dd"))
;

RunTarget(task.Task.Name);