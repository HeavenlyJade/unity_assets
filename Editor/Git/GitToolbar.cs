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
    private const string SKIP_UNITY_PROJECT_KEY = "GitStatusViewer_SkipUnityProject";
    private const string ENABLE_IMPORT_BLOCKING_KEY = "GitStatusViewer_EnableImportBlocking";
    
    private string gitRepoPath = "";
    private Vector2 scrollPosition;
    private List<string> modifiedFiles = new List<string>();
    private bool isProcessing = false;
    private bool skipNoExtension = true;
    private bool skipConfig = true;
    private bool skipUnityProject = true;
    private bool enableImportBlocking = true;

    // 性能优化：缓存路径
    private string cachedUnityProjectPath = "";
    private string cachedGitRepoPath = "";

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
        skipUnityProject = EditorPrefs.GetBool(SKIP_UNITY_PROJECT_KEY, true);
        enableImportBlocking = EditorPrefs.GetBool(ENABLE_IMPORT_BLOCKING_KEY, true);
        
        // 预缓存Unity项目路径
        RefreshCachedPaths();
    }

    private void RefreshCachedPaths()
    {
        try
        {
            cachedUnityProjectPath = Path.GetFullPath(Path.GetDirectoryName(Application.dataPath));
            cachedGitRepoPath = string.IsNullOrEmpty(gitRepoPath) ? "" : Path.GetFullPath(gitRepoPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"缓存路径失败: {e.Message}");
            cachedUnityProjectPath = "";
            cachedGitRepoPath = "";
        }
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
            RefreshCachedPaths(); // 路径变化时刷新缓存
        }
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("选择 Git 仓库", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                gitRepoPath = path;
                EditorPrefs.SetString(GIT_REPO_PATH_KEY, gitRepoPath);
                RefreshCachedPaths(); // 路径变化时刷新缓存
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

        // Skip Unity Project toggle
        EditorGUI.BeginChangeCheck();
        skipUnityProject = EditorGUILayout.Toggle("跳过Unity项目文件", skipUnityProject);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool(SKIP_UNITY_PROJECT_KEY, skipUnityProject);
        }

        // Import Blocking toggle
        EditorGUI.BeginChangeCheck();
        enableImportBlocking = EditorGUILayout.Toggle("启用导入阻止机制", enableImportBlocking);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool(ENABLE_IMPORT_BLOCKING_KEY, enableImportBlocking);
        }

        EditorGUILayout.Space();

        // 性能提示
        if (skipUnityProject)
        {
            EditorGUILayout.HelpBox("已启用跳过Unity项目文件，可避免资源导入卡顿", MessageType.Info);
        }
        
        if (enableImportBlocking)
        {
            EditorGUILayout.HelpBox("已启用导入阻止机制，将完全阻止Unity资源导入卡顿", MessageType.Info);
        }

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
    /// 检查文件路径是否应该被跳过（基础版本，不涉及Unity项目检查）
    /// </summary>
    private bool ShouldSkipFileBasic(string filePath, string fileName)
    {
        // 跳过 .git 目录下的所有文件
        if (filePath.StartsWith(".git/") || filePath.StartsWith(".git\\"))
        {
            return true;
        }

        // 检查文件名是否在跳过列表中
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
    /// 优化版本：检查文件是否在Unity项目内
    /// </summary>
    private bool IsInUnityProjectOptimized(string filePath)
    {
        if (!skipUnityProject || string.IsNullOrEmpty(cachedUnityProjectPath) || string.IsNullOrEmpty(cachedGitRepoPath))
        {
            return false;
        }

        try
        {
            // 使用缓存的路径进行快速比较
            string fullPath = Path.GetFullPath(Path.Combine(cachedGitRepoPath, filePath));
            return fullPath.StartsWith(cachedUnityProjectPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
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

        // 记录开始时间
        var startTime = DateTime.Now;
        
        // 确保缓存路径是最新的
        RefreshCachedPaths();

        // 完整的导入阻止机制
        if (enableImportBlocking)
        {
            EditorApplication.LockReloadAssemblies();    // 锁定程序集
            AssetDatabase.DisallowAutoRefresh();         // 禁用自动刷新
            AssetDatabase.StartAssetEditing();           // 批量编辑模式
        }

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

            // 2. 优化后的文件解析 - 单次循环完成所有处理
            EditorUtility.DisplayProgressBar("检查文件", "正在高速解析文件列表...", 0.2f);
            
            var parseStartTime = DateTime.Now;
            
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var needDetailedCheck = new List<string>();
            int skippedUnityFiles = 0;
            int basicSkippedFiles = 0;
            
            // 单次循环处理所有文件
            foreach (var line in lines)
            {
                if (line.Length <= 3) continue;

                string status = line.Substring(0, 2).Trim();
                string rawPath = line.Substring(3);
                string decodedPath = DecodeGitPath(rawPath);
                string cleanFilePath = decodedPath.Trim('"');
                string fileName = Path.GetFileName(cleanFilePath);

                // 基础过滤（.git目录、配置文件等）
                if (ShouldSkipFileBasic(cleanFilePath, fileName))
                {
                    basicSkippedFiles++;
                    continue;
                }

                // Unity项目文件检查
                if (IsInUnityProjectOptimized(cleanFilePath))
                {
                    skippedUnityFiles++;
                    continue;
                }
                
                // 新增和删除的文件直接添加到修改列表
                if (status == "D" || status.Contains("A"))
                {
                    modifiedFiles.Add(cleanFilePath);
                    continue;
                }
                
                // 判断是否需要详细检查此文件
                string extension = Path.GetExtension(cleanFilePath).ToLower();
                
                // 只处理 JSON 文件和 childrenIndex 文件
                if (extension == ".json" || fileName == "childrenIndex")
                {
                    needDetailedCheck.Add(cleanFilePath);
                }
                // Lua 文件和其他文件都跳过，不加入任何列表
            }
            
            var parseEndTime = DateTime.Now;
            var parseDuration = parseEndTime - parseStartTime;
            
            Debug.Log($"高速解析完成 - 耗时: {parseDuration.TotalMilliseconds:F0}ms");
            Debug.Log($"统计: 新增/删除: {modifiedFiles.Count} 个, 需详细检查: {needDetailedCheck.Count} 个");
            Debug.Log($"跳过: Unity项目文件: {skippedUnityFiles} 个, 基础过滤: {basicSkippedFiles} 个");

            // 3. 只对需要详细检查的文件进行内容比较
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
                            
                            // 只对JSON文件处理浮点数
                            string processedContent = currentContent;
                            if (Path.GetExtension(filePath).ToLower() == ".json")
                            {
                                processedContent = ProcessFloatNumbers(currentContent);
                            }

                            // 比较内容（JSON文件进行换行符归一化）
                            bool areEqual;
                            if (Path.GetExtension(filePath).ToLower() == ".json")
                            {
                                areEqual = NormalizeLineEndings(originalContent) == NormalizeLineEndings(processedContent);
                            }
                            else
                            {
                                // childrenIndex 文件直接比较
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

                // 4. 批量恢复仅有浮点数差异的文件
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
                    
                    Debug.Log($"已恢复 {filesToRevert.Count} 个仅有浮点数差异的文件");
                }

                modifiedFiles.AddRange(trulyModifiedFiles);
            }

            // 5. 排序最终结果
            modifiedFiles = modifiedFiles.OrderBy(f => f).ToList();
            
            // 计算处理时间
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            
            Debug.Log($"处理完成，最终修改文件数: {modifiedFiles.Count}，总耗时: {duration.TotalMilliseconds:F0}ms");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"执行 git 命令失败: {e.Message}", "确定");
            Debug.LogError(e);
        }
        finally
        {
            // 完整的导入恢复机制
            if (enableImportBlocking)
            {
                AssetDatabase.StopAssetEditing();           // 结束批量编辑模式
                AssetDatabase.AllowAutoRefresh();           // 恢复自动刷新
                EditorApplication.UnlockReloadAssemblies(); // 解锁程序集
            }
            
            EditorUtility.ClearProgressBar();
            isProcessing = false;
            Repaint();
        }
    }
}