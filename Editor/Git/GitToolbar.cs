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

    /// <summary>
    /// 判断文件是否明显有真实修改，无需详细检查
    /// </summary>
    private bool IsObviouslyModified(string filePath, string status)
    {
        // 删除的文件
        if (status == "D") return true;
        
        // 新增的文件
        if (status.Contains("A")) return true;
        
        // Lua文件不处理浮点数，直接视为修改
        if (Path.GetExtension(filePath).ToLower() == ".lua") return true;
        
        // 非json文件的大文件直接视为修改（避免不必要处理）
        var ext = Path.GetExtension(filePath).ToLower();
        if (ext != ".json")
        {
            try
            {
                var fullPath = Path.Combine(gitRepoPath, filePath);
                if (File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    if (fileInfo.Length > 50 * 1024) // 大于50KB
                        return true;
                }
            }
            catch
            {
                return true; // 文件访问异常，直接视为修改
            }
        }
        
        return false;
    }

    /// <summary>
    /// 批量获取文件的Git原始内容
    /// </summary>
    private async Task<Dictionary<string, string>> GetBatchOriginalContentsAsync(List<string> filePaths)
    {
        var contents = new Dictionary<string, string>();
        
        // 分批处理，避免命令行过长
        int batchSize = 100;
        for (int i = 0; i < filePaths.Count; i += batchSize)
        {
            var batch = filePaths.Skip(i).Take(batchSize).ToList();
            var batchContents = await GetBatchOriginalContentsInternalAsync(batch);
            
            foreach (var kvp in batchContents)
            {
                contents[kvp.Key] = kvp.Value;
            }
        }
        
        return contents;
    }

    private async Task<Dictionary<string, string>> GetBatchOriginalContentsInternalAsync(List<string> filePaths)
    {
        var contents = new Dictionary<string, string>();
        
        await Task.Run(() =>
        {
            foreach (var filePath in filePaths)
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"show HEAD:\"{filePath.Replace("\"", "\\\"")}\"",
                        WorkingDirectory = gitRepoPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        
                        if (process.ExitCode == 0)
                        {
                            contents[filePath] = output;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"获取文件 {filePath} 原始内容失败: {e.Message}");
                }
            }
        });
        
        return contents;
    }

    private string NormalizeLineEndings(string text)
    {
        return Regex.Replace(text, @"\r\n|\r|\n", "\n");
    }

    private string ProcessFloatNumbers(string content)
    {
        // 匹配浮点数的正则表达式，包括科学计数法
        string pattern = @"-?\d+\.\d+(?:[eE][-+]?\d+)?";
        
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
            EditorUtility.DisplayProgressBar("检查文件", "正在解析文件列表...", 0.2f);
            
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var allFiles = new List<(string status, string path)>();
            var obviouslyModified = new List<string>();
            var needDetailedCheck = new List<string>();
            
            foreach (var line in lines)
            {
                if (line.Length <= 3) continue;

                string status = line.Substring(0, 2).Trim();
                string rawPath = line.Substring(3);
                string decodedPath = DecodeGitPath(rawPath);
                string cleanFilePath = decodedPath.Trim('"');

                if (ShouldSkipFile(cleanFilePath))
                {
                    continue;
                }
                
                allFiles.Add((status, cleanFilePath));
                
                // 智能分类：明显修改 vs 需要详细检查
                if (IsObviouslyModified(cleanFilePath, status))
                {
                    obviouslyModified.Add(cleanFilePath);
                }
                else
                {
                    needDetailedCheck.Add(cleanFilePath);
                }
            }

            Debug.Log($"总文件: {allFiles.Count}, 明显修改: {obviouslyModified.Count}, 需详细检查: {needDetailedCheck.Count}");

            // 3. 直接添加明显修改的文件
            modifiedFiles.AddRange(obviouslyModified);

            // 4. 只对需要详细检查的文件进行内容比较
            if (needDetailedCheck.Count > 0)
            {
                EditorUtility.DisplayProgressBar("检查文件", $"正在批量获取 {needDetailedCheck.Count} 个文件的原始内容...", 0.4f);
                
                // 批量获取原始内容
                var originalContents = await GetBatchOriginalContentsAsync(needDetailedCheck);
                
                EditorUtility.DisplayProgressBar("检查文件", $"正在并发检查 {needDetailedCheck.Count} 个文件差异...", 0.6f);

                var trulyModifiedFiles = new ConcurrentBag<string>();
                var filesToRevert = new ConcurrentBag<string>();

                // 高效并发处理
                await Task.Run(() =>
                {
                    var po = new ParallelOptions 
                    { 
                        MaxDegreeOfParallelism = Environment.ProcessorCount // 使用全部CPU核心
                    };
                    
                    Parallel.ForEach(needDetailedCheck, po, filePath =>
                    {
                        try
                        {
                            var fullPath = Path.Combine(gitRepoPath, filePath);
                            if (!File.Exists(fullPath))
                            {
                                trulyModifiedFiles.Add(filePath);
                                return;
                            }

                            if (!originalContents.TryGetValue(filePath, out string originalContent))
                            {
                                // 如果获取原始内容失败，标记为修改
                                trulyModifiedFiles.Add(filePath);
                                return;
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
                            Debug.LogWarning($"处理文件 {filePath} 差异检查失败: {e.Message}");
                            trulyModifiedFiles.Add(filePath);
                        }
                    });
                });

                // 5. 批量恢复仅有浮点数差异的文件
                if (!filesToRevert.IsEmpty)
                {
                    EditorUtility.DisplayProgressBar("检查文件", $"正在批量恢复 {filesToRevert.Count} 个文件...", 0.9f);
                    var files = filesToRevert.ToList();
                    
                    // 分批恢复，避免命令行过长
                    int batchSize = 50;
                    for (int i = 0; i < files.Count; i += batchSize)
                    {
                        var batch = files.Skip(i).Take(batchSize).Select(f => $"\"{f.Replace("\"", "\\\"")}\"");
                        
                        await Task.Run(() =>
                        {
                            var checkoutInfo = new ProcessStartInfo
                            {
                                FileName = "git",
                                Arguments = $"checkout -- {string.Join(" ", batch)}",
                                WorkingDirectory = gitRepoPath,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            };
                            
                            using (var p = Process.Start(checkoutInfo))
                            {
                                p.WaitForExit();
                            }
                        });
                    }
                }

                modifiedFiles.AddRange(trulyModifiedFiles);
            }

            // 6. 排序最终结果
            modifiedFiles = modifiedFiles.OrderBy(f => f).ToList();
            
            Debug.Log($"处理完成，最终修改文件数: {modifiedFiles.Count}");
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