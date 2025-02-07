using System;
using System.IO;
using System.Reflection;

namespace Mod.DynamicEncounters.Common.Helpers;

public static class ResourceLoader
{
    public static string GetContents(string resourceName)
    {
        return GetContents(Assembly.GetExecutingAssembly(), resourceName);
    }
    
    public static string GetContents(Assembly assembly, string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            throw new ArgumentException("Resource name cannot be null or empty.", nameof(resourceName));
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new ArgumentException($"Resource '{resourceName}' not found.", nameof(resourceName));
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}