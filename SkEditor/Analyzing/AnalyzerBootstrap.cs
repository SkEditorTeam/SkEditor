using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;

namespace SkEditor.Analyzing;

internal static class AnalyzerBootstrap
{
    internal static async Task EnsureLibrariesExtractedAsync(string analyzerJarPath, string workingDir, ILogService logs)
    {
        if (!File.Exists(analyzerJarPath)) return;
        
        await using ZipArchive archive = await ZipFile.OpenReadAsync(analyzerJarPath);

        ZipArchiveEntry? listEntry = archive.GetEntry("META-INF/libraries.list");
        if (listEntry == null)
        {
            logs.Warning("META-INF/libraries.list missing in SkAnalyzer.jar!");
            return;
        }

        await using Stream listStream = await listEntry.OpenAsync();
        using StreamReader reader = new(listStream, Encoding.UTF8);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split('\t');
            if (parts.Length < 3) continue;

            string expectedHashHex = parts[0];
            string relPath = parts[2];

            string destinationPath = Path.Combine(workingDir, "libraries", relPath);

            if (IsFileValid(destinationPath, expectedHashHex))
            {
                continue;
            }

            string zipResourcePath = "META-INF/libraries/" + relPath.Replace('\\', '/');
            ZipArchiveEntry? resourceEntry = archive.GetEntry(zipResourcePath);
            if (resourceEntry == null) continue;

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            await resourceEntry.ExtractToFileAsync(destinationPath, overwrite: true);
        }
    }

    private static bool IsFileValid(string filePath, string expectedHashHex)
    {
        if (!File.Exists(filePath)) return false;

        try
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = File.OpenRead(filePath);
            byte[] hash = sha256.ComputeHash(stream);
            string actualHashHex = Convert.ToHexString(hash);

            return string.Equals(actualHashHex, expectedHashHex, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}