using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RcloneMounter
{
    public enum MountType
    {
        LocalDisk,
        WebDAV
    }
    
    public class MountPoint : INotifyPropertyChanged
    {
        private string name;
        private MountType type;
        private string rcloneConfig;
        private string mountPath;
        private string options;
        private bool autoMount;
        private bool isMounted;

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        public MountType Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RcloneConfig
        {
            get => rcloneConfig;
            set
            {
                if (rcloneConfig != value)
                {
                    rcloneConfig = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MountPath
        {
            get => mountPath;
            set
            {
                if (mountPath != value)
                {
                    mountPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Options
        {
            get => options;
            set
            {
                if (options != value)
                {
                    options = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoMount
        {
            get => autoMount;
            set
            {
                if (autoMount != value)
                {
                    autoMount = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMounted
        {
            get => isMounted;
            set
            {
                if (isMounted != value)
                {
                    isMounted = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 