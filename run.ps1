Param(
    [switch]$ForceNew,
    [string]$AmazonApi,
    [string]$AmazonApiId
)

$args = '-sln="C:\Users\igor.tishkin\Documents\Visual Studio 2017\Projects\TestFunction\LambdaTest.sln"'`
      , '--settings_skipverification=true'`
      , '-publishDir="c:\__entry\_publishDir\"'`
      , '-lambdaName="LambdaTest"'

if ($ForceNew.IsPresent) {
  $args = $args + "--forceNew=true"
}

if ($AmazonApi) {
  $args = $args + "-amazonApi=`"$AmazonApi`""
}

if ($AmazonApiId) {
  $args = $args + "-amazonApiId=`"$AmazonApiId`""
}

.\build.ps1 -ScriptArgs $args