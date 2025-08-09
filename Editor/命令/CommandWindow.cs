using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiGame.Commands;
using MiGame.Core;

namespace MiGame.CommandSystem.Editor
{
    public class CommandWindow : EditorWindow
    {
        // --- Private Fields ---
        private Vector2 _leftPanelScroll;
        private Vector2 _rightPanelScroll;
        private Vector2 _historyScrollPosition;

        private static List<Type> _commandTypes = new List<Type>();
        private string _searchText = "";

        private static AbstractCommand _selectedCommandInstance;
        private static UnityEditor.Editor _commandEditor;

        private static List<CommandHistoryEntry> _commandHistory = new List<CommandHistoryEntry>();
        private const int MAX_HISTORY_COUNT = 20;

        // --- Nested Class for History ---
        [Serializable]
        private class CommandHistoryEntry
        {
            public string CommandString;
            public string CommandTypeName;
            public List<FieldValue> FieldValues;
        }

        [Serializable]
        private class FieldValue
        {
            public string FieldName;
            public object Value;
        }

        // --- Unity Methods ---
        [MenuItem("工具/命令配置窗口 V2")]
        public static void ShowWindow()
        {
            GetWindow<CommandWindow>("命令配置 V2");
        }

        private void OnEnable()
        {
            LoadCommandTypes();
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }

        private void OnGUI()
        {
            DrawSearchBar();
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
        }

        // --- UI Drawing ---
        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _searchText = EditorGUILayout.TextField(_searchText, GUI.skin.FindStyle("ToolbarSearchTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
            {
                _searchText = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(250), GUILayout.ExpandHeight(true));
            _leftPanelScroll = EditorGUILayout.BeginScrollView(_leftPanelScroll);

            EditorGUILayout.LabelField("可用命令", EditorStyles.boldLabel);
            
            var filteredCommands = _commandTypes
                .Where(t => {
                    var attr = t.GetCustomAttribute<CommandAttribute>();
                    return string.IsNullOrEmpty(_searchText) || 
                           t.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                           (attr != null && attr.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           (attr != null && attr.Description.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                })
                .OrderBy(t => t.Name);

            foreach (var cmdType in filteredCommands)
            {
                var attr = cmdType.GetCustomAttribute<CommandAttribute>();
                var buttonContent = new GUIContent($"{attr.Name}", $"{attr.Description}");
                if (GUILayout.Button(buttonContent, EditorStyles.toolbarButton))
                {
                    SelectCommand(cmdType);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _rightPanelScroll = EditorGUILayout.BeginScrollView(_rightPanelScroll);

            if (_selectedCommandInstance != null)
            {
                var commandAttr = _selectedCommandInstance.GetType().GetCustomAttribute<CommandAttribute>();
                
                EditorGUILayout.LabelField($"当前命令: {commandAttr?.Name}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"描述: {commandAttr?.Description}");
                
                EditorGUILayout.Space();
                
                if (_commandEditor != null)
                {
                    _commandEditor.OnInspectorGUI();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("复制命令到剪贴板", GUILayout.Height(30)))
                {
                    string commandString = GenerateCommandString(true);
                    EditorGUIUtility.systemCopyBuffer = commandString;
                    ShowNotification(new GUIContent("已复制到剪贴板!"));
                    Debug.Log("已复制: " + commandString);
                }
                
                DrawHistory();
            }
            else
            {
                EditorGUILayout.LabelField("请从左侧选择一个命令进行配置。");
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawHistory()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("历史指令", EditorStyles.boldLabel);
            _historyScrollPosition = EditorGUILayout.BeginScrollView(_historyScrollPosition, GUILayout.MinHeight(200));

            if(_commandHistory.Count == 0)
            {
                EditorGUILayout.LabelField("暂无历史记录");
            }

            foreach (var entry in _commandHistory.ToList())
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                if (GUILayout.Button("加载", GUILayout.Width(60)))
                {
                    LoadFromHistory(entry);
                }
                if (GUILayout.Button("复制", GUILayout.Width(60)))
                {
                    EditorGUIUtility.systemCopyBuffer = entry.CommandString;
                }

                EditorGUILayout.LabelField(entry.CommandString, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }

        // --- Core Logic ---
        private static void LoadCommandTypes()
        {
            _commandTypes = TypeCache.GetTypesDerivedFrom<AbstractCommand>()
                .Where(t => !t.IsAbstract && t.GetCustomAttribute<CommandAttribute>() != null)
                .ToList();
        }
        
        private void SelectCommand(Type commandType)
        {
            Cleanup();
            _selectedCommandInstance = (AbstractCommand)CreateInstance(commandType);
            _commandEditor = UnityEditor.Editor.CreateEditor(_selectedCommandInstance);
        }

        private string GenerateCommandString(bool addToHistory)
        {
            if (_selectedCommandInstance == null) return "";

            var commandType = _selectedCommandInstance.GetType();
            var commandAttr = commandType.GetCustomAttribute<CommandAttribute>();
            string commandName = commandAttr?.Name ?? "unknown";

            var defaultInstance = CreateInstance(commandType) as AbstractCommand;
            var fields = commandType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var fieldStrings = new List<string>();
            var historyFields = new List<FieldValue>();

            try
            {
                foreach (var field in fields)
                {
                    object currentValue = field.GetValue(_selectedCommandInstance);
                    
                    // For history, save all values
                    historyFields.Add(new FieldValue { FieldName = field.Name, Value = currentValue });
                    
                    // Conditional serialization logic
                    if (field.Name == "发件人ID")
                    {
                        var senderType = (MailCommand.SenderType)commandType.GetField("发件人").GetValue(_selectedCommandInstance);
                        if (senderType != MailCommand.SenderType.玩家) continue;
                    }
                    if (field.Name == "收件人")
                    {
                        var deliveryMethod = (MailCommand.DeliveryMethod)commandType.GetField("投递方式").GetValue(_selectedCommandInstance);
                        if (deliveryMethod != MailCommand.DeliveryMethod.个人) continue;
                    }

                    // For command string, only include non-default values, but always include enums
                    if (!field.FieldType.IsEnum)
                    {
                        object defaultValue = field.GetValue(defaultInstance);
                        if (currentValue == null || currentValue.Equals(defaultValue))
                        {
                            continue;
                        }
                        if (currentValue is string s && string.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        if(currentValue is IDictionary dict && dict.Count == 0)
                        {
                            continue;
                        }
                        if(currentValue is IList list && list.Count == 0)
                        {
                            continue;
                        }
                    }

                    string valueString;
                    if(currentValue is UnityEngine.Object obj && obj != null)
                    {
                        valueString = $"\"{obj.name}\"";
                    }
                    else
                    {
                        valueString = ConvertValueToString(currentValue, field.FieldType);
                    }

                    fieldStrings.Add($"\"{field.Name}\": {valueString}");
                }
            }
            finally
            {
                if (defaultInstance != null)
                {
                    DestroyImmediate(defaultInstance);
                }
            }

            string commandString = $"{commandName} {{ {string.Join(", ", fieldStrings)} }}";
            
            if (addToHistory)
            {
                var historyEntry = new CommandHistoryEntry
                {
                    CommandString = commandString,
                    CommandTypeName = commandType.AssemblyQualifiedName,
                    FieldValues = historyFields
                };
                
                _commandHistory.RemoveAll(h => h.CommandString == commandString);
                _commandHistory.Insert(0, historyEntry);

                if (_commandHistory.Count > MAX_HISTORY_COUNT)
                {
                    _commandHistory.RemoveAt(_commandHistory.Count - 1);
                }
            }

            return commandString;
        }

        private void LoadFromHistory(CommandHistoryEntry entry)
        {
            Type commandType = Type.GetType(entry.CommandTypeName);
            if (commandType == null)
            {
                Debug.LogError($"无法找到历史指令的类型: {entry.CommandTypeName}");
                return;
            }

            SelectCommand(commandType);
            
            foreach (var fieldValue in entry.FieldValues)
            {
                FieldInfo field = commandType.GetField(fieldValue.FieldName);
                if (field != null)
                {
                    // This is a simplified restoration. Complex types might need more logic.
                    field.SetValue(_selectedCommandInstance, fieldValue.Value);
                }
            }
        }

        private string ConvertValueToString(object value, Type type)
        {
            if (value == null || value.Equals(null)) return "null";
            if (type.IsEnum || type == typeof(string)) return $"\"{value}\"";
            if (type == typeof(bool)) return value.ToString().ToLower();
            if (type.IsPrimitive) return value.ToString();
            
            if (value is IDictionary iDict)
            {
                 var entries = new List<string>();
                 foreach (DictionaryEntry pair in iDict)
                 {
                    if (pair.Key == null) continue;
                    string keyString = ConvertObjectToString(pair.Key);
                    string valueString = ConvertValueToString(pair.Value, pair.Value?.GetType() ?? typeof(object));
                    entries.Add($"{keyString}: {valueString}");
                 }
                 return $"{{ {string.Join(", ", entries)} }}";
            }
            
            if (value is PlayerBonus bonus)
            {
                return $"{{ \"名称\": \"{bonus.Name}\", \"作用类型\": \"{bonus.Calculation}\", \"缩放倍率\": {bonus.缩放倍率} }}";
            }

            if (value is OtherBonusList otherBonusList)
            {
                var entries = new List<string>();
                foreach (var item in otherBonusList.items)
                {
                    entries.Add($"\"{item}\"");
                }
                return $"[ {string.Join(", ", entries)} ]";
            }

            if (value is IList iList)
            {
                var entries = new List<string>();
                foreach (var item in iList)
                {
                    entries.Add(ConvertValueToString(item, item?.GetType() ?? typeof(object)));
                }
                return $"[ {string.Join(", ", entries)} ]";
            }
            
            // For other complex types, try to serialize to JSON
            if (!type.IsPrimitive && type != typeof(string))
            {
                return JsonUtility.ToJson(value);
            }

            return $"\"{value}\"";
        }

        private string ConvertObjectToString(object obj)
        {
            if (obj is UnityEngine.Object unityObj && unityObj != null) return $"\"{unityObj.name}\"";
            if (obj != null) return $"\"{obj}\"";
            return "null";
        }

        private void Cleanup()
        {
            if (_commandEditor != null)
            {
                DestroyImmediate(_commandEditor);
                _commandEditor = null;
            }
            if (_selectedCommandInstance != null)
            {
                DestroyImmediate(_selectedCommandInstance);
                _selectedCommandInstance = null;
            }
        }
    }
} 