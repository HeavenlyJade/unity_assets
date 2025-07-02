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

public class GitStatusViewerWindow : EditorWindow
{
    private const string GIT_REPO_PATH_KEY = "GitStatusViewer_GitRepoPath";
    private const string SKIP_NO_EXTENSION_KEY = "GitStatusViewer_SkipNoExtension";
    private const string SKIP_CONFIG_KEY = "GitStatusViewer_SkipConfig";
    
    // 添加调试模式控制
    private const bool DEBUG_MODE = false; // 生产环境设为false
    
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

    // 添加调试日志方法
    private static void DebugLog(string message)
    {
        if (DEBUG_MODE)
        {
            Debug.Log(message);
        }
    }

    private static void DebugLogError(string message)
    {
        if (DEBUG_MODE)
        {
            Debug.LogError(message);
        }
    }

    [MenuItem("Tools/Git 状态查看器")]
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

    private bool HasGitDiff(string filePath)
    {
        try
        {
            string fullPath = Path.Combine(gitRepoPath, filePath);
            string content = File.ReadAllText(fullPath);
            string extension = Path.GetExtension(filePath).ToLower();
            
            // 如果不是lua文件，处理浮点数
            if (extension != ".lua")
            {
                string processedContent = ProcessFloatNumbers(content);
                if (processedContent != content)
                {
                    // 如果内容有变化，写入处理后的内容
                    File.WriteAllText(fullPath, processedContent);
                    
                    // 检查处理后的文件是否还有差异
                    ProcessStartInfo checkStartInfo = new ProcessStartInfo();
                    checkStartInfo.FileName = "git";
                    checkStartInfo.Arguments = $"diff --quiet \"{filePath}\"";
                    checkStartInfo.WorkingDirectory = gitRepoPath;
                    checkStartInfo.UseShellExecute = false;
                    checkStartInfo.RedirectStandardOutput = true;
                    checkStartInfo.CreateNoWindow = true;
                    checkStartInfo.StandardOutputEncoding = Encoding.UTF8;

                    using (Process checkProcess = Process.Start(checkStartInfo))
                    {
                        checkProcess.WaitForExit();
                        // 如果处理后的文件没有差异，执行checkout
                        if (checkProcess.ExitCode == 0)
                        {
                            ProcessStartInfo checkoutStartInfo = new ProcessStartInfo();
                            checkoutStartInfo.FileName = "git";
                            checkoutStartInfo.Arguments = $"checkout -- \"{filePath}\"";
                            checkoutStartInfo.WorkingDirectory = gitRepoPath;
                            checkoutStartInfo.UseShellExecute = false;
                            checkoutStartInfo.RedirectStandardOutput = true;
                            checkoutStartInfo.CreateNoWindow = true;
                            checkoutStartInfo.StandardOutputEncoding = Encoding.UTF8;

                            using (Process checkoutProcess = Process.Start(checkoutStartInfo))
                            {
                                checkoutProcess.WaitForExit();
                            }
                            return false;
                        }
                    }
                    return true;
                }
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "git";
            startInfo.Arguments = $"diff --quiet \"{filePath}\"";
            startInfo.WorkingDirectory = gitRepoPath;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                // git diff --quiet 在文件有差异时返回1，无差异时返回0
                return process.ExitCode == 1;
            }
        }
        catch (System.Exception e)
        {
            DebugLogError($"Failed to check git diff for {filePath}: {e.Message}");
            return false;
        }
    }

    private bool HasChildrenIndexDiff(string filePath)
    {
        try
        {
            DebugLog($"开始检查childrenIndex文件差异: {filePath}");
            
            // 获取git diff的输出
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "git";
            startInfo.Arguments = $"diff \"{filePath}\"";
            startInfo.WorkingDirectory = gitRepoPath;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // 如果没有差异，直接返回false
                if (string.IsNullOrEmpty(output))
                {
                    DebugLog($"文件 {filePath} 没有差异");
                    return false;
                }

                DebugLog($"文件 {filePath} 的diff输出:\n{output}");

                // 获取childrenIndex文件的修改时间
                string fullPath = Path.Combine(gitRepoPath, filePath);
                DateTime indexFileTime = File.GetLastWriteTime(fullPath);
                DebugLog($"childrenIndex文件修改时间: {indexFileTime}");

                // 分析diff输出
                string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                bool hasContentChange = false;
                List<string> removedContent = new List<string>();
                List<string> addedContent = new List<string>();

                foreach (string line in lines)
                {
                    // 跳过git diff的头部信息
                    if (line.StartsWith("diff --git") || line.StartsWith("index ") || 
                        line.StartsWith("---") || line.StartsWith("+++"))
                    {
                        DebugLog($"跳过git diff头部信息: {line}");
                        continue;
                    }

                    // 检查是否是内容行（以+或-开头）
                    if (line.StartsWith("+") || line.StartsWith("-"))
                    {
                        string content = line.Substring(1).Trim();
                        DebugLog($"检查内容行: {content}");
                        
                        // 如果行包含分号，检查分号后的内容是否变化
                        int semicolonIndex = content.IndexOf(';');
                        if (semicolonIndex != -1)
                        {
                            string beforeSemicolon = content.Substring(0, semicolonIndex);
                            string afterSemicolon = content.Substring(semicolonIndex + 1);
                            
                            if (line.StartsWith("-"))
                            {
                                // 记录被删除行的分号后内容
                                removedContent.Add(afterSemicolon);
                            }
                            else if (line.StartsWith("+"))
                            {
                                // 记录新增行的分号后内容
                                addedContent.Add(afterSemicolon);
                                // 检查是否有对应的删除行（分号后内容相同）
                                if (removedContent.Contains(afterSemicolon))
                                {
                                    // 如果找到匹配的删除行，移除它
                                    removedContent.Remove(afterSemicolon);
                                }
                                else
                                {
                                    // 如果是新增的行，标记为需要更新
                                    DebugLog($"检测到新增行: {content}");
                                    hasContentChange = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            DebugLog($"行不包含分号，忽略: {content}");
                        }
                    }
                }

                if (removedContent.Count > 0)
                {
                    DebugLog($"检测到未匹配的删除行: {string.Join(", ", removedContent)}");
                    hasContentChange = true;

                    // 获取childrenIndex文件所在目录
                    string directory = Path.GetDirectoryName(fullPath);
                    DebugLog($"检查目录: {directory}");

                    // 遍历目录中的所有文件
                    string[] allFiles = Directory.GetFiles(directory, "*.*");
                    foreach (string file in allFiles)
                    {
                        try
                        {
                            // 检查文件修改时间是否在childrenIndex文件的10秒前
                            DateTime fileTime = File.GetLastWriteTime(file);
                            if (fileTime < indexFileTime.AddSeconds(-10))
                            {
                                string fileName = Path.GetFileName(file);
                                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                                
                                // 检查是否有同名文件夹
                                string folderPath = Path.Combine(directory, fileNameWithoutExt);
                                if (Directory.Exists(folderPath))
                                {
                                    // 获取文件夹中的所有文件
                                    string[] folderFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                                    foreach (string folderFile in folderFiles)
                                    {
                                        // 获取相对路径
                                        string relativePath = folderFile.Substring(gitRepoPath.Length).TrimStart(Path.DirectorySeparatorChar);
                                        // 添加到修改文件列表
                                        if (!modifiedFiles.Contains(relativePath))
                                        {
                                            modifiedFiles.Add(relativePath);
                                            DebugLog($"添加到修改文件列表: {relativePath}");
                                        }
                                    }

                                    // 删除文件夹
                                    Directory.Delete(folderPath, true);
                                    DebugLog($"已删除文件夹: {folderPath}");
                                }

                                // 删除文件
                                File.Delete(file);
                                DebugLog($"已删除文件: {file}");
                            }
                        }
                        catch (System.Exception e)
                        {
                            DebugLogError($"处理文件 {file} 失败: {e.Message}");
                        }
                    }
                }

                DebugLog($"文件 {filePath} 的检查结果: {(hasContentChange ? "需要更新" : "无需更新")}");
                return hasContentChange;
            }
        }
        catch (System.Exception e)
        {
            DebugLogError($"检查childrenIndex文件差异失败 {filePath}: {e.Message}");
            return false;
        }
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

            // 获取git status输出
            string output = await Task.Run(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "git";
                startInfo.Arguments = "status --porcelain";
                startInfo.WorkingDirectory = gitRepoPath;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                startInfo.StandardOutputEncoding = Encoding.UTF8;

                using (Process process = Process.Start(startInfo))
                {
                    string result = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return result;
                }
            });

            string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var fileTasks = new List<Task>();
            var modifiedFilesLock = new object();
            var unchangedFiles = new List<string>();

            EditorUtility.DisplayProgressBar("检查文件", "正在检查文件差异...", 0.3f);

            // 创建任务列表
            foreach (string line in lines)
            {
                if (line.Length > 3)
                {
                    string filePath = line.Substring(3).Trim();
                    string status = line.Substring(0, 2).Trim();
                    
                    // 打印原始路径
                    DebugLog($"原始Git输出行: '{line}'");
                    DebugLog($"提取的文件路径: '{filePath}'");

                    // 解码Git路径
                    filePath = DecodeGitPath(filePath);
                    DebugLog($"解码后的路径: '{filePath}'");

                    // 移除文件路径中的引号并处理空格
                    string cleanFilePath = filePath.Trim('"');
                    DebugLog($"清理后的路径: '{cleanFilePath}'");

                    string fullPath;
                    try
                    {
                        fullPath = Path.Combine(gitRepoPath, cleanFilePath);
                        DebugLog($"成功组合路径: '{fullPath}'");
                    }
                    catch (System.Exception e)
                    {
                        DebugLogError($"路径组合失败!");
                        DebugLogError($"Git仓库路径: '{gitRepoPath}'");
                        DebugLogError($"文件路径: '{cleanFilePath}'");
                        DebugLogError($"错误信息: {e.Message}");
                        
                        // 检查路径中的每个字符
                        DebugLogError("文件路径字符分析:");
                        for (int i = 0; i < cleanFilePath.Length; i++)
                        {
                            char c = cleanFilePath[i];
                            DebugLogError($"字符[{i}]: '{c}' (ASCII: {(int)c})");
                        }
                        continue; // 跳过这个文件，继续处理下一个
                    }

                    // 检查文件是否在跳过列表中
                    string fileName = Path.GetFileName(cleanFilePath);
                    if ((skipConfig && fileName.EndsWith("Config.lua")) || 
                        skippingFiles.Contains(fileName) || 
                        (skipNoExtension && cleanFilePath.Contains("sandbox/assets")))
                    {
                        // 将跳过的文件添加到unchangedFiles列表
                        lock (modifiedFilesLock)
                        {
                            unchangedFiles.Add(cleanFilePath);
                        }
                        continue;
                    }

                    // 如果文件被删除，直接添加到修改列表中
                    if (status == "D" || !File.Exists(fullPath))
                    {
                        lock (modifiedFilesLock)
                        {
                            modifiedFiles.Add(cleanFilePath);
                        }
                        continue;
                    }

                    // 创建检查文件差异的任务
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            if (HasGitDiff(cleanFilePath))
                            {
                                lock (modifiedFilesLock)
                                {
                                    modifiedFiles.Add(cleanFilePath);
                                }
                            }
                            else
                            {
                                lock (modifiedFilesLock)
                                {
                                    unchangedFiles.Add(cleanFilePath);
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            DebugLogError($"处理文件 {cleanFilePath} 失败: {e.Message}");
                        }
                    });

                    fileTasks.Add(task);
                }
            }

            // 等待所有任务完成
            int totalTasks = fileTasks.Count;
            int completedTasks = 0;

            while (fileTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(fileTasks);
                fileTasks.Remove(completedTask);
                completedTasks++;

                float progress = 0.3f + (0.6f * completedTasks / totalTasks);
                EditorUtility.DisplayProgressBar("检查文件", $"正在检查文件差异... ({completedTasks}/{totalTasks})", progress);
            }

            // 对未修改的文件执行git checkout
            if (unchangedFiles.Count > 0)
            {
                EditorUtility.DisplayProgressBar("检查文件", "正在恢复未修改的文件...", 0.9f);
                
                foreach (string file in unchangedFiles)
                {
                    try
                    {
                        ProcessStartInfo checkoutStartInfo = new ProcessStartInfo();
                        checkoutStartInfo.FileName = "git";
                        checkoutStartInfo.Arguments = $"checkout -- \"{file}\"";
                        checkoutStartInfo.WorkingDirectory = gitRepoPath;
                        checkoutStartInfo.UseShellExecute = false;
                        checkoutStartInfo.RedirectStandardOutput = true;
                        checkoutStartInfo.CreateNoWindow = true;
                        checkoutStartInfo.StandardOutputEncoding = Encoding.UTF8;

                        using (Process process = Process.Start(checkoutStartInfo))
                        {
                            process.WaitForExit();
                        }
                    }
                    catch (System.Exception e)
                    {
                        DebugLogError($"恢复文件 {file} 失败: {e.Message}");
                    }
                }
            }

            EditorUtility.DisplayProgressBar("检查文件", "完成", 1.0f);
            await Task.Delay(500); // 短暂延迟以确保进度条显示完成
            EditorUtility.ClearProgressBar();

            // 更新UI
            Repaint();
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("错误", $"执行 git 命令失败: {e.Message}", "确定");
            DebugLogError(e.ToString());
        }
        finally
        {
            isProcessing = false;
            Repaint();
        }
    }
} 