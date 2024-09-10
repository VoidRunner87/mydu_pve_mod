using System;
using System.IO;
using System.Reflection;

namespace Mod.DynamicEncounters.Common;

public static class ResourceLoader
{
    public static string GetContents(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            throw new ArgumentException("Resource name cannot be null or empty.", nameof(resourceName));
        }

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new ArgumentException($"Resource '{resourceName}' not found.", nameof(resourceName));
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}