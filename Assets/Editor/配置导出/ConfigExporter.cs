using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace MiGame.Editor.Tool
{
    /// <summary>
    /// 一个自定义的JsonConverter，用于将任何继承自ScriptableObject的对象序列化为其名称。
    /// 这个转换器由下面的ContractResolver动态应用到目标属性上。
    /// </summary>
    public class UnityObjectNameConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is UnityEngine.Object uo)
            {
                writer.WriteValue(uo.name);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // 导出功能不需要反序列化，所以此处不实现
            throw new NotImplementedException("This converter is for writing only.");
        }
    }

    /// <summary>
    /// 自定义契约解析器。它的作用是在序列化一个对象时，检查其所有属性，
    /// 如果发现某个属性的类型是ScriptableObject，就告诉Json.NET使用
    /// 'ScriptableObjectNameConverter'来序列化这个属性。
    /// </summary>
    public class ReferenceConverterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // 规则1：忽略所有继承自 UnityEngine.Object 的 'name' 和 'hideFlags' 属性
            if (property.DeclaringType == typeof(UnityEngine.Object) && (property.PropertyName == "name" || property.PropertyName == "hideFlags"))
            {
                property.ShouldSerialize = instance => false;
            }

            // 规则2：如果属性的类型是UnityEngine.Object（或其子类），就应用我们的引用-转-名称转换器
            else if (typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType))
            {
                // 不要对自身类型应用，只对子属性应用
                if (property.DeclaringType != property.PropertyType)
                {
                    property.Converter = new UnityObjectNameConverter();
                }
            }

            return property;
        }
    }

    /// <summary>
    /// 自定义Json转换器，用于将Unity的Vector3类型序列化为数组 [x, y, z]
    /// </summary>
    public class CustomVector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.x, 3));
            writer.WriteValue(Math.Round(value.y, 3));
            writer.WriteValue(Math.Round(value.z, 3));
            writer.WriteEndArray();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var arr = JArray.Load(reader);
            return new Vector3(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>());
        }
    }

    /// <summary>
    /// 自定义Json转换器，用于将Unity的Color类型序列化为数组 [r, g, b, a]
    /// </summary>
    public class CustomColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.r, 3));
            writer.WriteValue(Math.Round(value.g, 3));
            writer.WriteValue(Math.Round(value.b, 3));
            writer.WriteValue(Math.Round(value.a, 3));
            writer.WriteEndArray();
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var arr = JArray.Load(reader);
            return new Color(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>(), arr[3].Value<float>());
        }
    }

    /// <summary>
    /// 配置导出工具的编辑器窗口
    /// </summary>
    public class ConfigExporterWindow : EditorWindow
    {
        private const string ConfigsSourceFolder = "Assets/GameConf";
        private const string BeginMarker = "--- AUTO GENERATED CONFIG BEGIN ---";
        private const string EndMarker = "--- AUTO GENERATED CONFIG END ---";
        private const string OutputPathPrefKey = "ConfigExporter_OutputPath";

        private string _outputPath;

        [MenuItem("工具/配置导出/导出到Lua")]
        public static void ShowWindow()
        {
            GetWindow<ConfigExporterWindow>("配置导出到Lua");
        }

        private void OnEnable()
        {
            // 当窗口启用时，加载上次保存的路径，如果找不到则使用默认值
            _outputPath = EditorPrefs.GetString(OutputPathPrefKey, "Assets/../Lua/Config");
        }

        private void OnGUI()
        {
            GUILayout.Label("配置导出流水线", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("此工具将扫描 " + ConfigsSourceFolder + " 目录下的所有ScriptableObject资源，并将其转换为Lua配置文件。", MessageType.Info);
            
            EditorGUI.BeginChangeCheck();
            _outputPath = EditorGUILayout.TextField("Lua输出路径", _outputPath);
            if (EditorGUI.EndChangeCheck())
            {
                // 当用户修改路径后，立即保存
                EditorPrefs.SetString(OutputPathPrefKey, _outputPath);
            }

            if (GUILayout.Button("开始导出"))
            {
                try
                {
                    ExportAllConfigs();
                    EditorUtility.DisplayDialog("成功", "所有配置已成功导出!", "确定");
                }
                catch (Exception e)
                {
                    Debug.LogError($"配置导出失败: {e.Message}\n{e.StackTrace}");
                    EditorUtility.DisplayDialog("失败", $"配置导出失败，请查看控制台获取详细信息。\n\n错误: {e.Message}", "确定");
                }
            }
        }

        /// <summary>
        /// 执行导出的核心逻辑
        /// </summary>
        private void ExportAllConfigs()
        {
            if (!Directory.Exists(ConfigsSourceFolder))
            {
                throw new DirectoryNotFoundException($"配置源文件夹未找到: {ConfigsSourceFolder}");
            }

            var absoluteOutputPath = Path.GetFullPath(Path.Combine(Application.dataPath, Path.GetDirectoryName(_outputPath), Path.GetFileName(_outputPath)));
            if (!Directory.Exists(absoluteOutputPath))
            {
                Directory.CreateDirectory(absoluteOutputPath);
                Debug.Log($"创建输出目录: {absoluteOutputPath}");
            }

            var allAssetPaths = AssetDatabase.FindAssets("t:ScriptableObject", new[] { ConfigsSourceFolder });
            if (allAssetPaths.Length == 0)
            {
                Debug.LogWarning($"在 '{ConfigsSourceFolder}' 中没有找到任何 ScriptableObject 资源。");
                return;
            }

            var groupedAssets = GroupAssetsByType(allAssetPaths);

            var jsonSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { 
                    new Newtonsoft.Json.Converters.StringEnumConverter(), 
                    new CustomVector3Converter(), 
                    new CustomColorConverter() 
                },
                ContractResolver = new ReferenceConverterContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                Error = delegate(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                {
                    // 当遇到无法序列化的属性时（例如某些Unity内置类型的特殊属性），
                    //在此处记录一个警告，并将错误标记为已处理，以便序列化过程可以继续。
                    Debug.LogWarning($"JSON 序列化跳过属性: {args.ErrorContext.Path} - {args.ErrorContext.Error.Message}");
                    args.ErrorContext.Handled = true;
                }
            };

            foreach (var entry in groupedAssets)
            {
                var configType = entry.Key;
                var assets = entry.Value;
                
                var jsonString = JsonConvert.SerializeObject(assets, jsonSettings);
                var luaString = JsonToLua(jsonString);

                var outputFileName = $"{configType.Name}Config.lua";
                var outputFilePath = Path.Combine(absoluteOutputPath, outputFileName);
                
                WriteToLuaTemplate(outputFilePath, luaString, configType);
            }

            AssetDatabase.Refresh();
            Debug.Log($"配置导出完成，共处理了 {groupedAssets.Count} 种配置类型。");
        }

        /// <summary>
        /// 将扫描到的资源按其C#类型进行分组
        /// </summary>
        private Dictionary<Type, Dictionary<string, ScriptableObject>> GroupAssetsByType(string[] guids)
        {
            var groupedAssets = new Dictionary<Type, Dictionary<string, ScriptableObject>>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset != null)
                {
                    var assetType = asset.GetType();
                    if (!groupedAssets.ContainsKey(assetType))
                    {
                        groupedAssets[assetType] = new Dictionary<string, ScriptableObject>();
                    }
                    // 使用资源文件名作为Key，确保唯一性
                    groupedAssets[assetType][asset.name] = asset;
                }
            }
            return groupedAssets;
        }

        /// <summary>
        /// 将JSON字符串转换为格式化的Lua Table字符串
        /// </summary>
        private string JsonToLua(string json)
        {
            var token = JToken.Parse(json);
            var sb = new StringBuilder();
            ConvertJTokenToLua(token, sb, 1);
            return sb.ToString();
        }
        
        /// <summary>
        /// 递归函数，用于将JToken转换为Lua字符串
        /// </summary>
        private void ConvertJTokenToLua(JToken token, StringBuilder sb, int indent)
        {
            string indentStr = new string(' ', indent * 4);

            if (token is JObject obj)
            {
                sb.AppendLine("{");
                foreach (var property in obj.Properties())
                {
                    sb.Append(indentStr);
                    sb.Append($"[\"{property.Name}\"] = ");
                    ConvertJTokenToLua(property.Value, sb, indent + 1);
                    sb.AppendLine(",");
                }
                sb.Append(new string(' ', (indent - 1) * 4));
                sb.Append("}");
            }
            else if (token is JArray array)
            {
                sb.AppendLine("{");
                foreach (var item in array)
                {
                    sb.Append(indentStr);
                    ConvertJTokenToLua(item, sb, indent + 1);
                    sb.AppendLine(",");
                }
                sb.Append(new string(' ', (indent - 1) * 4));
                sb.Append("}");
            }
            else if (token is JValue val)
            {
                switch (val.Type)
                {
                    case JTokenType.String:
                        sb.Append($"\"{val.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"")}\"");
                        break;
                    case JTokenType.Integer:
                    case JTokenType.Float:
                        sb.Append(val.ToString(CultureInfo.InvariantCulture));
                        break;
                    case JTokenType.Boolean:
                        sb.Append(val.ToObject<bool>() ? "true" : "false");
                        break;
                    case JTokenType.Null:
                        sb.Append("nil");
                        break;
                    default:
                        sb.Append($"\"{val.ToString()}\"");
                        break;
                }
            }
            else
            {
                sb.Append("nil");
            }
        }

        /// <summary>
        /// 将C#类型转换为对应的EmmyLua类型注解
        /// </summary>
        private string CSharpTypeToLuaType(Type type)
        {
            if (type == typeof(string) || typeof(ScriptableObject).IsAssignableFrom(type))
            {
                // ScriptableObjects are converted to their names (strings) during serialization
                return "string";
            }
            if (type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(long) || type.IsEnum)
            {
                return "number";
            }
            if (type == typeof(bool))
            {
                return "boolean";
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = type.GetGenericArguments()[0];
                return $"{CSharpTypeToLuaType(itemType)}[]";
            }
            if (type.IsArray)
            {
                var itemType = type.GetElementType();
                return $"{CSharpTypeToLuaType(itemType)}[]";
            }
            // Dictionaries, custom structs, Vector3 will be tables
            return "table"; 
        }

        /// <summary>
        /// 为给定的C#类型生成EmmyLua的---@field注解
        /// </summary>
        private string GenerateLuaFieldAnnotations(Type type)
        {
            var sb = new StringBuilder();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.IsDefined(typeof(NonSerializedAttribute), false))
                {
                    continue;
                }

                string fieldName = field.Name;
                string luaType = CSharpTypeToLuaType(field.FieldType);
                sb.AppendLine($"---@field {fieldName} {luaType}");
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// 将生成的Lua数据写入目标文件，仅当内容有变化时才覆盖。
        /// </summary>
        private void WriteToLuaTemplate(string filePath, string luaData, Type configType)
        {
            string configName = configType.Name;
            string luaTableName = $"{configName}Config";

            // 1. 完整生成新的文件内容
            var sb = new StringBuilder();
            sb.AppendLine($"-- {luaTableName}.lua");
            sb.AppendLine($"-- Generated by ConfigExporter. Any custom code will be overwritten.");
            sb.AppendLine();

            sb.AppendLine($"---@class {luaTableName}");
            sb.AppendLine($"---@field Data table<string, table>");
            sb.AppendLine();

            sb.AppendLine($"---@type {luaTableName}");
            sb.AppendLine($"local {luaTableName} = {{Data = {{}}}}");
            sb.AppendLine();
            
            sb.AppendLine($"-- {BeginMarker}");
            sb.AppendLine($"{luaTableName}.Data = {luaData}");
            sb.AppendLine($"-- {EndMarker}");
            sb.AppendLine();
            sb.AppendLine($"return {luaTableName}");

            string newContent = sb.ToString();

            // 2. 检查现有文件，仅在内容不同时才写入
            bool shouldWrite = true;
            if (File.Exists(filePath))
            {
                string existingContent = File.ReadAllText(filePath, Encoding.UTF8);
                if (string.Equals(existingContent, newContent, StringComparison.Ordinal))
                {
                    shouldWrite = false;
                }
            }

            if (shouldWrite)
            {
                File.WriteAllText(filePath, newContent, Encoding.UTF8);
            }
        }
    }
} 