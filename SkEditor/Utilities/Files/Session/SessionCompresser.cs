using System;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace SkEditor.Utilities.Files;

public static class SessionCompresser
{
    public const int MaxCompressionSize = 50 * 1024 * 1024; // 50 MB
    
    public static async Task<string> Compress(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return string.Empty;
        }

        byte[] byteArray = Encoding.UTF8.GetBytes(data);
        if (byteArray.Length > MaxCompressionSize)
        {
            throw new ArgumentException($"Data size exceeds maximum allowed size ({MaxCompressionSize} bytes)");
        }

        try
        {
            using MemoryStream ms = new();
            await using (GZipStream sw = new(ms, CompressionLevel.Optimal, true))
            {
                await sw.WriteAsync(byteArray);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error compressing data");
            throw;
        }
    }
    
    public static async Task<string> Decompress(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return string.Empty;
        }

        try
        {
            byte[] byteArray = Convert.FromBase64String(data);
            
            if (byteArray.Length > MaxCompressionSize)
            {
                throw new SecurityException("Compressed data exceeds maximum allowed size");
            }

            using MemoryStream ms = new(byteArray);
            await using GZipStream sr = new(ms, CompressionMode.Decompress);
            
            using MemoryStream resultStream = new();
            await sr.CopyToAsync(resultStream);
            resultStream.Position = 0;
            
            if (resultStream.Length > MaxCompressionSize)
            {
                throw new SecurityException("Decompressed data exceeds maximum allowed size");
            }
            
            using StreamReader reader = new(resultStream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
        catch (FormatException ex)
        {
            Log.Warning(ex, "Invalid format while decompressing data");
            return string.Empty;
        }
        catch (InvalidDataException ex)
        {
            Log.Warning(ex, "Invalid compressed data");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error decompressing data");
            return string.Empty;
        }
    }
}