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

        public SettingsDialog()
        {
            InitializeComponent();
            
            rcloneService = new RcloneService();
            LoadConfiguration();
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
                        
                        rcloneService.RclonePath = configuration.RclonePath;
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
                Title = "选择Rclone可执行文件"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                RclonePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private async void TestRcloneButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestRcloneButton.IsEnabled = false;
                TestRcloneButton.Content = "测试中...";
                
                rcloneService.RclonePath = RclonePathTextBox.Text;
                bool isAvailable = await rcloneService.CheckRcloneAsync();
                
                if (isAvailable)
                {
                    MessageBox.Show("Rclone测试成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
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
                configuration.RclonePath = RclonePathTextBox.Text;
                configuration.MinimizeToTray = MinimizeToTrayCheckBox.IsChecked ?? true;
                configuration.AutoStartWithWindows = AutoStartCheckBox.IsChecked ?? false;
                configuration.CheckRcloneOnStartup = CheckRcloneCheckBox.IsChecked ?? true;
                
                // 保存配置
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(configuration, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText("config.json", json);
                
                // 更新注册表（开机自启动）
                UpdateStartupRegistry();
                
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