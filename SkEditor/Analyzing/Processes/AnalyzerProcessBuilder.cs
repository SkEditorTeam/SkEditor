using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SkEditor.Analyzing.Processes;

public class AnalyzerProcessBuilder : IAnalyzerProcessBuilder
{
    public ProcessStartInfo BuildProcessStartInfo(string workingDir, string javaPath, string analyzerPath,
        string? aotPath, string[] libraryPaths,
        bool isGeneratingAot = false)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = javaPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDir,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        startInfo.ArgumentList.Add("-Dskanalyzer.parentProcess=" + Environment.ProcessId);

        if (isGeneratingAot)
        {
            startInfo.ArgumentList.Add($"-XX:AOTCacheOutput={aotPath}");
        }
        else if (!string.IsNullOrEmpty(aotPath) && File.Exists(aotPath))
        {
            startInfo.ArgumentList.Add($"-XX:AOTCache={aotPath}");
        }

        startInfo.ArgumentList.Add("--enable-native-access=ALL-UNNAMED");

        startInfo.ArgumentList.Add("-XX:+UseStringDeduplication");
        startInfo.ArgumentList.Add("-XX:+DisableExplicitGC");
        startInfo.ArgumentList.Add("-XX:+UseCompactObjectHeaders");

        string libsDir = Path.Combine(workingDir, "libraries");
        string classpath;

        if (Directory.Exists(libsDir))
        {
            IEnumerable<string> relativeJars = libraryPaths.Select(p => Path.GetRelativePath(workingDir, p));
            string relativeAnalyzerPath = Path.GetRelativePath(workingDir, analyzerPath);

            string separator = Path.PathSeparator.ToString();
            classpath = string.Join(separator, relativeJars) + separator + relativeAnalyzerPath;
        }
        else
        {
            classpath = analyzerPath;
        }

        startInfo.ArgumentList.Add("-cp");
        startInfo.ArgumentList.Add(classpath);
        startInfo.ArgumentList.Add("me.glicz.skanalyzer.shell.SkAnalyzerShell");

        string? binDir = Directory.GetParent(analyzerPath)?.FullName;
        string? skriptJar = binDir != null
            ? Path.Combine(binDir, "Skript.jar")
            : null;

        if (File.Exists(skriptJar))
        {
            startInfo.ArgumentList.Add($"--add-plugin={skriptJar}");
        }

        return startInfo;
    }
}