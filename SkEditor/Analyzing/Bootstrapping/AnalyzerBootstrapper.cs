using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;

namespace SkEditor.Analyzing.Bootstrapping;

public class AnalyzerBootstrapper(ILogService logs) : IAnalyzerBootstrapper
{
    public async Task<bool> EnsureLibrariesExtractedAsync(string analyzerJarPath, string workingDir, CancellationToken ct = default)
    {
        if (!File.Exists(analyzerJarPath)) return false;

        try
        {
            await using ZipArchive archive = await ZipFile.OpenReadAsync(analyzerJarPath, ct);

            ZipArchiveEntry? listEntry = archive.GetEntry("META-INF/libraries.list");
            if (listEntry == null)
            {
                logs.Warning("META-INF/libraries.list missing in SkAnalyzer.jar!");
                return false;
            }

            await using Stream listStream = await listEntry.OpenAsync(ct);
            using StreamReader reader = new(listStream, Encoding.UTF8);

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split('\t');
                if (parts.Length < 3) continue;

                string expectedHashHex = parts[0];
                string relPath = parts[2];
                string destinationPath = Path.Combine(workingDir, "libraries", relPath);

                bool isValid = await IsFileValid(destinationPath, expectedHashHex, ct);
                if (isValid)
                {
                    continue;
                }

                string zipResourcePath = "META-INF/libraries/" + relPath.Replace('\\', '/');
                ZipArchiveEntry? resourceEntry = archive.GetEntry(zipResourcePath);
                if (resourceEntry == null) continue;

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await resourceEntry.ExtractToFileAsync(destinationPath, overwrite: true, ct);
            }

            return true;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to extract libraries from SkAnalyzer.jar");
            return false;
        }
    }

    private static async Task<bool> IsFileValid(string filePath, string expectedHashHex, CancellationToken ct = default)
    {
        if (!File.Exists(filePath)) return false;

        try
        {
            using SHA256 sha256 = SHA256.Create();
            await using FileStream stream = File.OpenRead(filePath);
            byte[] hash = await sha256.ComputeHashAsync(stream, ct);
            string actualHashHex = Convert.ToHexString(hash);

            return string.Equals(actualHashHex, expectedHashHex, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}