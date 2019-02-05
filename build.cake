///////////////////////////////////////////////////////////////////////////////
// Tools
///////////////////////////////////////////////////////////////////////////////
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool
#tool nuget:?package=NUnit.ConsoleRunner
#tool nuget:?package=JetBrains.dotCover.CommandLineTools

///////////////////////////////////////////////////////////////////////////////
// AddIn
///////////////////////////////////////////////////////////////////////////////
#addin nuget:?package=Cake.Sonar

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
MSBuildSettings msBuildSettings = null;
NUnit3Settings nUnit3Settings = null;
DotCoverAnalyseSettings dotCoverAnalyseSettings = null;
SonarEndSettings sonarqubeEndSettings = null;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{   
  Information("Initialize Parameters");
  parameters.Initialize(context);

  Information("Initialize MSBuildSettings");
  msBuildSettings = new MSBuildSettings() 
  { 
    ToolPath = parameters.MsBuildPath,
    MaxCpuCount = Environment.ProcessorCount,
    Configuration = parameters.Configuration,
    PlatformTarget = (PlatformTarget)parameters.ProjectBuildTargetPlatform,
    ToolVersion = (MSBuildToolVersion)parameters.MsBuildToolVersion
  };

  Information("Initialize NUnit3Settings");
  nUnit3Settings = new NUnit3Settings() 
  {
    Agents = Environment.ProcessorCount,
    Results = new List<NUnit3Result>()
    {
      new NUnit3Result 
      {
        FileName = parameters.Paths.Directories.ToolsReports.Combine("./UnitResultsReport.xml").FullPath
      }
    },
    Where = "cat = Unit"
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
    Key = parameters.ProjectKey,
    Name = parameters.ProjectName,
    Branch = parameters.Branch,
    NUnitReportsPath = parameters.Paths.Directories.ToolsReports.Combine(Directory("./UnitResultsReport.xml")).FullPath,
    DotCoverReportsPath = parameters.Paths.Directories.ToolsReports.Combine(Directory("./DotCoverReport.html")).FullPath
  };

  Information("Initialize SonarEndSettings");
  sonarqubeEndSettings = new SonarEndSettings()
  {
    Login = parameters.SonarqubeLogin,
    Password = parameters.SonarqubePassword
  };
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
  NuGetRestore(parameters.Solution);
});

Task("InitializeSonar")
  .Does(() => 
{
  SonarBegin(sonarqubeBeginSettings);
});

Task("Build")
  .IsDependentOn("RestorePackages")
  .IsDependentOn("InitializeSonar")
  .Does(() =>
{
  MSBuild(parameters.Paths.Directories.Source.Combine(parameters.Solution).FullPath, msBuildSettings);
});

Task("RunUnitTests")
  .IsDependentOn("Build")
  .Does(() =>
{
  if (!DirectoryExists(parameters.Paths.Directories.ToolsReports.FullPath))
  {
    CreateDirectory(parameters.Paths.Directories.ToolsReports.FullPath);
  }
  
  DotCoverAnalyse(tool => 
  {
    tool.NUnit3(parameters.Paths.Directories.Bin.FullPath + "/**/*.Tests.dll", nUnit3Settings);
  }, parameters.Paths.Directories.ToolsReports.Combine("./DotCoverReport.html").FullPath, dotCoverAnalyseSettings);
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