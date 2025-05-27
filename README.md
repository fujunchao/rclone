# RcloneMounter

RcloneMounter是一个Windows图形界面应用程序，用于轻松管理和挂载使用rclone的网盘。

## 功能特点

- 美观的Windows图形界面
- 支持本地磁盘挂载模式
- 支持WebDAV服务模式
- 可保存多个挂载配置
- 系统托盘支持
- 开机自启动选项
- 多种挂载参数配置

## 系统要求

- Windows 7/8/10/11
- [Rclone](https://rclone.org/downloads/) 已安装
- .NET 6.0 或更高版本

## 使用方法

1. 下载并安装Rclone
2. 配置好Rclone的远程存储设置
3. 运行本应用程序
4. 点击"添加挂载点"配置新的网盘挂载
5. 选择挂载类型（本地磁盘或WebDAV）
6. 设置Rclone配置名称和挂载路径
7. 点击"启动"开始挂载

## 本地磁盘模式与WebDAV模式

- **本地磁盘模式**：将远程存储挂载为本地磁盘，可以像使用普通硬盘一样使用
- **WebDAV模式**：将远程存储作为WebDAV服务提供，可以被其他支持WebDAV的应用程序使用

## 下载

从[GitHub Releases](https://github.com/fujunchao/rclone/releases)页面下载最新版本。

## 构建项目

```bash
dotnet restore
dotnet build
dotnet publish -c Release -r win-x64 --self-contained true
```

## 注意事项

- 使用前请确保已正确安装Rclone并进行基本配置
- 首次使用时请在设置中指定Rclone可执行文件路径
- 本地磁盘模式在Windows上需要管理员权限才能正常运行

## 截图

(请在使用后添加应用截图)

## 贡献

欢迎提交Pull Request或创建Issue。

## 许可

[MIT](LICENSE) 