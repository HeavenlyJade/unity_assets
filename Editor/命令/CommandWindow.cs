using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiGame.Commands;

public class CommandWindow : EditorWindow
{
    private Vector2 _leftPanelScroll;
    private Vector2 _rightPanelScroll;
    private static readonly Dictionary<string, Type> CommandRegistry = new Dictionary<string, Type>();
    private static AbstractCommand _selectedCommandInstance;
    private static Editor _commandEditor;

    [MenuItem("Tools/命令配置窗口")]
    public static void ShowWindow()
    {
        GetWindow<CommandWindow>("命令配置");
    }

    private void OnEnable()
    {
        DiscoverCommands();
    }

    private static void DiscoverCommands()
    {
        CommandRegistry.Clear();
        var commandTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(AbstractCommand)) && !type.IsAbstract);

        foreach (var type in commandTypes)
        {
            var attribute = type.GetCustomAttribute<CommandAttribute>();
            if (attribute != null && !CommandRegistry.ContainsKey(attribute.commandName))
            {
                CommandRegistry.Add(attribute.commandName, type);
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        DrawLeftPanel();
        DrawRightPanel();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(200), GUILayout.ExpandHeight(true));
        _leftPanelScroll = EditorGUILayout.BeginScrollView(_leftPanelScroll);

        EditorGUILayout.LabelField("可用命令", EditorStyles.boldLabel);

        foreach (var cmd in CommandRegistry.OrderBy(c => c.Key))
        {
            if (GUILayout.Button(cmd.Key))
            {
                SelectCommand(cmd.Key);
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
            
            EditorGUILayout.LabelField("当前命令: " + commandAttr?.commandName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("描述: " + commandAttr?.description);
            
            EditorGUILayout.Space();
            
            if (_commandEditor != null)
            {
                // 使用自带的Inspector绘制UI
                _commandEditor.OnInspectorGUI();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("执行命令", GUILayout.Height(30)))
            {
                _selectedCommandInstance.Execute();
            }
        }
        else
        {
            EditorGUILayout.LabelField("请从左侧选择一个命令进行配置。");
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void SelectCommand(string commandName)
    {
        if (CommandRegistry.TryGetValue(commandName, out var commandType))
        {
            // 清理旧实例和Editor
            if (_selectedCommandInstance != null)
            {
                DestroyImmediate(_commandEditor);
                DestroyImmediate(_selectedCommandInstance);
            }

            _selectedCommandInstance = (AbstractCommand)CreateInstance(commandType);
            _commandEditor = Editor.CreateEditor(_selectedCommandInstance);
        }
    }

    private void OnDestroy()
    {
        // 清理，防止内存泄漏
        if (_selectedCommandInstance != null)
        {
            DestroyImmediate(_commandEditor);
            DestroyImmediate(_selectedCommandInstance);
        }
    }
} 