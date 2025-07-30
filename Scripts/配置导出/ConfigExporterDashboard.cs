using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiGame.Editor.Exporter
{
    public class ConfigExporterDashboard : EditorWindow
    {
        private const string OutputPathPrefKey = "ConfigExporter_OutputPath";
        private string _outputPath;
        private List<IConfigExporter> _exporters;
        private Vector2 _scrollPosition;

        [MenuItem("工具/配置导出/导出面板(新)")]
        public static void ShowWindow()
        {
            GetWindow<ConfigExporterDashboard>("配置导出面板");
        }

        private void OnEnable()
        {
            _outputPath = EditorPrefs.GetString(OutputPathPrefKey, "Assets/Lua/Config");
            RefreshExporters();
        }

        private void OnGUI()
        {
            GUILayout.Label("配置导出面板", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("此工具将扫描项目中所有独立的配置导出器，并在此处列出。", MessageType.Info);
            
            DrawOutputPath();
            DrawActionButtons();
            DrawExporterList();
        }

        private void DrawOutputPath()
        {
            EditorGUI.BeginChangeCheck();
            _outputPath = EditorGUILayout.TextField("Lua输出路径", _outputPath);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(OutputPathPrefKey, _outputPath);
            }
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("刷新导出器列表"))
            {
                RefreshExporters();
            }

            if (GUILayout.Button("导出全部", GUILayout.Height(30)))
            {
                ExportAll();
            }
            EditorGUILayout.Space();
        }

        private void DrawExporterList()
        {
            if (_exporters == null || _exporters.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到任何配置导出器。请确保您的导出器脚本已创建并继承自 IConfigExporter。", MessageType.Warning);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var exporter in _exporters)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                var configName = exporter.TargetType.Name;
                EditorGUILayout.LabelField(configName, GUILayout.Width(200));
                
                if (GUILayout.Button("导出", GUILayout.Width(100)))
                {
                    ExportSingle(exporter);
                }

                string assetPath = exporter.GetAssetPath();
                EditorGUILayout.LabelField(new GUIContent(assetPath, assetPath), EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void RefreshExporters()
        {
            _exporters = new List<IConfigExporter>();
            var exporterTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IConfigExporter).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (var type in exporterTypes)
            {
                try
                {
                    _exporters.Add((IConfigExporter)Activator.CreateInstance(type));
                }
                catch (Exception e)
                {
                    Debug.LogError($"无法实例化导出器 '{type.Name}': {e.Message}");
                }
            }
            _exporters = _exporters.OrderBy(e => e.TargetType.Name).ToList();
            Debug.Log($"刷新完成，找到 {_exporters.Count} 个配置导出器。");
        }

        private void ExportAll()
        {
            if (_exporters == null || _exporters.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有可导出的配置。", "确定");
                return;
            }

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var exporter in _exporters)
                {
                    ExportSingle(exporter, false); // 传入 false 避免重复刷新
                }
                EditorUtility.DisplayDialog("成功", "所有配置已成功导出!", "确定");
            }
            catch (Exception e)
            {
                Debug.LogError($"配置导出失败: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("失败", $"配置导出失败，请查看控制台获取详细信息。\n\n错误: {e.Message}", "确定");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                // 使用延迟调用来避免在资源导入期间刷新
                EditorApplication.delayCall += () => {
                    if (!AssetDatabase.IsAssetImportWorkerProcess())
                    {
                        AssetDatabase.Refresh();
                    }
                };
            }
        }

        private void ExportSingle(IConfigExporter exporter, bool refreshDb = true)
        {
            var absoluteOutputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", _outputPath));
            if (!Directory.Exists(absoluteOutputPath))
            {
                Directory.CreateDirectory(absoluteOutputPath);
            }

            Debug.Log($"--- 开始导出: {exporter.TargetType.Name} ---");
            exporter.Export(absoluteOutputPath);
            Debug.Log($"--- 完成导出: {exporter.TargetType.Name} ---");
            
            if (refreshDb)
            {
                // 使用延迟调用来避免在资源导入期间刷新
                EditorApplication.delayCall += () => {
                    if (!AssetDatabase.IsAssetImportWorkerProcess())
                    {
                        AssetDatabase.Refresh();
                    }
                };
            }
        }
    }
} 