///////////////////////////////////////////////////////////////////////////////
// Tools
///////////////////////////////////////////////////////////////////////////////
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.2.0
#tool nuget:?package=NUnit.ConsoleRunner&version=3.9.0
#tool nuget:?package=JetBrains.dotCover.CommandLineTools&version=2018.3.1

///////////////////////////////////////////////////////////////////////////////
// AddIn
///////////////////////////////////////////////////////////////////////////////
#addin nuget:?package=Cake.Sonar&version=1.1.16
#addin nuget:?package=Cake.ArgumentHelpers&version=0.3.0

///////////////////////////////////////////////////////////////////////////////
// Load custom classes
///////////////////////////////////////////////////////////////////////////////
#load "./Build/Parameters.cake"

///////////////////////////////////////////////////////////////////////////////
// Arguments
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// Variables
///////////////////////////////////////////////////////////////////////////////
BuildParameters parameters = BuildParameters.GetParameters(Context);
SonarBeginSettings sonarqubeBeginSettings = null;
DotNetCoreBuildSettings coreBuildSettings = null;
DotNetCoreTestSettings  coreTestsSettings = null;
DotCoverAnalyseSettings dotCoverAnalyseSettings = null;
SonarEndSettings sonarqubeEndSettings = null;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{   
  Information("Initialize Parameters");
  parameters.Initialize(context);

  Information($"Initialize { nameof(DotNetCoreBuildSettings) }");
  coreBuildSettings= new DotNetCoreBuildSettings()
  {
    Framework = "netcoreapp2.1",
    Configuration = parameters.Configuration,
    OutputDirectory = parameters.Paths.Directories.Bin
  };

  Information("Initialize NUnit3Settings");
  coreTestsSettings = new DotNetCoreTestSettings () 
  {
    Configuration = parameters.Configuration,
    Filter = "TestCategory=Unit",
    NoBuild = true,
    OutputDirectory = parameters.Paths.Directories.Bin
  };

  Information("Initialize DotCoverAnalyseSettings");
  dotCoverAnalyseSettings = new DotCoverAnalyseSettings()
  {
    ReportType = DotCoverReportType.HTML
  };

  Information("Initialize SonarBeginSettings");
  sonarqubeBeginSettings = new SonarBeginSettings() 
  {
    Url = parameters.SonarqubeUrl,
    Login = parameters.SonarqubeLogin,
    Password = parameters.SonarqubePassword,
    Key = parameters.SonarProjectKey,
    Name = parameters.SonarProjectName,
    Organization= parameters.SonarOrganization,
    Branch = parameters.Branch,
    NUnitReportsPath = parameters.Paths.Directories.ToolsReports.Combine(Directory("./UnitResultsReport.xml")).FullPath,
    DotCoverReportsPath = parameters.Paths.Directories.ToolsReports.Combine(Directory("./DotCoverReport.html")).FullPath
  };

  Information("Initialize SonarEndSettings");
  sonarqubeEndSettings = sonarqubeBeginSettings.GetEndSettings();
});


Teardown(context =>
{
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("RestorePackages")
.Does(() =>
{
  NuGetRestore(parameters.Paths.Directories.Source.Combine(parameters.Solution).FullPath);
});

Task("InitializeSonar")
  .Does(() => 
{
  SonarBegin(sonarqubeBeginSettings);
});

Task("Build")
  .IsDependentOn("RestorePackages")
  .Does(() =>
{
   DotNetCoreBuild(parameters.Paths.Directories.Source.Combine(parameters.Solution).FullPath, coreBuildSettings);
});

Task("RunUnitTests")
  .IsDependentOn("InitializeSonar")
  .IsDependentOn("Build")
  .Does(() =>
{
  if (!DirectoryExists(parameters.Paths.Directories.ToolsReports.FullPath))
  {
    CreateDirectory(parameters.Paths.Directories.ToolsReports.FullPath);
  }
  
  DotCoverAnalyse(tool => 
  {
     var projectFiles = GetFiles(parameters.Paths.Directories.Source.FullPath + "/**/*.Tests.csproj");
     foreach(var file in projectFiles)
     {
       tool.DotNetCoreTest(
         file.FullPath, 
         coreTestsSettings);
     }
  }, 
  parameters.Paths.Directories.ToolsReports.Combine("./DotCoverReport.html").FullPath, 
  dotCoverAnalyseSettings.WithFilter("+:BookTracker")
                         .WithFilter("-:BookTracker.Tests"));
});

Task("StaticAnalysis")
  .IsDependentOn("RunUnitTests")
  .Does(() =>
{
  SonarEnd(sonarqubeEndSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////


Task("Default")
  .IsDependentOn("StaticAnalysis");

RunTarget(parameters.Target);