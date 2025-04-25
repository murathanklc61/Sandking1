using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal class RegistrySnapshot
    {
        private readonly List<string> registryPaths = new List<string>
            {
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run",
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce",
                @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run",
                @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce"
            };

        public Dictionary<string, Dictionary<string, string>> TakeSnapshot()
        {
            var snapshot = new Dictionary<string, Dictionary<string, string>>();

            foreach (var path in registryPaths)
            {
                var values = GetRegistryValues(path);
                snapshot[path] = values;
            }

            // Removed the problematic code block as it is unnecessary here
            return snapshot;
        }

        private Dictionary<string, string> GetRegistryValues(string registryPath)
        {
            var values = new Dictionary<string, string>();
            try
            {
                string hive = registryPath.Split('\\')[0];
                string subKey = registryPath.Substring(hive.Length + 1);

                RegistryKey baseKey;
                switch (hive)
                {
                    case "HKEY_CURRENT_USER":
                        baseKey = Registry.CurrentUser;
                        break;
                    case "HKEY_LOCAL_MACHINE":
                        baseKey = Registry.LocalMachine;
                        break;
                    default:
                        baseKey = null;
                        break;
                }

                if (baseKey != null)
                {
                    using (var key = baseKey.OpenSubKey(subKey, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if (key == null)
                        {
                            Console.WriteLine($"Access denied or key not found: {registryPath}");
                        }
                        else
                        {
                            ReadRegistryKey(key, registryPath, values);
                        }
                    }
                }

                if (values.Count == 0)
                {
                    Console.WriteLine($"No values found in: {registryPath}");
                    values["(No values found)"] = string.Empty;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Unauthorized access to registry path: {registryPath}. Try running as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry path {registryPath}: {ex.Message}");
            }

            return values;
        }

       private void ReadRegistryKey(RegistryKey key, string path, Dictionary<string, string> values)
{
    if (key == null)
    {
        Console.WriteLine($"Registry key is null for path: {path}");
        return;
    }

    // Anahtarın değerlerini oku
    foreach (var valueName in key.GetValueNames())
    {
        var value = key.GetValue(valueName)?.ToString() ?? string.Empty;

        // Ortam değişkenlerini çöz
        if (value.Contains("%windir%"))
        {
            value = value.Replace("%windir%", Environment.GetFolderPath(Environment.SpecialFolder.Windows));
        }

        Console.WriteLine($"Found value: {path}\\{valueName} = {value}");
        values[$"{path}\\{valueName}"] = value;
    }

    // Alt anahtarları oku
    foreach (var subKeyName in key.GetSubKeyNames())
    {
        Console.WriteLine($"Found subkey: {path}\\{subKeyName}");
        using (var subKey = key.OpenSubKey(subKeyName))
        {
            ReadRegistryKey(subKey, $"{path}\\{subKeyName}", values);
        }
    }
}
    }
}
