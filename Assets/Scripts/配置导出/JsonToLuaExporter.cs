using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace MiGame.Editor.Exporter
{
    /// <summary>
    /// 直接将单个JSON文件导出为Lua表的专用导出器。
    /// </summary>
    public static class JsonToLuaExporter
    {
        // 在Unity菜单中添加一个手动触发的选项
        [MenuItem("工具/导出/导出玩家变量 (JSON to Lua)")]
        public static void ExportVariableNames()
        {
            string jsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
            
            // 从编辑器偏好设置中读取导出面板配置的路径
            string luaExportDir = EditorPrefs.GetString("ConfigExporter_OutputPath", "Assets/Lua/Config");
            string luaFileName = "VariableNameConfig.lua";

            // 如果用户取消了对话框（返回空字符串），则中止操作
            if (string.IsNullOrEmpty(luaExportDir))
            {
                Debug.LogError("Lua输出路径未设置，请在\"配置导出面板\"中指定路径。");
                return;
            }

            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSON文件未找到，无法导出: {jsonPath}");
                return;
            }

            // 读取JSON
            string jsonContent = File.ReadAllText(jsonPath);
            var data = JsonUtility.FromJson<VariableData>(jsonContent);

            if (data == null)
            {
                Debug.LogError($"解析JSON文件失败: {jsonPath}");
                return;
            }

            // 构建Lua文件内容
            var sb = new StringBuilder();
            sb.AppendLine("-- VariableNameConfig.lua");
            sb.AppendLine("-- Generated from VariableNames.json. Any custom code will be overwritten.");
            sb.AppendLine();
            sb.AppendLine("local VariableNameConfig = {Data = {}}");
            sb.AppendLine();

            // 导出 VariableNames
            sb.AppendLine("VariableNameConfig.VariableNames = {");
            AppendStringList(sb, data.VariableNames);
            sb.AppendLine("}");
            sb.AppendLine();

            // 导出 StatNames
            sb.AppendLine("VariableNameConfig.StatNames = {");
            AppendStringList(sb, data.StatNames);
            sb.AppendLine("}");
            sb.AppendLine();

            // 导出 PlayerAttributeNames
            sb.AppendLine("VariableNameConfig.PlayerAttributeNames = {");
            AppendStringList(sb, data.PlayerAttributeNames);
            sb.AppendLine("}");
            sb.AppendLine();

            // 导出 DependencyRules
            if (data.DependencyRules != null && data.DependencyRules.Count > 0)
            {
                sb.AppendLine("VariableNameConfig.DependencyRules = {");
                foreach (var kvp in data.DependencyRules)
                {
                    var rule = kvp.Value;
                    sb.AppendLine($"    ['{kvp.Key}'] = {{");
                    sb.AppendLine($"        目标变量 = '{rule.target}',");
                    sb.AppendLine($"        条件 = {GetConditionString(rule.condition)},");
                    sb.AppendLine($"        动作 = {GetActionString(rule.action)},");
                    sb.AppendLine($"        固定值 = {rule.value},");
                    sb.AppendLine($"        倍率 = {rule.multiplier}");
                    sb.AppendLine("    },");
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }

            sb.AppendLine("return VariableNameConfig");

            // 确保导出目录存在
            // 注意：这里的路径可能是项目外的绝对路径，所以需要特殊处理
            string absoluteOutputPath = Path.GetFullPath(luaExportDir);
            if (!Directory.Exists(absoluteOutputPath))
            {
                Directory.CreateDirectory(absoluteOutputPath);
            }

            // 写入文件
            string outputPath = Path.Combine(absoluteOutputPath, luaFileName);
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);

            // 只在路径是项目内时才刷新
            if (outputPath.StartsWith(Application.dataPath))
            {
                // 使用延迟调用来避免在资源导入期间刷新
                EditorApplication.delayCall += () => {
                    if (!AssetDatabase.IsAssetImportWorkerProcess())
                    {
                        AssetDatabase.Refresh();
                    }
                };
            }
            
            Debug.Log($"成功将 {jsonPath} 导出到 {outputPath}");
        }

        /// <summary>
        /// 一个辅助类，用于匹配JSON的结构。
        /// </summary>
        [System.Serializable]
        private class VariableData
        {
            public List<string> VariableNames = new List<string>();
            public List<string> StatNames = new List<string>();
            public List<string> PlayerAttributeNames = new List<string>();
            public Dictionary<string, DependencyRule> DependencyRules = new Dictionary<string, DependencyRule>();
        }

        /// <summary>
        /// 依赖规则类
        /// </summary>
        [System.Serializable]
        private class DependencyRule
        {
            public string key;
            public string target;
            public int condition;
            public int action;
            public float value;
            public float multiplier;
        }

        /// <summary>
        /// 将字符串列表追加到StringBuilder中，格式化为Lua表。
        /// </summary>
        private static void AppendStringList(StringBuilder sb, List<string> list)
        {
            if (list == null) return;
            foreach (var item in list)
            {
                // 将字符串转换为安全的Lua字符串字面量
                sb.AppendLine($"    '{item.Replace("'", "\\'").Replace("\\", "\\\\")}',");
            }
        }

        /// <summary>
        /// 获取条件的中文字符串表示
        /// </summary>
        private static string GetConditionString(int condition)
        {
            switch (condition)
            {
                case 0: return "'大于'";
                case 1: return "'大于等于'";
                case 2: return "'变化时'";
                default: return $"'{condition}'";
            }
        }

        /// <summary>
        /// 获取动作的中文字符串表示
        /// </summary>
        private static string GetActionString(int action)
        {
            switch (action)
            {
                case 0: return "'设置为源值'";
                case 1: return "'设置为固定值'";
                case 2: return "'设置为倍数值'";
                default: return $"'{action}'";
            }
        }
    }
}