using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files (*.exe;*.msi)|*.exe;*.msi"; // Sadece exe ve msi dosyalarını göster
                openFileDialog.Title = "Select an Executable or MSI File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;

                    // Uzantıyı kontrol et
                    string fileExtension = Path.GetExtension(selectedFilePath).ToLower();
                    if (fileExtension == ".exe" || fileExtension == ".msi")
                    {
                        // Log dosyasını masaüstüne kaydet
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string logFilePath = Path.Combine(desktopPath, $"Netstat_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                        // Seçilen dosyayı çalıştır
                        Process process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = selectedFilePath,
                                UseShellExecute = true
                            }
                        };

                        process.Start();
                        int parentProcessId = process.Id; // Parent process'in PID'sini al

                        // Netstat çıktısını izlemek için bir görev başlat
                        Task.Run(() =>
                        {
                            List<int> trackedPIDs = new List<int> { parentProcessId }; // İzlenen PID'ler (parent + child)
                            using (StreamWriter logFile = new StreamWriter(logFilePath, append: true))
                            {
                                logFile.WriteLine("=== Netstat Log Started ===");
                                logFile.WriteLine($"Process: {selectedFilePath}");
                                logFile.WriteLine($"Parent PID: {parentProcessId}");
                                logFile.WriteLine($"Timestamp: {DateTime.Now}");
                                logFile.WriteLine("===========================");

                                while (!process.HasExited)
                                {
                                    // Child process'leri güncelle
                                    UpdateChildProcesses(parentProcessId, trackedPIDs);

                                    string netstatOutput = RunNetstat();
                                    var ips = ParseNetstatOutputForPIDs(netstatOutput, trackedPIDs);

                                    // Yerel olmayan IP adreslerini log dosyasına yaz
                                    foreach (var ip in ips)
                                    {
                                        logFile.WriteLine($"{DateTime.Now:HH:mm:ss} - {ip}");
                                    }

                                    logFile.Flush(); // Verileri hemen dosyaya yaz
                                    Task.Delay(2000).Wait(); // 2 saniye bekle
                                }

                                logFile.WriteLine("=== Netstat Log Ended ===");
                            }

                            // İşlem tamamlandığında kullanıcıya bilgi ver
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show($"Netstat log saved to:\n{logFilePath}", "Log Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }));
                        });
                    }
                    else
                    {
                        MessageBox.Show("Only .exe or .msi files are allowed.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        // Netstat komutunu çalıştır ve çıktıyı döndür
        private string RunNetstat()
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-ano", // PID bilgisi için -ano kullanılır
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        // Netstat çıktısını analiz et ve belirli PID'lere ait yerel olmayan IP adreslerini döndür
        private List<string> ParseNetstatOutputForPIDs(string netstatOutput, List<int> targetPIDs)
        {
            List<string> externalIPs = new List<string>();
            string[] lines = netstatOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Contains("ESTABLISHED") || line.Contains("SYN_SENT"))
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5)
                    {
                        string remoteAddress = parts[2];
                        string ip = remoteAddress.Split(':')[0];
                        int pid;

                        // PID'yi al ve hedef PID'ler listesinde olup olmadığını kontrol et
                        if (int.TryParse(parts[4], out pid) && targetPIDs.Contains(pid))
                        {
                            // Yerel IP adreslerini hariç tut
                            if (!IsLocalIPAddress(ip))
                            {
                                externalIPs.Add(ip);
                            }
                        }
                    }
                }
            }

            return externalIPs;
        }

        // Yerel IP adreslerini kontrol et
        private bool IsLocalIPAddress(string ipAddress)
        {
            return ipAddress.StartsWith("127.") || ipAddress.StartsWith("192.168.") || ipAddress.StartsWith("10.") || ipAddress.StartsWith("172.");
        }

        // Parent process'in child process'lerini güncelle
        private void UpdateChildProcesses(int parentPID, List<int> trackedPIDs)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    // Parent PID'si verilen PID ile eşleşen process'leri child olarak ekle
                    if (!trackedPIDs.Contains(process.Id) && GetParentProcessId(process) == parentPID)
                    {
                        trackedPIDs.Add(process.Id);
                    }
                }
                catch
                {
                    // Bazı process'lere erişim izni olmayabilir, bu durumda devam et
                }
            }
        }

        // Bir process'in parent PID'sini al
        private int GetParentProcessId(Process process)
        {
            try
            {
                using (var query = new System.Management.ManagementObject($"win32_process.handle='{process.Id}'"))
                {
                    query.Get();
                    return Convert.ToInt32(query["ParentProcessId"]);
                }
            }
            catch
            {
                return -1; // Erişim hatası durumunda -1 döndür
            }
        }



        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
