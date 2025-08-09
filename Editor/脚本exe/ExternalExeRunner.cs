using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class ExternalExeRunner
{
    [MenuItem("工具/脚本exe/运行 Git 状态检查器")]
    public static async void RunGitStatusChecker()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string assetsEditorDir = Path.Combine(Application.dataPath, "Editor", "脚本exe");
        string rootEditorDir = Path.Combine(projectRoot, "Editor", "脚本exe");

        string exePath = Path.Combine(assetsEditorDir, "git_status_checker.exe");
        string exeDirectory = assetsEditorDir;
        if (!File.Exists(exePath))
        {
            exePath = Path.Combine(rootEditorDir, "git_status_checker.exe");
            exeDirectory = rootEditorDir;
        }

        if (!File.Exists(exePath))
        {
            EditorUtility.DisplayDialog(
                "未找到可执行文件",
                $"未在以下路径找到 git_status_checker.exe:\n1) {Path.Combine(assetsEditorDir, "git_status_checker.exe")}\n2) {Path.Combine(rootEditorDir, "git_status_checker.exe")}",
                "确定");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("执行外部程序", "正在运行 git_status_checker.exe...", 0.3f);

            var result = await Task.Run(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = exeDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                // 解决 Python 程序在中文 Windows 控制台下默认使用 GBK 编码导致的表情/Unicode 输出异常
                // 强制 Python 使用 UTF-8，避免 '\U0001f680'（🚀）等字符输出时报 gbk 编码错误
                try
                {
                    startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
                    startInfo.EnvironmentVariables["PYTHONUTF8"] = "1"; // Python 3.7+
                    // 可选：某些程序会读取 LANG
                    startInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";
                }
                catch {}

                using (var process = Process.Start(startInfo))
                {
                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return (exitCode: process.ExitCode, output: stdout, error: stderr);
                }
            });

            if (!string.IsNullOrEmpty(result.output))
            {
                Debug.Log($"[git_status_checker] 输出:\n{result.output}");
            }

            if (!string.IsNullOrEmpty(result.error))
            {
                Debug.LogWarning($"[git_status_checker] 错误:\n{result.error}");
            }

            // 当因控制台代码页或编码导致 Python 报 gbk/codec 错误时，使用 cmd 切换到 UTF-8 代码页后重试
            if (result.exitCode != 0 && !string.IsNullOrEmpty(result.error) &&
                (result.error.IndexOf("gbk", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                 result.error.IndexOf("codec", System.StringComparison.OrdinalIgnoreCase) >= 0))
            {
                Debug.Log("[git_status_checker] 检测到编码相关错误，使用 UTF-8 代码页重试...");

                var retry = await Task.Run(() =>
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c chcp 65001>nul & \"{exePath}\"",
                        WorkingDirectory = exeDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };
                    try
                    {
                        startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
                        startInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
                        startInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";
                    }
                    catch {}

                    using (var process = Process.Start(startInfo))
                    {
                        string stdout = process.StandardOutput.ReadToEnd();
                        string stderr = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        return (exitCode: process.ExitCode, output: stdout, error: stderr);
                    }
                });

                if (!string.IsNullOrEmpty(retry.output))
                {
                    Debug.Log($"[git_status_checker][Retry UTF-8] 输出:\n{retry.output}");
                }
                if (!string.IsNullOrEmpty(retry.error))
                {
                    Debug.LogWarning($"[git_status_checker][Retry UTF-8] 错误:\n{retry.error}");
                }

                result = retry;
            }

            if (result.exitCode == 0)
            {
                EditorUtility.DisplayDialog("执行完成", "git_status_checker.exe 执行成功", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("执行失败", $"退出码: {result.exitCode}\n详情查看 Console 日志", "确定");
            }
        }
        catch (System.SystemException e)
        {
            Debug.LogError($"运行 git_status_checker.exe 失败: {e.Message}\n{e}");
            EditorUtility.DisplayDialog("执行异常", e.Message, "确定");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}


