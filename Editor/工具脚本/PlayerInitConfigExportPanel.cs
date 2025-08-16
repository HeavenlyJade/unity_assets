#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using MiGame.Data;

namespace MiGame.Editor
{
    /// <summary>
    /// 玩家初始化配置导出面板
    /// </summary>
    public class PlayerInitConfigExportPanel : EditorWindow
    {
        private const string DefaultOutputPath = "Assets/Lua/MiGame/Data/PlayerInitConfig.lua";
        private const string OutputPathPrefKey = "PlayerInitConfig_OutputPath";
        
        private string _outputPath;

        [MenuItem("工具/配置导出/玩家初始化配置面板")]
        public static void ShowWindow()
        {
            GetWindow<PlayerInitConfigExportPanel>("玩家初始化配置导出");
        }

        private void OnEnable()
        {
            _outputPath = EditorPrefs.GetString(OutputPathPrefKey, DefaultOutputPath);
        }

        private void OnGUI()
        {
            GUILayout.Label("玩家初始化配置导出", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("导出所有PlayerInitConfig配置文件到Lua格式", MessageType.Info);
            
            EditorGUILayout.Space();
            
            // 输出路径设置
            EditorGUILayout.LabelField("导出设置", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _outputPath = EditorGUILayout.TextField("输出路径", _outputPath);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(OutputPathPrefKey, _outputPath);
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("选择路径"))
            {
                string selectedPath = EditorUtility.SaveFilePanel(
                    "选择输出路径", 
                    Path.GetDirectoryName(_outputPath), 
                    Path.GetFileName(_outputPath), 
                    "lua");
                
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // 转换为相对于项目的路径
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    
                    _outputPath = selectedPath;
                    EditorPrefs.SetString(OutputPathPrefKey, _outputPath);
                }
            }
            
            if (GUILayout.Button("恢复默认"))
            {
                _outputPath = DefaultOutputPath;
                EditorPrefs.SetString(OutputPathPrefKey, _outputPath);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 预览信息
            EditorGUILayout.LabelField("预览信息", EditorStyles.boldLabel);
            
            // 查找所有PlayerInitConfig文件
            string[] guids = AssetDatabase.FindAssets("t:PlayerInitConfig");
            EditorGUILayout.LabelField($"找到配置文件数量: {guids.Length}");
            
            if (guids.Length > 0)
            {
                EditorGUILayout.LabelField("配置文件列表:");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var config = AssetDatabase.LoadAssetAtPath<PlayerInitConfig>(path);
                    if (config != null)
                    {
                        EditorGUILayout.LabelField($"  • {config.name} ({path})");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未找到任何PlayerInitConfig配置文件。请先创建配置文件。", MessageType.Warning);
            }
            
            EditorGUILayout.Space();
            
            // 导出按钮
            EditorGUI.BeginDisabledGroup(guids.Length == 0);
            if (GUILayout.Button("导出到Lua", GUILayout.Height(40)))
            {
                ExportConfigs();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            // 快捷操作
            EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);
            if (GUILayout.Button("打开输出文件夹"))
            {
                string folderPath = Path.GetDirectoryName(_outputPath);
                if (Directory.Exists(folderPath))
                {
                    EditorUtility.RevealInFinder(folderPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", $"文件夹不存在: {folderPath}", "确定");
                }
            }
            
            if (GUILayout.Button("创建新的玩家初始化配置"))
            {
                // 打开资产创建菜单的替代方法
                Selection.activeObject = null;
                EditorUtility.DisplayDialog("提示", 
                    "请在Project窗口中右键 -> 工具 -> 玩家初始化配置 来创建新配置", "确定");
            }
        }

        private void ExportConfigs()
        {
            try
            {
                PlayerInitConfigExporter.ExportToPath(_outputPath);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("导出失败", $"导出过程中发生错误:\n{e.Message}", "确定");
                Debug.LogError($"导出PlayerInitConfig失败: {e}");
            }
        }
    }
}
#endif
