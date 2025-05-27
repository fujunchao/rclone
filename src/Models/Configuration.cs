using System.Collections.Generic;

namespace RcloneMounter
{
    public class Configuration
    {
        public string RclonePath { get; set; } = "rclone.exe";
        public List<MountPoint> MountPoints { get; set; } = new List<MountPoint>();
        public bool MinimizeToTray { get; set; } = true;
        public bool AutoStartWithWindows { get; set; } = false;
        public bool CheckRcloneOnStartup { get; set; } = true;
    }
} 