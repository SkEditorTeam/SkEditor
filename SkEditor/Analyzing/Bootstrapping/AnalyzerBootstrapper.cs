using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Core.Logging;

namespace SkEditor.Analyzing.Bootstrapping;

public class AnalyzerBootstrapper(ILogService logs) : IAnalyzerBootstrapper
{
    public async Task<string[]?> EnsureLibrariesExtractedAsync(string analyzerJarPath, string workingDir,
        CancellationToken ct = default)
    {
        if (!File.Exists(analyzerJarPath))
        {
            return null;
        }

        try
        {
            await using ZipArchive archive = await ZipFile.OpenReadAsync(analyzerJarPath, ct);

            ZipArchiveEntry? listEntry = archive.GetEntry("META-INF/libraries.list");
            if (listEntry == null)
            {
                logs.Warning("META-INF/libraries.list missing in SkAnalyzer.jar!", LogSource.Analyzer);
                return null;
            }

            await using Stream listStream = await listEntry.OpenAsync(ct);
            using StreamReader reader = new(listStream, Encoding.UTF8);

            string librariesFolder = Path.Combine(workingDir, "libraries");
            HashSet<string> validPaths = new(StringComparer.OrdinalIgnoreCase);

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length < 3)
                {
                    continue;
                }

                string expectedHashHex = parts[0];
                string relPath = parts[2];
                string destinationPath = Path.Combine(librariesFolder, relPath);
                validPaths.Add(Path.GetFullPath(destinationPath));

                string fullLibsFolder = Path.GetFullPath(librariesFolder);
                if (!fullLibsFolder.EndsWith(Path.DirectorySeparatorChar))
                {
                    fullLibsFolder += Path.DirectorySeparatorChar;
                }

                if (!Path.GetFullPath(destinationPath).StartsWith(fullLibsFolder, StringComparison.OrdinalIgnoreCase))
                {
                    logs.Warning("Skipping extraction of library with invalid path: " + relPath, LogSource.Analyzer);
                    continue;
                }

                bool isValid = await IsFileValid(destinationPath, expectedHashHex, ct);
                if (isValid)
                {
                    continue;
                }

                string zipResourcePath = "META-INF/libraries/" + relPath.Replace('\\', '/');
                ZipArchiveEntry? resourceEntry = archive.GetEntry(zipResourcePath);
                if (resourceEntry == null)
                {
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await resourceEntry.ExtractToFileAsync(destinationPath, true, ct);
            }
            
            CleanUpOldJars(librariesFolder, validPaths);

            return [.. validPaths];
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to extract libraries from SkAnalyzer.jar", LogSource.Analyzer);
            return null;
        }
    }

    private void CleanUpOldJars(string librariesFolder, HashSet<string> validPaths)
    {
        if (!Directory.Exists(librariesFolder))
        {
            return;
        }

        foreach (string file in Directory.GetFiles(librariesFolder, "*.jar", SearchOption.AllDirectories))
        {
            if (validPaths.Contains(Path.GetFullPath(file)))
            {
                continue;
            }

            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                logs.Error(ex, $"Could not delete invalid library file '{file}'", LogSource.Analyzer);
            }
        }
    }

    private static async Task<bool> IsFileValid(string filePath, string expectedHashHex, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            await using FileStream stream = File.OpenRead(filePath);
            byte[] hash = await SHA256.HashDataAsync(stream, ct);
            string actualHashHex = Convert.ToHexString(hash);

            return string.Equals(actualHashHex, expectedHashHex, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}