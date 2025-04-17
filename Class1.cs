using System;
using System.IO;
using System.Management;
using System.Text;

namespace WindowsFormsApp1
{
    public static class DriverLogger
    {
        public static string GetAllSystemDrivers()
        {
            StringBuilder driverInfo = new StringBuilder();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemDriver");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string sysFile = queryObj["PathName"]?.ToString();
                    if (sysFile != null && sysFile.EndsWith(".sys"))
                    {
                        driverInfo.AppendLine($"System Driver SYS File: {sysFile}");
                    }

                    driverInfo.AppendLine($"Driver Name: {queryObj["Name"]}");
                    driverInfo.AppendLine($"Driver State: {queryObj["State"]}");
                    driverInfo.AppendLine($"Driver Start Mode: {queryObj["StartMode"]}");
                    driverInfo.AppendLine($"Driver Status: {queryObj["Status"]}");
                    driverInfo.AppendLine("----------------------------------");
                }
            }
            catch (Exception ex)
            {
                driverInfo.AppendLine($"Hata oluştu: {ex.Message}");
            }

            return driverInfo.ToString();
        }

        public static string GetAllInfFiles()
        {
            StringBuilder infFiles = new StringBuilder();
            string infDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "INF");

            try
            {
                if (Directory.Exists(infDirectory))
                {
                    string[] infFilesList = Directory.GetFiles(infDirectory, "*.inf");
                    foreach (var file in infFilesList)
                    {
                        infFiles.AppendLine(file);
                    }
                }
                else
                {
                    infFiles.AppendLine("INF klasörü bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                infFiles.AppendLine($"Hata oluştu: {ex.Message}");
            }

            return infFiles.ToString();
        }
    }
}
