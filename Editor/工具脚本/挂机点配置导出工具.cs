using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using MiGame.Scene;

namespace MiGame.Editor
{
    /// <summary>
    /// 挂机点配置导出工具
    /// 只导出作数值的配置和定时指令列表
    /// </summary>
    public class 挂机点配置导出工具 : EditorWindow
    {
        private string 输出路径 = "Assets/配置exel/挂机点配置.json";
        private Vector2 滚动位置;
        private List<string> 处理日志 = new List<string>();

        [MenuItem("Tools/场景/挂机点配置导出工具")]
        public static void ShowWindow()
        {
            GetWindow<挂机点配置导出工具>("挂机点配置导出工具");
        }

        private void OnGUI()
        {
            GUILayout.Label("挂机点配置导出工具", EditorStyles.boldLabel);
            GUILayout.Label("只导出作数值的配置（整数）", EditorStyles.miniLabel);
            
            EditorGUILayout.Space();
            
            // 输出路径设置
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("输出路径:", GUILayout.Width(80));
            输出路径 = EditorGUILayout.TextField(输出路径);
            if (GUILayout.Button("选择路径", GUILayout.Width(80)))
            {
                string 选择的路径 = EditorUtility.SaveFilePanel("选择输出路径", "Assets", "挂机点配置", "json");
                if (!string.IsNullOrEmpty(选择的路径))
                {
                    输出路径 = 选择的路径.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 操作按钮
            if (GUILayout.Button("导出挂机点配置", GUILayout.Height(30)))
            {
                导出挂机点配置();
            }
            
            EditorGUILayout.Space();
            
            // 显示处理日志
            if (处理日志.Count > 0)
            {
                GUILayout.Label("处理日志:", EditorStyles.boldLabel);
                滚动位置 = EditorGUILayout.BeginScrollView(滚动位置, GUILayout.Height(200));
                
                foreach (string 日志 in 处理日志)
                {
                    EditorGUILayout.HelpBox(日志, MessageType.Info);
                }
                
                EditorGUILayout.EndScrollView();
                
                if (GUILayout.Button("清空日志"))
                {
                    处理日志.Clear();
                }
            }
        }

        /// <summary>
        /// 导出挂机点配置
        /// </summary>
        private void 导出挂机点配置()
        {
            处理日志.Clear();
            添加日志("开始导出挂机点配置...");
            
            try
            {
                // 查找所有挂机点配置
                string[] 挂机点配置路径s = AssetDatabase.FindAssets("t:SceneNodeConfig");
                List<挂机点配置数据> 挂机点配置列表 = new List<挂机点配置数据>();
                
                添加日志($"找到 {挂机点配置路径s.Length} 个场景节点配置");
                
                foreach (string 配置路径 in 挂机点配置路径s)
                {
                    string 完整路径 = AssetDatabase.GUIDToAssetPath(配置路径);
                    SceneNodeConfig 配置 = AssetDatabase.LoadAssetAtPath<SceneNodeConfig>(完整路径);
                    
                    if (配置 != null && 配置.场景类型 == SceneNodeType.挂机点)
                    {
                        挂机点配置数据 数据 = 提取挂机点配置(配置, 完整路径);
                        挂机点配置列表.Add(数据);
                        添加日志($"处理挂机点: {配置.名字} ({完整路径})");
                        添加日志($"  - 作数值: {配置.作数值的配置} -> 导出值: {数据.numericalValue}");
                    }
                }
                
                添加日志($"找到 {挂机点配置列表.Count} 个挂机点配置");
                
                if (挂机点配置列表.Count == 0)
                {
                    添加日志("警告: 没有找到任何挂机点配置！");
                    EditorUtility.DisplayDialog("导出警告", "没有找到任何挂机点配置，请检查场景节点配置。", "确定");
                    return;
                }
                
                // 创建导出数据结构
                var 导出数据 = new 导出数据结构
                {
                    exportTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalCount = 挂机点配置列表.Count,
                    configs = 挂机点配置列表
                };
                
                添加日志($"导出数据结构创建完成，包含 {导出数据.configs.Count} 个配置");
                
                // 调试：检查数据结构
                添加日志($"调试信息: exportTime={导出数据.exportTime}, totalCount={导出数据.totalCount}");
                if (导出数据.configs != null && 导出数据.configs.Count > 0)
                {
                    var 第一个配置 = 导出数据.configs[0];
                    添加日志($"第一个配置: fileName={第一个配置.fileName}, numericalValue={第一个配置.numericalValue}");
                }
                
                // 转换为JSON
                string json内容 = JsonUtility.ToJson(导出数据, true);
                添加日志($"JSON序列化完成，内容长度: {json内容.Length}");
                添加日志($"JSON内容预览: {json内容.Substring(0, Mathf.Min(200, json内容.Length))}...");
                
                if (string.IsNullOrEmpty(json内容) || json内容 == "{}")
                {
                    添加日志("错误: JSON序列化失败，生成的内容为空！");
                    
                    // 尝试手动序列化第一个对象进行调试
                    if (挂机点配置列表.Count > 0)
                    {
                        string 调试json = JsonUtility.ToJson(挂机点配置列表[0], true);
                        添加日志($"单个对象序列化测试: {调试json}");
                    }
                    
                    EditorUtility.DisplayDialog("导出失败", "JSON序列化失败，请检查数据结构。", "确定");
                    return;
                }
                
                // 保存文件
                string 目录路径 = Path.GetDirectoryName(输出路径);
                if (!Directory.Exists(目录路径))
                {
                    Directory.CreateDirectory(目录路径);
                }
                
                File.WriteAllText(输出路径, json内容);
                
                添加日志($"导出完成! 文件保存至: {输出路径}");
                添加日志($"共导出 {挂机点配置列表.Count} 个挂机点配置");
                
                // 刷新资源数据库
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("导出完成", $"成功导出 {挂机点配置列表.Count} 个挂机点配置到:\n{输出路径}", "确定");
            }
            catch (System.Exception e)
            {
                添加日志($"导出失败: {e.Message}");
                添加日志($"堆栈信息: {e.StackTrace}");
                EditorUtility.DisplayDialog("导出失败", $"导出过程中发生错误:\n{e.Message}", "确定");
            }
        }

        /// <summary>
        /// 提取挂机点配置数据
        /// </summary>
        private 挂机点配置数据 提取挂机点配置(SceneNodeConfig 配置, string 文件路径)
        {
            var 数据 = new 挂机点配置数据
            {
                fileName = Path.GetFileNameWithoutExtension(文件路径),
                numericalValue = Mathf.RoundToInt(配置.作数值的配置)  // 转换为整数
            };
            
            return 数据;
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        private void 添加日志(string 消息)
        {
            处理日志.Add($"[{System.DateTime.Now:HH:mm:ss}] {消息}");
            Debug.Log($"挂机点导出工具: {消息}");  // 同时输出到Console
            Repaint();
        }
    }


}