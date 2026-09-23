using System.Diagnostics;

namespace Tests.Deposits.Support;

public static class FrontendDist
{
    public static string EnsureBuilt(string projectFolder = "deposits-frontend")
    {
        var root = FindRepoRoot();
        var frontend = Path.Combine(root, projectFolder);
        var dist = Path.Combine(frontend, "dist");
        var index = Path.Combine(dist, "index.html");
        var nodeModules = Path.Combine(frontend, "node_modules");

        if (!Directory.Exists(nodeModules))
        {
            Run("npm", "ci", frontend);
        }

        Run("npm", "run build", frontend);
        if (!File.Exists(index))
        {
            throw new InvalidOperationException($"Frontend build did not produce {index}");
        }

        return dist;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Bank.slnx")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not find Bank.slnx from the test output directory.");
    }

    private static void Run(string fileName, string arguments, string workingDirectory)
    {
        var start = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : fileName,
            Arguments = OperatingSystem.IsWindows() ? $"/c {fileName} {arguments}" : arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(start)
            ?? throw new InvalidOperationException($"Failed to start {fileName} {arguments}");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} {arguments} failed ({process.ExitCode}):\n{output}\n{error}");
        }
    }
}
