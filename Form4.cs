using DriverLoggerNameSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }
        private void takeButton_Click(object sender, EventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // File paths for driver, service, and registry logs
            string driverOutputPath = Path.Combine(desktopPath, $"DriverNew_{timestamp}.txt");
            string allServicesPath = Path.Combine(desktopPath, $"all_services_{timestamp}.txt");
            string serviceNamesPath = Path.Combine(desktopPath, $"ServicesNew_{timestamp}.txt");
            string registrySnapshotPath = Path.Combine(desktopPath, $"RegistryNew_{timestamp}.txt");

            try
            {
                // Log driver information
                StringBuilder result = new StringBuilder();
                result.AppendLine("=== Tüm Yüklenmiş Sürücüler ===");
                result.AppendLine(DriverLogger.GetAllSystemDrivers());
                result.AppendLine();
                result.AppendLine("=== Tüm INF Dosyaları ===");
                result.AppendLine(DriverLogger.GetAllInfFiles());
                File.WriteAllText(driverOutputPath, result.ToString());

                // Log all services and their names
                ServicesLogger.LogAllServices(allServicesPath);
                ServicesLogger.LogAllServiceNames(serviceNamesPath);

                // Take registry snapshot
                var registrySnapshot = new RegistrySnapshot();
                var snapshotData = registrySnapshot.TakeSnapshot();

                // Save registry snapshot to file
                using (var writer = new StreamWriter(registrySnapshotPath))
                {
                    foreach (var path in snapshotData.Keys)
                    {
                        writer.WriteLine($"Registry Path: {path}");
                        foreach (var kvp in snapshotData[path])
                        {
                            writer.WriteLine($"  {kvp.Key}: {kvp.Value}");
                        }
                        writer.WriteLine();
                    }
                }

                // Show success message
                MessageBox.Show(
                    $"Logs have been saved successfully:\n{driverOutputPath}\n{allServicesPath}\n{serviceNamesPath}\n{registrySnapshotPath}",
                    "Completed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving logs:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
