using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using MiGame.Scene;

namespace MiGame.Editor
{
    /// <summary>
    /// 挂机点指令更新工具
    /// 根据挂机点配置JSON文件中的numericalValue值，替换指令模板中的数值
    /// </summary>
    public class 挂机点指令更新工具 : EditorWindow
    {
        private string 挂机点配置路径 = "Assets/配置exel/挂机点配置.json";
        private string 指令模板 = "variable { \"操作类型\": \"新增\", \"变量名\": \"数据_固定值_战力值\", \"数值\": {数值}, \"玩家变量加成\": [{ \"名称\": \"特权_百分比_训练加成\", \"作用类型\": \"基础相乘\", \"缩放倍率\": 1 }, { \"名称\": \"数据_固定值_重生次数\", \"作用类型\": \"基础相乘\", \"缩放倍率\": 0.1 }, { \"名称\": \"天赋_百分比_训练加成\", \"作用类型\": \"基础相乘\", \"缩放倍率\": 1 }, { \"名称\": \"特权_百分比_双倍训练\", \"作用类型\": \"基础相乘\", \"缩放倍率\": 1 }, { \"名称\": \"加成_百分比_训练加成\", \"作用类型\": \"单独相加\", \"缩放倍率\": 0 } ], \"其他加成\": [ \"宠物\", \"伙伴\", \"尾迹\", \"翅膀\" ] }";
        private Vector2 滚动位置;
        private List<string> 处理日志 = new List<string>();
        private List<挂机点配置数据> 挂机点配置列表 = new List<挂机点配置数据>();

        [MenuItem("Tools/场景/挂机点指令更新工具")]
        public static void ShowWindow()
        {
            GetWindow<挂机点指令更新工具>("挂机点指令更新工具");
        }

        private void OnGUI()
        {
            GUILayout.Label("挂机点指令更新工具", EditorStyles.boldLabel);
            GUILayout.Label("根据numericalValue值替换指令模板中的数值", EditorStyles.miniLabel);
            
            EditorGUILayout.Space();
            
            // 挂机点配置路径设置
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("挂机点配置路径:", GUILayout.Width(120));
            挂机点配置路径 = EditorGUILayout.TextField(挂机点配置路径);
            if (GUILayout.Button("选择文件", GUILayout.Width(80)))
            {
                string 选择的路径 = EditorUtility.OpenFilePanel("选择挂机点配置文件", "Assets", "json");
                if (!string.IsNullOrEmpty(选择的路径))
                {
                    挂机点配置路径 = 选择的路径.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 指令模板设置
            GUILayout.Label("指令模板 (使用{数值}作为占位符):", EditorStyles.boldLabel);
            指令模板 = EditorGUILayout.TextArea(指令模板, GUILayout.Height(100));
            
            EditorGUILayout.Space();
            
            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("加载挂机点配置", GUILayout.Height(30)))
            {
                加载挂机点配置();
            }
            if (GUILayout.Button("备份原始配置", GUILayout.Height(30)))
            {
                备份原始配置();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("更新所有挂机点指令", GUILayout.Height(30)))
            {
                更新所有挂机点指令();
            }
            if (GUILayout.Button("验证指令模板", GUILayout.Height(30)))
            {
                验证指令模板();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 显示挂机点配置列表
            if (挂机点配置列表.Count > 0)
            {
                GUILayout.Label($"已加载 {挂机点配置列表.Count} 个挂机点配置:", EditorStyles.boldLabel);
                滚动位置 = EditorGUILayout.BeginScrollView(滚动位置, GUILayout.Height(200));
                
                foreach (var 配置 in 挂机点配置列表)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{配置.fileName}: {配置.numericalValue}", GUILayout.Width(200));
                    if (GUILayout.Button("预览指令", GUILayout.Width(80)))
                    {
                        预览指令(配置);
                    }
                    if (GUILayout.Button("更新此配置", GUILayout.Width(80)))
                    {
                        更新单个挂机点指令(配置);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.Space();
            
            // 显示处理日志
            if (处理日志.Count > 0)
            {
                GUILayout.Label("处理日志:", EditorStyles.boldLabel);
                Vector2 日志滚动位置 = EditorGUILayout.BeginScrollView(new Vector2(), GUILayout.Height(150));
                
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
        /// 加载挂机点配置
        /// </summary>
        private void 加载挂机点配置()
        {
            处理日志.Clear();
            添加日志("开始加载挂机点配置...");
            
            try
            {
                if (!File.Exists(挂机点配置路径))
                {
                    添加日志($"错误: 配置文件不存在: {挂机点配置路径}");
                    return;
                }
                
                string json内容 = File.ReadAllText(挂机点配置路径);
                var 导出数据 = JsonUtility.FromJson<导出数据结构>(json内容);
                
                if (导出数据 == null || 导出数据.configs == null)
                {
                    添加日志("错误: JSON解析失败或数据结构不正确");
                    return;
                }
                
                挂机点配置列表 = 导出数据.configs;
                添加日志($"成功加载 {挂机点配置列表.Count} 个挂机点配置");
                
                // 刷新资源数据库
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                添加日志($"加载失败: {e.Message}");
            }
        }

        /// <summary>
        /// 更新所有挂机点指令
        /// </summary>
        private void 更新所有挂机点指令()
        {
            if (挂机点配置列表.Count == 0)
            {
                添加日志("请先加载挂机点配置");
                return;
            }
            
            if (!验证指令模板())
            {
                return;
            }
            
            // 询问是否备份
            if (!EditorUtility.DisplayDialog("确认更新", 
                $"即将更新 {挂机点配置列表.Count} 个挂机点配置的定时指令。\n建议先备份原始配置。\n\n是否继续？", 
                "继续", "取消"))
            {
                return;
            }
            
            处理日志.Clear();
            添加日志("开始更新所有挂机点指令...");
            
            int 成功数量 = 0;
            int 失败数量 = 0;
            
            foreach (var 配置 in 挂机点配置列表)
            {
                try
                {
                    if (更新单个挂机点指令(配置))
                    {
                        成功数量++;
                    }
                    else
                    {
                        失败数量++;
                    }
                }
                catch (System.Exception e)
                {
                    添加日志($"更新 {配置.fileName} 失败: {e.Message}");
                    失败数量++;
                }
            }
            
            添加日志($"更新完成! 成功: {成功数量}, 失败: {失败数量}");
            
            // 刷新资源数据库
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("更新完成", $"成功更新 {成功数量} 个挂机点配置\n失败 {失败数量} 个", "确定");
        }

        /// <summary>
        /// 更新单个挂机点指令
        /// </summary>
        private bool 更新单个挂机点指令(挂机点配置数据 配置)
        {
            try
            {
                // 生成新指令
                string 新指令 = 生成指令(配置);
                
                // 查找对应的配置文件
                string 配置文件路径 = 查找配置文件路径(配置.fileName);
                if (string.IsNullOrEmpty(配置文件路径))
                {
                    添加日志($"警告: 找不到配置文件 {配置.fileName}");
                    return false;
                }
                
                // 加载配置文件
                SceneNodeConfig 场景配置 = AssetDatabase.LoadAssetAtPath<SceneNodeConfig>(配置文件路径);
                if (场景配置 == null)
                {
                    添加日志($"错误: 无法加载配置文件 {配置文件路径}");
                    return false;
                }
                
                // 确保定时指令列表存在
                if (场景配置.定时指令列表 == null)
                {
                    场景配置.定时指令列表 = new List<TimedCommand>();
                }
                
                // 更新第一个定时指令
                if (场景配置.定时指令列表.Count > 0)
                {
                    场景配置.定时指令列表[0].指令 = 新指令;
                    添加日志($"已更新 {配置.fileName} 的第一个定时指令");
                }
                else
                {
                    // 如果没有定时指令，创建一个新的
                    var 新定时指令 = new TimedCommand
                    {
                        指令 = 新指令,
                        间隔 = 1.0f
                    };
                    场景配置.定时指令列表.Add(新定时指令);
                    添加日志($"已为 {配置.fileName} 创建新的定时指令");
                }
                
                // 标记为已修改
                EditorUtility.SetDirty(场景配置);
                
                return true;
            }
            catch (System.Exception e)
            {
                添加日志($"更新 {配置.fileName} 失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 生成指令
        /// </summary>
        private string 生成指令(挂机点配置数据 配置)
        {
            return 指令模板.Replace("{数值}", 配置.numericalValue.ToString());
        }

        /// <summary>
        /// 预览指令
        /// </summary>
        private void 预览指令(挂机点配置数据 配置)
        {
            string 预览指令 = 生成指令(配置);
            EditorUtility.DisplayDialog($"预览 {配置.fileName} 的指令", 预览指令, "确定");
        }

        /// <summary>
        /// 验证指令模板
        /// </summary>
        private bool 验证指令模板()
        {
            if (string.IsNullOrEmpty(指令模板))
            {
                添加日志("错误: 指令模板不能为空");
                EditorUtility.DisplayDialog("验证失败", "指令模板不能为空", "确定");
                return false;
            }
            
            if (!指令模板.Contains("{数值}"))
            {
                添加日志("错误: 指令模板必须包含 {数值} 占位符");
                EditorUtility.DisplayDialog("验证失败", "指令模板必须包含 {数值} 占位符", "确定");
                return false;
            }
            
            // 测试模板是否有效
            try
            {
                string 测试指令 = 指令模板.Replace("{数值}", "999");
                if (string.IsNullOrEmpty(测试指令))
                {
                    添加日志("错误: 指令模板生成失败");
                    return false;
                }
                添加日志("指令模板验证通过");
            }
            catch (System.Exception e)
            {
                添加日志($"指令模板验证失败: {e.Message}");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 备份原始配置
        /// </summary>
        private void 备份原始配置()
        {
            if (挂机点配置列表.Count == 0)
            {
                添加日志("请先加载挂机点配置");
                return;
            }
            
            处理日志.Clear();
            添加日志("开始备份原始配置...");
            
            try
            {
                string 备份目录 = "Assets/配置exel/备份";
                if (!Directory.Exists(备份目录))
                {
                    Directory.CreateDirectory(备份目录);
                }
                
                string 备份文件名 = $"挂机点配置备份_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
                string 备份路径 = Path.Combine(备份目录, 备份文件名);
                
                // 创建备份数据结构
                var 备份数据 = new 导出数据结构
                {
                    exportTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalCount = 挂机点配置列表.Count,
                    configs = 挂机点配置列表
                };
                
                string 备份json = JsonUtility.ToJson(备份数据, true);
                File.WriteAllText(备份路径, 备份json);
                
                添加日志($"备份完成: {备份路径}");
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("备份完成", $"原始配置已备份到:\n{备份路径}", "确定");
            }
            catch (System.Exception e)
            {
                添加日志($"备份失败: {e.Message}");
                EditorUtility.DisplayDialog("备份失败", $"备份过程中发生错误:\n{e.Message}", "确定");
            }
        }

        /// <summary>
        /// 查找配置文件路径
        /// </summary>
        private string 查找配置文件路径(string 文件名)
        {
            // 在GameConf/场景/挂机点目录下查找
            string[] 搜索路径s = {
                "Assets/GameConf/场景/挂机点",
                "Assets/GameConf/场景/抽奖点"
            };
            
            foreach (string 搜索路径 in 搜索路径s)
            {
                if (Directory.Exists(搜索路径))
                {
                    string[] 子目录s = Directory.GetDirectories(搜索路径, "*", SearchOption.AllDirectories);
                    foreach (string 子目录 in 子目录s)
                    {
                        string 配置文件路径 = Path.Combine(子目录, $"{文件名}.asset");
                        if (File.Exists(配置文件路径))
                        {
                            return 配置文件路径.Replace("\\", "/");
                        }
                    }
                }
            }
            
            // 如果没找到，尝试在Assets目录下全局搜索
            string[] 所有配置文件 = AssetDatabase.FindAssets($"{文件名} t:SceneNodeConfig");
            if (所有配置文件.Length > 0)
            {
                string 找到的路径 = AssetDatabase.GUIDToAssetPath(所有配置文件[0]);
                添加日志($"通过全局搜索找到配置文件: {找到的路径}");
                return 找到的路径;
            }
            
            return null;
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        private void 添加日志(string 消息)
        {
            处理日志.Add($"[{System.DateTime.Now:HH:mm:ss}] {消息}");
            Debug.Log($"挂机点指令更新工具: {消息}");
            Repaint();
        }
    }

    /// <summary>
    /// 挂机点配置数据结构
    /// </summary>
    [System.Serializable]
    public class 挂机点配置数据
    {
        public string fileName;           // 文件名作为唯一key
        public int numericalValue;        // 作数值的配置（整数）
    }

    /// <summary>
    /// 导出数据结构
    /// </summary>
    [System.Serializable]
    public class 导出数据结构
    {
        public string exportTime;                    // 导出时间
        public int totalCount;                       // 挂机点总数
        public List<挂机点配置数据> configs;          // 挂机点配置列表
    }
}
