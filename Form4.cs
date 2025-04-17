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

            // File paths for driver and service logs
            string driverOutputPath = Path.Combine(desktopPath, $"driver_snapshot_{timestamp}.txt");
            string allServicesPath = Path.Combine(desktopPath, $"all_services_{timestamp}.txt");
            string serviceNamesPath = Path.Combine(desktopPath, $"service_names_{timestamp}.txt");

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

                // Show success message
                MessageBox.Show(
                    $"Logs have been saved successfully:\n{driverOutputPath}\n{allServicesPath}\n{serviceNamesPath}",
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
