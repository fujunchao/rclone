using System;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Threading.Tasks;

namespace RcloneMounter
{
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private Configuration configuration;
        private RcloneService rcloneService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 设置未捕获异常处理
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            // 初始化系统托盘图标
            InitializeNotifyIcon();
            
            // 加载配置
            LoadConfiguration();
            
            // 初始化Rclone服务
            rcloneService = new RcloneService();
            
            // 如果配置中设置了检查Rclone
            if (configuration != null && configuration.CheckRcloneOnStartup)
            {
                CheckRcloneAsync();
            }
            
            // 自动挂载配置的挂载点
            AutoMountConfiguredPoints();
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists("config.json"))
                {
                    var json = File.ReadAllText("config.json");
                    configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(json);
                    
                    if (configuration != null && !string.IsNullOrEmpty(configuration.RclonePath))
                    {
                        rcloneService.RclonePath = configuration.RclonePath;
                    }
                }
                else
                {
                    configuration = new Configuration();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                configuration = new Configuration();
            }
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                // 临时使用系统默认图标
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Rclone挂载器"
            };
            
            notifyIcon.DoubleClick += (s, e) => 
            {
                if (MainWindow != null)
                {
                    MainWindow.WindowState = WindowState.Normal;
                    MainWindow.Activate();
                }
            };
            
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            
            var openMenuItem = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "打开主窗口"
            };
            openMenuItem.Click += (s, e) => 
            {
                if (MainWindow != null)
                {
                    MainWindow.WindowState = WindowState.Normal;
                    MainWindow.Activate();
                }
            };
            
            var exitMenuItem = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "退出"
            };
            exitMenuItem.Click += (s, e) => Shutdown();
            
            contextMenu.Items.Add(openMenuItem);
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);
            
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            MessageBox.Show($"发生未处理的异常：{exception?.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"发生未处理的UI异常：{e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 清理系统托盘图标
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
            
            base.OnExit(e);
        }

        private async Task CheckRcloneAsync()
        {
            try
            {
                bool isAvailable = await rcloneService.CheckRcloneAsync();
                
                if (!isAvailable)
                {
                    MessageBox.Show("无法找到或运行Rclone。请在设置中检查Rclone可执行文件路径。", 
                        "Rclone未找到", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"检查Rclone时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AutoMountConfiguredPoints()
        {
            if (configuration == null || configuration.MountPoints == null)
                return;
            
            foreach (var mountPoint in configuration.MountPoints)
            {
                if (mountPoint.AutoMount)
                {
                    try
                    {
                        await rcloneService.MountAsync(mountPoint);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"自动挂载 {mountPoint.Name} 时出错：{ex.Message}", 
                            "自动挂载错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
} 