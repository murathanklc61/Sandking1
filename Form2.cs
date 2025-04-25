using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using DriverLoggerNameSpace;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e) { }

        private void button2_click(object sender, EventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // File paths for driver and service logs
            string driverOutputPath = Path.Combine(desktopPath, $"DriverOld_{timestamp}.txt");
            string serviceNamesPath = Path.Combine(desktopPath, $"ServicesOld_{timestamp}.txt");
            string registrySnapshotPath = Path.Combine(desktopPath, $"RegistryOld_{timestamp}.txt");

            StringBuilder result = new StringBuilder();

            // Log driver information
            result.AppendLine("=== Tüm Yüklenmiş Sürücüler ===");
            result.AppendLine(DriverLogger.GetAllSystemDrivers());

            result.AppendLine();
            result.AppendLine("=== Tüm INF Dosyaları ===");
            result.AppendLine(DriverLogger.GetAllInfFiles());

            try
            {
                // Save driver snapshot
                File.WriteAllText(driverOutputPath, result.ToString());

                // Log all services and their names
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
                DialogResult dialogResult = MessageBox.Show(
                    $"Snapshot başarıyla kaydedildi:\n{driverOutputPath}\n{serviceNamesPath}\n{registrySnapshotPath}",
                    "Tamamlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // OK tuşuna basıldıysa Form2'yi kapat ve Form3'ü aç
                if (dialogResult == DialogResult.OK)
                {
                    Form3 form3 = new Form3(); // Yeni Form3 nesnesi oluştur
                    form3.Show(); // Form3'ü göster
                    this.Close(); // Form2'yi kapat
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dosya kaydedilirken hata oluştu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Text değiştiğinde yapılacaklar
        }
    }
}