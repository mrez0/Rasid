using System;
using System.IO;

namespace Rasid.App;

public static class AppPaths
{
    public static string DataFolder { get; set; } = CreateDataFolder();
    public static string DatabaseFile { get; set; } = Path.Combine(DataFolder, "rasid.db");
    public static string LogFolder { get; set; } = Path.Combine(DataFolder, "logs");

    private static string CreateDataFolder()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(root, "Rasid");
        Directory.CreateDirectory(folder);
        return folder;
    }
}