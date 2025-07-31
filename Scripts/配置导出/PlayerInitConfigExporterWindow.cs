#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace MiGame.Data
{
    public class PlayerInitConfigExporterWindow : EditorWindow
    {
        private const string EXPORT_PATH_KEY = "PlayerInitConfigExporter_ExportPath";
        private static string _exportPath;

        [MenuItem("工具/配置导出/玩家初始化配置面板")]
        public static void ShowWindow()
        {
            GetWindow<PlayerInitConfigExporterWindow>("玩家初始化配置导出");
        }

        private void OnEnable()
        {
            // 加载保存的路径，如果没有则使用默认值
            _exportPath = EditorPrefs.GetString(EXPORT_PATH_KEY, "Assets/Lua/MiGame/Data/PlayerInitConfig.lua");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("配置导出面板", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("此工具将导出项目中所有“玩家初始化配置”文件，并生成对应的Lua配置文件。", MessageType.Info);
            
            EditorGUILayout.Space();

            // 路径设置
            EditorGUILayout.BeginHorizontal();
            _exportPath = EditorGUILayout.TextField("Lua输出路径", _exportPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string directory = Path.GetDirectoryName(_exportPath);
                string fileName = Path.GetFileName(_exportPath);
                string path = EditorUtility.SaveFilePanel("选择Lua输出路径", directory, fileName, "lua");
                if (!string.IsNullOrEmpty(path))
                {
                    // 将绝对路径转换为相对Assets目录的路径以便保存和显示
                    if (path.StartsWith(Application.dataPath))
                    {
                        _exportPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        // 如果路径在Assets目录外，则显示完整路径
                        _exportPath = path;
                        Debug.LogWarning("选择了Assets目录之外的路径，这可能导致在不同设备上路径不一致。");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // 保存路径更改
            if (GUI.changed)
            {
                EditorPrefs.SetString(EXPORT_PATH_KEY, _exportPath);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("导出配置", GUILayout.Height(40)))
            {
                if (string.IsNullOrEmpty(_exportPath))
                {
                    EditorUtility.DisplayDialog("错误", "输出路径不能为空！", "确定");
                    return;
                }

                string finalPath = _exportPath;
                // 检查路径是否指向一个已存在的目录
                if (Directory.Exists(finalPath))
                {
                    // 如果是目录，则自动拼接默认文件名
                    finalPath = Path.Combine(finalPath, "PlayerInitConfig.lua");
                }
                // (可选) 如果路径没有扩展名，也拼接默认文件名
                else if (string.IsNullOrEmpty(Path.GetExtension(finalPath)))
                {
                     finalPath = Path.Combine(finalPath, "PlayerInitConfig.lua");
                }

                // 调用核心导出逻辑
                PlayerInitConfigExporter.ExportToPath(finalPath);
            }
        }
    }
}
#endif
