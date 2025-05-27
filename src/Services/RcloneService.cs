using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RcloneMounter
{
    public class RcloneService
    {
        private readonly Dictionary<string, Process> runningProcesses = new Dictionary<string, Process>();
        
        public string RclonePath { get; set; } = "rclone.exe";
        
        public async Task<bool> MountAsync(MountPoint mountPoint)
        {
            if (mountPoint == null)
            {
                throw new ArgumentNullException(nameof(mountPoint));
            }
            
            if (runningProcesses.ContainsKey(mountPoint.Name))
            {
                throw new InvalidOperationException($"挂载点 '{mountPoint.Name}' 已经在运行中");
            }
            
            try
            {
                // 创建挂载目录（如果不存在）
                if (!Directory.Exists(mountPoint.MountPath))
                {
                    Directory.CreateDirectory(mountPoint.MountPath);
                }
                
                var process = new Process();
                process.StartInfo.FileName = RclonePath;
                
                if (mountPoint.Type == MountType.LocalDisk)
                {
                    // 本地磁盘模拟
                    process.StartInfo.Arguments = $"mount {mountPoint.RcloneConfig}: {mountPoint.MountPath} {mountPoint.Options} --vfs-cache-mode full";
                }
                else
                {
                    // WebDAV 服务
                    process.StartInfo.Arguments = $"serve webdav {mountPoint.RcloneConfig}: --addr 127.0.0.1:8080 {mountPoint.Options}";
                }
                
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                
                // 异步处理输出，避免阻塞
                process.OutputDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.WriteLine($"Rclone Output: {e.Data}");
                    }
                };
                
                process.ErrorDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.WriteLine($"Rclone Error: {e.Data}");
                    }
                };
                
                bool started = process.Start();
                if (started)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    
                    runningProcesses[mountPoint.Name] = process;
                    mountPoint.IsMounted = true;
                    
                    // 等待确保进程已经正常启动
                    await Task.Delay(1000);
                    
                    // 检查进程是否仍在运行
                    if (process.HasExited)
                    {
                        return false;
                    }
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"挂载时出错: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> UnmountAsync(MountPoint mountPoint)
        {
            if (mountPoint == null)
            {
                throw new ArgumentNullException(nameof(mountPoint));
            }
            
            if (!runningProcesses.TryGetValue(mountPoint.Name, out var process))
            {
                throw new InvalidOperationException($"挂载点 '{mountPoint.Name}' 没有在运行");
            }
            
            try
            {
                if (!process.HasExited)
                {
                    // 首先尝试正常关闭
                    process.CloseMainWindow();
                    
                    // 等待进程响应关闭命令
                    await Task.Delay(2000);
                    
                    // 如果进程仍未退出，强制终止
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                
                // 如果是本地磁盘模拟，还需要执行rclone的unmount命令
                if (mountPoint.Type == MountType.LocalDisk)
                {
                    // 在Windows上卸载驱动器的命令
                    var unmountProcess = new Process();
                    unmountProcess.StartInfo.FileName = RclonePath;
                    unmountProcess.StartInfo.Arguments = $"unmount {mountPoint.MountPath}";
                    unmountProcess.StartInfo.UseShellExecute = false;
                    unmountProcess.StartInfo.CreateNoWindow = true;
                    
                    unmountProcess.Start();
                    await Task.Delay(1000); // 给unmount命令一些时间执行
                }
                
                runningProcesses.Remove(mountPoint.Name);
                mountPoint.IsMounted = false;
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"卸载时出错: {ex.Message}");
                return false;
            }
        }
        
        public bool IsMounted(MountPoint mountPoint)
        {
            if (mountPoint == null)
            {
                return false;
            }
            
            return runningProcesses.ContainsKey(mountPoint.Name) && 
                   !runningProcesses[mountPoint.Name].HasExited;
        }
        
        public async Task<bool> CheckRcloneAsync()
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = RclonePath;
                process.StartInfo.Arguments = "version";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                return output.Contains("rclone");
            }
            catch
            {
                return false;
            }
        }
    }
} 