using System;
using System.ServiceProcess;
using System.IO;
using System.Text;

namespace WindowsFormsApp1
{
    internal class ServicesLogger
    {
        public static void LogAllServices(string outputPath)
        {
            StringBuilder serviceInfo = new StringBuilder();

            try
            {
                ServiceController[] services = ServiceController.GetServices();

                foreach (ServiceController service in services)
                {
                    serviceInfo.AppendLine($"Service Name: {service.ServiceName}");
                    serviceInfo.AppendLine($"Display Name: {service.DisplayName}");
                    serviceInfo.AppendLine($"Status: {service.Status}");
                    serviceInfo.AppendLine("----------------------------------");
                }

                File.WriteAllText(outputPath, serviceInfo.ToString());
            }
            catch (Exception ex)
            {
                File.WriteAllText(outputPath, $"An error occurred while retrieving services: {ex.Message}");
            }
        }

        public static void LogAllServiceNames(string outputPath)
        {
            StringBuilder serviceNames = new StringBuilder();

            try
            {
                ServiceController[] services = ServiceController.GetServices();

                foreach (ServiceController service in services)
                {
                    serviceNames.AppendLine(service.ServiceName);
                }

                File.WriteAllText(outputPath, serviceNames.ToString());
            }
            catch (Exception ex)
            {
                File.WriteAllText(outputPath, $"An error occurred while retrieving service names: {ex.Message}");
            }
        }
    }
}
