#load "./Paths.cake"

public class BuildParameters
{
  private const string DefaultMsBuildPath = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe";

  public string Target { get; private set; }
  public string Configuration { get; private set; }
  public string Branch { get; private set; }
  public string Solution { get; private set; }
  public string MsBuildPath { get; private set; }
  public string SonarqubeUrl { get; private set; }
  public string SonarqubeLogin { get; private set; }
  public string SonarqubePassword { get; private set; }
  public string SonarProjectKey { get; private set; }
  public string SonarProjectName { get; private set; }
  public string SonarOrganization { get; private set; }

  public bool IsLocalBuild { get; private set; }
  public bool IsMasterBranch { get; private set; }
  public bool SkipUnitTests { get; private set; }

  public int ProjectBuildTargetPlatform {get; private set;}
  public int MsBuildToolVersion {get; private set;}

  public BuildPaths Paths { get; private set; }

  public void Initialize(ICakeContext context)
  {
    context.Information("Initialize paths");
    Paths = BuildPaths.GetPaths(context, (DirectoryPath)MsBuildPath);
  }

  public static BuildParameters GetParameters(ICakeContext context)
  {
    if(context is null)
    {
      throw new ArgumentNullException("context");
    }
    
    var buildSystem = context.BuildSystem();
    var branch = buildSystem?.AppVeyor?.Environment?.Repository?.Branch ?? "master";
    
    return new BuildParameters 
    {
      Target = context.ArgumentOrEnvironmentVariable(nameof(Target), string.Empty),
      Configuration = context.ArgumentOrEnvironmentVariable(nameof(Configuration), string.Empty),
      Branch = branch,
      Solution = context.ArgumentOrEnvironmentVariable(nameof(Solution), string.Empty),
      MsBuildPath = context.ArgumentOrEnvironmentVariable(nameof(MsBuildPath), string.Empty, DefaultMsBuildPath),
      SonarqubeUrl = context.ArgumentOrEnvironmentVariable(nameof(SonarqubeUrl), string.Empty),
      SonarqubeLogin = context.ArgumentOrEnvironmentVariable(nameof(SonarqubeLogin), string.Empty),
      SonarqubePassword = context.ArgumentOrEnvironmentVariable(nameof(SonarqubePassword), string.Empty),
      SonarProjectKey = context.ArgumentOrEnvironmentVariable(nameof(SonarProjectKey), string.Empty),
      SonarProjectName = context.ArgumentOrEnvironmentVariable(nameof(SonarProjectName), string.Empty),
      SonarOrganization = context.ArgumentOrEnvironmentVariable(nameof(SonarOrganization), string.Empty),
      IsLocalBuild = buildSystem.IsLocalBuild,
      SkipUnitTests = context.ArgumentOrEnvironmentVariable(nameof(SkipUnitTests), default),
      IsMasterBranch = StringComparer.OrdinalIgnoreCase.Equals("master", branch),            
    };
  }
}