using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

public class GitStatusViewerWindow : EditorWindow
{
    private const string GIT_REPO_PATH_KEY = "GitStatusViewer_GitRepoPath";
    private const string SKIP_NO_EXTENSION_KEY = "GitStatusViewer_SkipNoExtension";
    private const string SKIP_CONFIG_KEY = "GitStatusViewer_SkipConfig";
    
    private string gitRepoPath = "";
    private Vector2 scrollPosition;
    private List<string> modifiedFiles = new List<string>();
    private bool isProcessing = false;
    private bool skipNoExtension = true;
    private bool skipConfig = true;

    private List<string> skippingFiles = new List<string>{
        "version.json",
        "sandboxData.json",
        "bg.json",
        "MaterialService.json",
        "Localization.json"
    };

    [MenuItem("工具/Git 状态查看器")]
    public static void ShowWindow()
    {
        GetWindow<GitStatusViewerWindow>("Git 状态查看器");
    }

    private void OnEnable()
    {
        // 从EditorPrefs加载保存的路径和设置
        gitRepoPath = EditorPrefs.GetString(GIT_REPO_PATH_KEY, "");
        skipNoExtension = EditorPrefs.GetBool(SKIP_NO_EXTENSION_KEY, true);
        skipConfig = EditorPrefs.GetBool(SKIP_CONFIG_KEY, true);
    }

    private void OnGUI()
    {
        GUILayout.Label("Git 仓库状态查看器", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // Repository path input
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        gitRepoPath = EditorGUILayout.TextField("Git 仓库路径", gitRepoPath);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(GIT_REPO_PATH_KEY, gitRepoPath);
        }
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("选择 Git 仓库", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                gitRepoPath = path;
                EditorPrefs.SetString(GIT_REPO_PATH_KEY, gitRepoPath);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Skip no extension toggle
        EditorGUI.BeginChangeCheck();
        skipNoExtension = EditorGUILayout.Toggle("跳过CloudMap", skipNoExtension);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool(SKIP_NO_EXTENSION_KEY, skipNoExtension);
        }

        // Skip Config toggle
        EditorGUI.BeginChangeCheck();
        skipConfig = EditorGUILayout.Toggle("跳过Config文件", skipConfig);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool(SKIP_CONFIG_KEY, skipConfig);
        }

        EditorGUILayout.Space();

        // Check status button
        GUI.enabled = !string.IsNullOrEmpty(gitRepoPath) && !isProcessing;
        if (GUILayout.Button("检查更新文件"))
        {
            CheckGitStatus();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        // Display modified files
        if (modifiedFiles.Count > 0)
        {
            EditorGUILayout.LabelField("已修改的文件:", EditorStyles.boldLabel);
            
            // 统计修改和删除的文件数量
            int modifiedCount = 0;
            int deletedCount = 0;
            foreach (string file in modifiedFiles.ToList())
            {
                string fullPath = Path.Combine(gitRepoPath, file);
                if (!File.Exists(fullPath))
                {
                    deletedCount++;
                }
                else
                {
                    modifiedCount++;
                }
            }
            
            // 显示统计信息
            EditorGUILayout.LabelField($"总计: {modifiedFiles.Count} 个文件 (修改: {modifiedCount} 个, 删除: {deletedCount} 个)", EditorStyles.miniLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (string file in modifiedFiles.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                string fullPath = Path.Combine(gitRepoPath, file);
                string status = !File.Exists(fullPath) ? "[已删除] " : "";
                EditorGUILayout.LabelField(status + file);
                if (GUILayout.Button("打开", GUILayout.Width(60)))
                {
                    if (File.Exists(fullPath))
                    {
                        EditorUtility.OpenWithDefaultApp(fullPath);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// 检查文件路径是否应该被跳过
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>如果应该跳过返回true</returns>
    private bool ShouldSkipFile(string filePath)
    {
        // 跳过 .git 目录下的所有文件
        if (filePath.StartsWith(".git/") || filePath.StartsWith(".git\\"))
        {
            Debug.Log($"跳过.git目录下的文件: {filePath}");
            return true;
        }

        // 检查文件名是否在跳过列表中
        string fileName = Path.GetFileName(filePath);
        if (skippingFiles.Contains(fileName))
        {
            return true;
        }

        // 跳过Config文件
        if (skipConfig && fileName.EndsWith("Config.lua"))
        {
            return true;
        }

        // 跳过CloudMap相关文件
        if (skipNoExtension && filePath.Contains("sandbox/assets"))
        {
            return true;
        }

        return false;
    }

    private string NormalizeLineEndings(string text)
    {
        return Regex.Replace(text, @"\r\n|\r|\n", "\n");
    }

    private string ProcessFloatNumbers(string content)
    {
        // 匹配浮点数的正则表达式，包括科学计数法
        string pattern = @"-?\d+\.\d+(?:[eE][-+]?\d+)?";
        var matches = Regex.Matches(content, pattern);

        string result = Regex.Replace(content, pattern, match =>
        {
            if (float.TryParse(match.Value, out float number))
            {
                // 向下取整到小数点后1位
                number = Mathf.Floor(number * 10) / 10;
                // 转换为字符串并去除结尾的0
                return number.ToString("F1").TrimEnd('0').TrimEnd('.');
            }
            return match.Value;
        });

        return result;
    }

    private string DecodeUtf8Escape(string input)
    {
        // 将八进制转义序列转换为字节数组
        var bytes = new List<byte>();
        var matches = Regex.Matches(input, @"\\(\d{3})");
        
        foreach (Match match in matches)
        {
            bytes.Add(Convert.ToByte(match.Groups[1].Value, 8));
        }
        
        // 将字节数组转换为UTF-8字符串
        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    private string DecodeGitPath(string input)
    {
        // 分割路径
        var parts = input.Split('/');
        var decodedParts = new List<string>();
        
        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Contains("\\"))
            {
                // 如果部分包含转义序列，解码它
                // 检查是否是最后一个部分（文件名）
                if (i == parts.Length - 1)
                {
                    // 分离文件名和扩展名
                    var lastDotIndex = part.LastIndexOf('.');
                    if (lastDotIndex != -1)
                    {
                        var name = part.Substring(0, lastDotIndex);
                        var extension = part.Substring(lastDotIndex);
                        // 保存下划线，然后解码其他部分
                        var nameParts = name.Split('_');
                        for (int j = 0; j < nameParts.Length; j++)
                        {
                            if (nameParts[j].Contains("\\"))
                            {
                                nameParts[j] = DecodeUtf8Escape(nameParts[j]);
                            }
                        }
                        decodedParts.Add(string.Join("_", nameParts) + extension);
                    }
                    else
                    {
                        // 保存下划线，然后解码其他部分
                        var nameParts = part.Split('_');
                        for (int j = 0; j < nameParts.Length; j++)
                        {
                            if (nameParts[j].Contains("\\"))
                            {
                                nameParts[j] = DecodeUtf8Escape(nameParts[j]);
                            }
                        }
                        decodedParts.Add(string.Join("_", nameParts));
                    }
                }
                else
                {
                    // 保存下划线，然后解码其他部分
                    var nameParts = part.Split('_');
                    for (int j = 0; j < nameParts.Length; j++)
                    {
                        if (nameParts[j].Contains("\\"))
                        {
                            nameParts[j] = DecodeUtf8Escape(nameParts[j]);
                        }
                    }
                    decodedParts.Add(string.Join("_", nameParts));
                }
            }
            else
            {
                // 否则保持原样
                decodedParts.Add(part);
            }
        }
        
        // 重新组合路径
        return string.Join("/", decodedParts);
    }

    private async void CheckGitStatus()
    {
        if (string.IsNullOrEmpty(gitRepoPath) || !Directory.Exists(gitRepoPath))
        {
            EditorUtility.DisplayDialog("错误", "请选择有效的 Git 仓库路径", "确定");
            return;
        }

        isProcessing = true;
        modifiedFiles.Clear();

        try
        {
            EditorUtility.DisplayProgressBar("检查文件", "正在获取Git状态...", 0.1f);

            // 1. 一次性获取所有Git状态
            string output = await Task.Run(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "status --porcelain",
                    WorkingDirectory = gitRepoPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using (var process = Process.Start(startInfo))
                {
                    string result = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return result;
                }
            });

            // 2. 解析和过滤文件列表
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var filesToCheck = new List<(string status, string path)>();
            foreach (var line in lines)
            {
                if (line.Length <= 3) continue;

                string status = line.Substring(0, 2).Trim();
                // 原始路径可能包含UTF-8转义和引号，先解码再清理
                string rawPath = line.Substring(3);
                string decodedPath = DecodeGitPath(rawPath);
                string cleanFilePath = decodedPath.Trim('"');

                if (ShouldSkipFile(cleanFilePath))
                {
                    Debug.Log($"跳过文件: {cleanFilePath}");
                    continue;
                }
                filesToCheck.Add((status, cleanFilePath));
            }

            var trulyModifiedFiles = new System.Collections.Concurrent.ConcurrentBag<string>();
            var filesToRevert = new System.Collections.Concurrent.ConcurrentBag<string>();
            
            EditorUtility.DisplayProgressBar("检查文件", $"准备并发检查 {filesToCheck.Count} 个文件...", 0.3f);

            // 3. 并行检查文件差异
            await Task.Run(() =>
            {
                var po = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2) };
                Parallel.ForEach(filesToCheck, po, fileInfo =>
                {
                    var (status, filePath) = fileInfo;
                    var fullPath = Path.Combine(gitRepoPath, filePath);

                    // 已删除的文件直接标记为修改
                    if (status == "D" || !File.Exists(fullPath))
                    {
                        trulyModifiedFiles.Add(filePath);
                        return; // continue
                    }
                    
                    // Lua文件不处理浮点数，直接视为修改
                    if (Path.GetExtension(filePath).ToLower() == ".lua")
                    {
                        trulyModifiedFiles.Add(filePath);
                        return; // continue
                    }

                    // 在内存中进行差异比较
                    try
                    {
                        // 使用 git show 获取暂存区或HEAD的文件原始内容
                        string originalContent;
                        var showInfo = new ProcessStartInfo
                        {
                            FileName = "git",
                            // 使用 :"" 来处理带空格和特殊字符的路径
                            Arguments = $"show :\"{filePath.Replace("\"", "\\\"")}\"",
                            WorkingDirectory = gitRepoPath,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.UTF8
                        };
                        using (var p = Process.Start(showInfo))
                        {
                            originalContent = p.StandardOutput.ReadToEnd();
                            p.WaitForExit();
                        }
                        
                        string currentContent = File.ReadAllText(fullPath);
                        string processedContent = ProcessFloatNumbers(currentContent);
                        
                        bool areEqual;
                        // 只对json文件进行换行符归一化处理
                        if (Path.GetExtension(filePath).ToLower() == ".json")
                        {
                            areEqual = NormalizeLineEndings(originalContent) == NormalizeLineEndings(processedContent);
                        }
                        else
                        {
                            // 对于其他文件，直接比较，但仍然使用处理了浮点数的当前内容
                            areEqual = originalContent == processedContent;
                        }
                        
                        if (areEqual)
                        {
                            filesToRevert.Add(filePath);
                        }
                        else
                        {
                            trulyModifiedFiles.Add(filePath);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"处理文件 {filePath} 差异失败，将标记为已修改: {e.Message}");
                        trulyModifiedFiles.Add(filePath);
                    }
                });
            });

            // 4. 批量恢复仅有浮点数差异的文件
            if (!filesToRevert.IsEmpty)
            {
                EditorUtility.DisplayProgressBar("检查文件", $"正在批量恢复 {filesToRevert.Count} 个文件...", 0.9f);
                var files = filesToRevert.ToList();
                int batchSize = 50; // 避免命令行过长
                for (int i = 0; i < files.Count; i += batchSize)
                {
                    var batch = files.Skip(i).Take(batchSize).Select(f => $"\"{f.Replace("\"", "\\\"")}\"");
                    var checkoutInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"checkout -- {string.Join(" ", batch)}",
                        WorkingDirectory = gitRepoPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    using (var p = Process.Start(checkoutInfo))
                    {
                        p.WaitForExit();
                    }
                }
            }
            
            modifiedFiles.AddRange(trulyModifiedFiles.OrderBy(f => f));
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"执行 git 命令失败: {e.Message}", "确定");
            Debug.LogError(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            isProcessing = false;
            Repaint();
        }
    }
}