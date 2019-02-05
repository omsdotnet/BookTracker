public class BuildPaths
{
    public BuildDirectories Directories { get; private set; }

    public static BuildPaths GetPaths(ICakeContext context, DirectoryPath msBuildPath)
    {
        context ?? throw new ArgumentNullException("context");

        var source = DirectoryPath.FromString("./Src");
        var bin = source.Combine("./Bin");
        var toolsReports = DirectoryPath.FromString("./Tools-reports");
        var packages = DirectoryPath.FromString("./packages");
        var templates = DirectoryPath.FromString("./Build/NuspecTemplates");

        // Directories
        var buildDirectories = new BuildDirectories(DirectoryPath.FromString("./").MakeAbsolute(context.Environment), 
                                                    source.MakeAbsolute(context.Environment), 
                                                    bin.MakeAbsolute(context.Environment), 
                                                    toolsReports.MakeAbsolute(context.Environment),
                                                    msBuildPath.MakeAbsolute(context.Environment), 
                                                    packages.MakeAbsolute(context.Environment),
                                                    templates.MakeAbsolute(context.Environment));

        context.Information($"Working dir {buildDirectories.Working}");
        context.Information($"Source dir {buildDirectories.Source}");
        context.Information($"Bin dir {buildDirectories.Bin}");
        context.Information($"ToolsReports dir {buildDirectories.ToolsReports}");
        context.Information($"MsBuild dir {buildDirectories.MsBuildPath}");
        context.Information($"Packages dir {buildDirectories.Packages}");
        context.Information($"Templates dir {buildDirectories.Templates}");

        return new BuildPaths
        {
            Directories = buildDirectories
        };
    }
}

public class BuildDirectories
{
    public DirectoryPath Source { get; private set; }
    public DirectoryPath Bin { get; private set; }
    public DirectoryPath ToolsReports { get; private set; }
    public DirectoryPath MsBuildPath { get; private set; }
    public DirectoryPath Working {get; private set;}
    public DirectoryPath Packages {get; private set;}
    public DirectoryPath Templates {get; private set;}

    public BuildDirectories(DirectoryPath working, 
                            DirectoryPath source, 
                            DirectoryPath bin, 
                            DirectoryPath toolsReports, 
                            DirectoryPath nugetRoot, 
                            DirectoryPath msBuildPath, 
                            DirectoryPath packagesPath, 
                            DirectoryPath templates)
    {
        Working = working;
        Source = source;
        Bin = bin;
        ToolsReports = toolsReports;
        NugetRoot = nugetRoot;
        MsBuildPath = msBuildPath;
        Packages = packagesPath;
        Templates = templates;
    }
}