using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace RcloneMounter
{
    public partial class SettingsDialog : Window
    {
        private RcloneService rcloneService;
        private Configuration configuration;
        private bool rclonePathChanged = false;

        public SettingsDialog()
        {
            InitializeComponent();
            
            rcloneService = new RcloneService();
            LoadConfiguration();
            
            // 尝试自动查找rclone路径
            if (string.IsNullOrEmpty(RclonePathTextBox.Text))
            {
                TryAutoDetectRclone();
            }
        }

        private void TryAutoDetectRclone()
        {
            try
            {
                // 尝试在常见位置查找rclone.exe
                string[] commonPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "rclone", "rclone.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "rclone", "rclone.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "rclone", "rclone.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "rclone", "rclone.exe"),
                    "C:\\rclone\\rclone.exe"
                };

                foreach (string path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        RclonePathTextBox.Text = path;
                        rclonePathChanged = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // 忽略自动检测中的错误
                Console.WriteLine($"自动检测rclone时出错: {ex.Message}");
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists("config.json"))
                {
                    var json = File.ReadAllText("config.json");
                    configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(json);
                    
                    if (configuration != null)
                    {
                        RclonePathTextBox.Text = configuration.RclonePath;
                        MinimizeToTrayCheckBox.IsChecked = configuration.MinimizeToTray;
                        AutoStartCheckBox.IsChecked = configuration.AutoStartWithWindows;
                        CheckRcloneCheckBox.IsChecked = configuration.CheckRcloneOnStartup;
                        
                        if (!string.IsNullOrEmpty(configuration.RclonePath))
                        {
                            rcloneService.RclonePath = configuration.RclonePath;
                        }
                    }
                    else
                    {
                        configuration = new Configuration();
                    }
                }
                else
                {
                    configuration = new Configuration();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置文件时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                configuration = new Configuration();
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*",
                Title = "选择Rclone可执行文件",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                RclonePathTextBox.Text = openFileDialog.FileName;
                rclonePathChanged = true;
            }
        }

        private async void TestRcloneButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RclonePathTextBox.Text))
            {
                MessageBox.Show("请先选择Rclone可执行文件路径", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                TestRcloneButton.IsEnabled = false;
                TestRcloneButton.Content = "测试中...";
                
                rcloneService.RclonePath = RclonePathTextBox.Text;
                bool isAvailable = await rcloneService.CheckRcloneAsync();
                
                if (isAvailable)
                {
                    MessageBox.Show("Rclone测试成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    rclonePathChanged = true;
                }
                else
                {
                    MessageBox.Show("无法连接到Rclone。请检查路径是否正确。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"测试Rclone时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                TestRcloneButton.IsEnabled = true;
                TestRcloneButton.Content = "测试Rclone连接";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 更新配置
                if (configuration == null)
                {
                    configuration = new Configuration();
                }
                
                configuration.RclonePath = RclonePathTextBox.Text;
                configuration.MinimizeToTray = MinimizeToTrayCheckBox.IsChecked ?? true;
                configuration.AutoStartWithWindows = AutoStartCheckBox.IsChecked ?? false;
                configuration.CheckRcloneOnStartup = CheckRcloneCheckBox.IsChecked ?? true;
                
                // 保存配置
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(configuration, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText("config.json", json);
                
                // 更新注册表（开机自启动）
                UpdateStartupRegistry();
                
                // 更新rclone服务的路径
                if (rclonePathChanged && !string.IsNullOrEmpty(RclonePathTextBox.Text))
                {
                    rcloneService.RclonePath = RclonePathTextBox.Text;
                }
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStartupRegistry()
        {
            try
            {
                string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        if (configuration.AutoStartWithWindows)
                        {
                            key.SetValue("RcloneMounter", appPath);
                        }
                        else
                        {
                            if (key.GetValue("RcloneMounter") != null)
                            {
                                key.DeleteValue("RcloneMounter");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新启动项时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 