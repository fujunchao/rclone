using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;

namespace RcloneMounter
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<MountPoint> mountPoints;
        private RcloneService rcloneService;
        private MountPoint currentMountPoint;
        private bool isEditing = false;

        public MainWindow()
        {
            InitializeComponent();
            
            mountPoints = new ObservableCollection<MountPoint>();
            MountPointListView.ItemsSource = mountPoints;
            
            rcloneService = new RcloneService();
            
            LoadConfiguration();
            
            EnableDetailPanel(false);
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists("config.json"))
                {
                    var json = File.ReadAllText("config.json");
                    var config = JsonConvert.DeserializeObject<Configuration>(json);
                    
                    if (config != null && config.MountPoints != null)
                    {
                        foreach (var mountPoint in config.MountPoints)
                        {
                            mountPoints.Add(mountPoint);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置文件时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                var config = new Configuration
                {
                    MountPoints = new System.Collections.Generic.List<MountPoint>(mountPoints)
                };
                
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText("config.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置文件时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EnableDetailPanel(bool enable)
        {
            DetailPanel.IsEnabled = enable;
        }

        private void ClearDetailPanel()
        {
            NameTextBox.Text = string.Empty;
            TypeComboBox.SelectedIndex = -1;
            RcloneConfigTextBox.Text = string.Empty;
            MountPathTextBox.Text = string.Empty;
            OptionsTextBox.Text = string.Empty;
            AutoMountCheckBox.IsChecked = false;
        }

        private void FillDetailPanel(MountPoint mountPoint)
        {
            if (mountPoint == null) return;
            
            NameTextBox.Text = mountPoint.Name;
            TypeComboBox.SelectedIndex = mountPoint.Type == MountType.LocalDisk ? 0 : 1;
            RcloneConfigTextBox.Text = mountPoint.RcloneConfig;
            MountPathTextBox.Text = mountPoint.MountPath;
            OptionsTextBox.Text = mountPoint.Options;
            AutoMountCheckBox.IsChecked = mountPoint.AutoMount;
        }

        private MountPoint GetMountPointFromForm()
        {
            return new MountPoint
            {
                Name = NameTextBox.Text,
                Type = TypeComboBox.SelectedIndex == 0 ? MountType.LocalDisk : MountType.WebDAV,
                RcloneConfig = RcloneConfigTextBox.Text,
                MountPath = MountPathTextBox.Text,
                Options = OptionsTextBox.Text,
                AutoMount = AutoMountCheckBox.IsChecked ?? false
            };
        }

        private bool ValidateMountPoint(MountPoint mountPoint)
        {
            if (string.IsNullOrWhiteSpace(mountPoint.Name))
            {
                MessageBox.Show("请输入挂载名称", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(mountPoint.RcloneConfig))
            {
                MessageBox.Show("请输入Rclone配置名称", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(mountPoint.MountPath))
            {
                MessageBox.Show("请输入挂载路径", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            return true;
        }

        private void MountPointListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MountPointListView.SelectedItem is MountPoint selectedMountPoint)
            {
                isEditing = true;
                currentMountPoint = selectedMountPoint;
                
                EnableDetailPanel(true);
                FillDetailPanel(selectedMountPoint);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            isEditing = false;
            currentMountPoint = null;
            
            MountPointListView.SelectedItem = null;
            ClearDetailPanel();
            EnableDetailPanel(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var mountPoint = GetMountPointFromForm();
            
            if (!ValidateMountPoint(mountPoint))
            {
                return;
            }
            
            if (isEditing && currentMountPoint != null)
            {
                // 更新现有挂载点
                int index = mountPoints.IndexOf(currentMountPoint);
                mountPoints[index] = mountPoint;
            }
            else
            {
                // 添加新挂载点
                mountPoints.Add(mountPoint);
            }
            
            SaveConfiguration();
            
            EnableDetailPanel(false);
            ClearDetailPanel();
            MountPointListView.SelectedItem = null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            EnableDetailPanel(false);
            ClearDetailPanel();
            MountPointListView.SelectedItem = null;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (MountPointListView.SelectedItem is MountPoint selectedMountPoint)
            {
                try
                {
                    StartButton.IsEnabled = false;
                    
                    bool success = await rcloneService.MountAsync(selectedMountPoint);
                    
                    if (success)
                    {
                        MessageBox.Show($"成功挂载 {selectedMountPoint.Name}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"挂载 {selectedMountPoint.Name} 失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"启动挂载时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    StartButton.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("请先选择一个挂载点", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (MountPointListView.SelectedItem is MountPoint selectedMountPoint)
            {
                try
                {
                    StopButton.IsEnabled = false;
                    
                    bool success = await rcloneService.UnmountAsync(selectedMountPoint);
                    
                    if (success)
                    {
                        MessageBox.Show($"成功卸载 {selectedMountPoint.Name}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"卸载 {selectedMountPoint.Name} 失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"卸载挂载时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    StopButton.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("请先选择一个挂载点", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            // 显示更多选项的对话框
            var dialog = new OptionsDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
            
            if (dialog.DialogResult == true && !string.IsNullOrEmpty(dialog.Options))
            {
                OptionsTextBox.Text = dialog.Options;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开设置对话框
            var dialog = new SettingsDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            // 显示关于对话框
            MessageBox.Show("Rclone网盘挂载器\n版本：1.0\n\n一个使用rclone挂载远程网盘到本地的图形界面工具。",
                "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
} 