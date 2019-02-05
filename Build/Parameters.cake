#load "./Paths.cake"

public class BuildParameters
{
    public string Target { get; private set; }
    public string Configuration { get; private set; }
    public string Branch { get; private set; }
    public string ProjectKey { get; private set; }
    public string ProjectName { get; private set; }
    public string Solution { get; private set; }
    public string MsBuildPath { get; private set; }
    public string SonarqubeUrl { get; private set; }
    public string SonarqubeLogin { get; private set; }
    public string SonarqubePassword { get; private set; }

    public bool IsLocalBuild { get; private set; }
    public bool IsMasterBranch { get; private set; }
    
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
        context ?? throw new ArgumentNullException("context");

        var buildSystem = context.BuildSystem();

        return new BuildParameters 
        {
            Target = context.Argument("Target", "Default"),
            Configuration = context.Argument("ProjectBuildConfiguration", "Release"),
            Branch = buildSystem.AppVeyor.Environment.Repository.Branch,
            ProjectKey = context.Argument("ProjectKey", "BookTracker"),
            ProjectName = context.Argument("ProjectName", "BookTracker"),
            Solution = context.Argument("Solution", "BookTracker.sln"),
            MsBuildPath = context.Argument("MsBuildPath", @"C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"),
            SonarqubeUrl = context.Argument("SonarqubeUrl", ""),
            SonarqubeLogin = context.Argument("SonarqubeLogin", ""),
            SonarqubePassword = context.Argument("SonarqubePassword", ""),
            IsLocalBuild = buildSystem.IsLocalBuild,
            IsMasterBranch = StringComparer.OrdinalIgnoreCase.Equals("master", Branch),            
        };
    }
}