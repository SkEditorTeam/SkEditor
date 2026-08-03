using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SkEditor.Analyzing.Processes;

public class AnalyzerProcessBuilder : IAnalyzerProcessBuilder
{
    public ProcessStartInfo BuildProcessStartInfo(string workingDir, string analyzerPath, string? aotPath, bool isGeneratingAot = false)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "java",
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

        string libsDir = Path.Combine(workingDir, "libraries");
        string classpath;

        if (Directory.Exists(libsDir))
        {
            string[] jarFiles = Directory.GetFiles(libsDir, "*.jar", SearchOption.AllDirectories);
            Array.Sort(jarFiles);

            IEnumerable<string> relativeJars = jarFiles.Select(p => Path.GetRelativePath(workingDir, p));
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

        return startInfo;
    }
}