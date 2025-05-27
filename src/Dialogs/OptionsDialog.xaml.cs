using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RcloneMounter
{
    public partial class OptionsDialog : Window
    {
        public string Options { get; private set; }

        public OptionsDialog()
        {
            InitializeComponent();
        }
        
        public OptionsDialog(string existingOptions) : this()
        {
            ParseExistingOptions(existingOptions);
        }

        private void ParseExistingOptions(string existingOptions)
        {
            if (string.IsNullOrEmpty(existingOptions))
                return;
            
            // 分割选项字符串为独立的选项
            string[] options = existingOptions.Split(' ');
            
            foreach (var option in options)
            {
                // 根据选项设置相应的复选框
                switch (option.Trim())
                {
                    case "--vfs-cache-mode full":
                        VfsCacheModeCheckBox.IsChecked = true;
                        break;
                    case "--vfs-read-ahead 128M":
                        VfsReadAheadCheckBox.IsChecked = true;
                        break;
                    case "--vfs-read-chunk-size 64M":
                        VfsReadChunkSizeCheckBox.IsChecked = true;
                        break;
                    case "--vfs-cache-max-age 1h":
                        VfsCacheMaxAgeCheckBox.IsChecked = true;
                        break;
                    case "--vfs-cache-max-size 10G":
                        VfsCacheMaxSizeCheckBox.IsChecked = true;
                        break;
                    case "--allow-other":
                        AllowOtherCheckBox.IsChecked = true;
                        break;
                    case "--allow-non-empty":
                        AllowNonEmptyCheckBox.IsChecked = true;
                        break;
                    case "--dir-cache-time 30m":
                        DirCacheTimeCheckBox.IsChecked = true;
                        break;
                    case "--attr-timeout 30s":
                        AttrTimeoutCheckBox.IsChecked = true;
                        break;
                    default:
                        // 如果是WebDAV端口选项
                        if (option.StartsWith("--addr"))
                        {
                            WebdavAddrCheckBox.IsChecked = true;
                            // 尝试解析端口
                            string[] parts = option.Split(':');
                            if (parts.Length > 1)
                            {
                                WebdavPortTextBox.Text = parts[parts.Length - 1];
                            }
                        }
                        else if (option == "--baseurl /webdav")
                        {
                            WebdavBaseUrlCheckBox.IsChecked = true;
                        }
                        else if (option.StartsWith("--cert"))
                        {
                            WebdavCertCheckBox.IsChecked = true;
                        }
                        else if (option.StartsWith("--key"))
                        {
                            WebdavKeyCheckBox.IsChecked = true;
                        }
                        else
                        {
                            // 添加到自定义选项中
                            if (!string.IsNullOrEmpty(CustomOptionsTextBox.Text))
                                CustomOptionsTextBox.Text += "\n";
                            CustomOptionsTextBox.Text += option;
                        }
                        break;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 收集所有选中的选项
            var selectedOptions = new List<string>();
            
            // VFS选项
            if (VfsCacheModeCheckBox.IsChecked == true)
                selectedOptions.Add("--vfs-cache-mode full");
            
            if (VfsReadAheadCheckBox.IsChecked == true)
                selectedOptions.Add("--vfs-read-ahead 128M");
            
            if (VfsReadChunkSizeCheckBox.IsChecked == true)
                selectedOptions.Add("--vfs-read-chunk-size 64M");
            
            if (VfsCacheMaxAgeCheckBox.IsChecked == true)
                selectedOptions.Add("--vfs-cache-max-age 1h");
            
            if (VfsCacheMaxSizeCheckBox.IsChecked == true)
                selectedOptions.Add("--vfs-cache-max-size 10G");
            
            // 挂载选项
            if (AllowOtherCheckBox.IsChecked == true)
                selectedOptions.Add("--allow-other");
            
            if (AllowNonEmptyCheckBox.IsChecked == true)
                selectedOptions.Add("--allow-non-empty");
            
            if (DirCacheTimeCheckBox.IsChecked == true)
                selectedOptions.Add("--dir-cache-time 30m");
            
            if (AttrTimeoutCheckBox.IsChecked == true)
                selectedOptions.Add("--attr-timeout 30s");
            
            // WebDAV选项
            if (WebdavAddrCheckBox.IsChecked == true)
            {
                string port = "8080";
                if (!string.IsNullOrEmpty(WebdavPortTextBox.Text))
                    port = WebdavPortTextBox.Text.Trim();
                
                selectedOptions.Add($"--addr 127.0.0.1:{port}");
            }
            
            if (WebdavBaseUrlCheckBox.IsChecked == true)
                selectedOptions.Add("--baseurl /webdav");
            
            if (WebdavCertCheckBox.IsChecked == true)
                selectedOptions.Add("--cert PATH");  // 实际使用时应替换PATH
            
            if (WebdavKeyCheckBox.IsChecked == true)
                selectedOptions.Add("--key PATH");   // 实际使用时应替换PATH
            
            // 添加自定义选项
            if (!string.IsNullOrEmpty(CustomOptionsTextBox.Text))
            {
                string[] customOptions = CustomOptionsTextBox.Text.Split('\n');
                foreach (var option in customOptions)
                {
                    if (!string.IsNullOrWhiteSpace(option))
                        selectedOptions.Add(option.Trim());
                }
            }
            
            // 合并所有选项
            Options = string.Join(" ", selectedOptions);
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 