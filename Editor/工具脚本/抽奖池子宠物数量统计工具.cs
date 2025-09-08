using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// 抽奖池子宠物数量统计工具
/// 统计GameConf/抽奖目录下所有抽奖池子中数量不等于1的宠物配置
/// </summary>
public class 抽奖池子宠物数量统计工具 : EditorWindow
{
    private Vector2 scrollPosition;
    private List<抽奖池子宠物信息> 统计结果 = new List<抽奖池子宠物信息>();
    private bool 已统计 = false;

    [System.Serializable]
    public class 抽奖池子宠物信息
    {
        public string 抽奖池名称;
        public string 宠物GUID;
        public int 数量;
        public int 权重;
        public string 文件路径;
    }

    [MenuItem("工具/抽奖池子宠物数量统计")]
    public static void ShowWindow()
    {
        GetWindow<抽奖池子宠物数量统计工具>("抽奖池子宠物数量统计");
    }

    private void OnGUI()
    {
        GUILayout.Label("抽奖池子宠物数量统计工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("开始统计", GUILayout.Height(30)))
        {
            开始统计();
        }

        GUILayout.Space(10);

        if (已统计)
        {
            var 非1数量宠物 = 统计结果.FindAll(x => x.数量 != 1);
            GUILayout.Label($"统计完成！找到 {非1数量宠物.Count} 个数量不等于1的宠物配置", EditorStyles.boldLabel);
            
            if (非1数量宠物.Count > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var 信息 in 非1数量宠物)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("抽奖池名称", 信息.抽奖池名称);
                    EditorGUILayout.LabelField("宠物GUID", 信息.宠物GUID);
                    EditorGUILayout.LabelField("数量", 信息.数量.ToString());
                    EditorGUILayout.LabelField("权重", 信息.权重.ToString());
                    EditorGUILayout.LabelField("文件路径", 信息.文件路径);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("所有抽奖池子中的宠物数量都是1，没有发现异常配置。", EditorStyles.helpBox);
            }
        }
    }

    private void 开始统计()
    {
        统计结果.Clear();
        已统计 = false;

        string 抽奖目录 = "Assets/GameConf/抽奖";
        if (!Directory.Exists(抽奖目录))
        {
            Debug.LogError($"抽奖目录不存在: {抽奖目录}");
            return;
        }

        // 获取所有.asset文件
        string[] 所有文件 = Directory.GetFiles(抽奖目录, "*.asset", SearchOption.AllDirectories);
        
        foreach (string 文件路径 in 所有文件)
        {
            解析抽奖配置文件(文件路径);
        }

        已统计 = true;
        var 非1数量宠物 = 统计结果.FindAll(x => x.数量 != 1);
        Debug.Log($"统计完成！共找到 {统计结果.Count} 个宠物配置，其中 {非1数量宠物.Count} 个数量不等于1");
    }

    private void 解析抽奖配置文件(string 文件路径)
    {
        try
        {
            string 内容 = File.ReadAllText(文件路径);
            string 文件名 = Path.GetFileNameWithoutExtension(文件路径);
            
            // 使用正则表达式匹配抽奖池子中的宠物配置
            // 匹配模式：奖励类型为2（宠物），提取GUID、数量和权重
            string 模式 = @"奖励类型:\s*2\s*\n(?:[^\n]*\n)*?宠物配置:\s*\{fileID:\s*\d+,\s*guid:\s*([a-f0-9]+),\s*type:\s*2\}\s*\n(?:[^\n]*\n)*?数量:\s*([0-9]+)\s*\n(?:[^\n]*\n)*?权重:\s*([0-9]+)";
            
            MatchCollection 匹配结果 = Regex.Matches(内容, 模式, RegexOptions.Multiline);
            
            foreach (Match 匹配 in 匹配结果)
            {
                string 宠物GUID = 匹配.Groups[1].Value;
                int 数量 = int.Parse(匹配.Groups[2].Value);
                int 权重 = int.Parse(匹配.Groups[3].Value);
                
                // 记录所有宠物配置，无论数量是否为1
                抽奖池子宠物信息 信息 = new 抽奖池子宠物信息
                {
                    抽奖池名称 = 文件名,
                    宠物GUID = 宠物GUID,
                    数量 = 数量,
                    权重 = 权重,
                    文件路径 = 文件路径
                };
                统计结果.Add(信息);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"解析文件失败: {文件路径}, 错误: {e.Message}");
        }
    }
}
