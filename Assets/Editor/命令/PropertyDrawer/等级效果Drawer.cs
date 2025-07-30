using UnityEngine;
using UnityEditor;
using MiGame.Achievement;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(LevelEffect))]
public class 等级效果Drawer : PropertyDrawer
{
    private List<string> _variableNames;
    private List<string> _statNames;
    private int _selectedIndex;
    private bool _listsLoaded = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 获取所有序列化属性
        var 效果类型Prop = property.FindPropertyRelative("效果类型");
        var 效果字段名称Prop = property.FindPropertyRelative("效果字段名称");
        var 基础数值Prop = property.FindPropertyRelative("基础数值");
        var 效果数值Prop = property.FindPropertyRelative("效果数值");
        var 效果描述Prop = property.FindPropertyRelative("效果描述");

        // 计算基础行高和间距
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        float currentY = position.y;

        // 1. 绘制效果类型
        var typeRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(typeRect, 效果类型Prop, new GUIContent("效果类型"));
        currentY += singleLineHeight + verticalSpacing;
        
        PlayerVariableType type = (PlayerVariableType)效果类型Prop.enumValueIndex;
        
        // 加载所有清单
        if (!_listsLoaded)
        {
            LoadAllLists();
        }

        // 2. 根据效果类型绘制效果字段名称
        List<string> nameList = (type == PlayerVariableType.玩家变量) ? _variableNames : _statNames;
        var fieldNameRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        DrawStringPopup(fieldNameRect, "效果字段名称", 效果字段名称Prop, nameList);
        currentY += singleLineHeight + verticalSpacing;

        // 3. 绘制基础数值
        var baseValueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(baseValueRect, 基础数值Prop, new GUIContent("基础数值"));
        currentY += singleLineHeight + verticalSpacing;

        // 4. 绘制效果数值
        var effectValueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(effectValueRect, 效果数值Prop, new GUIContent("效果数值"));
        currentY += singleLineHeight + verticalSpacing;

        // 5. 绘制效果描述
        var descriptionRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(descriptionRect, 效果描述Prop, new GUIContent("效果描述"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 效果类型行 + 效果字段名称行 + 基础数值行 + 效果数值行 + 效果描述行
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 5;
    }

    private void DrawStringPopup(Rect rect, string label, SerializedProperty property, List<string> options)
    {
        if (options == null || options.Count == 0)
        {
            EditorGUI.LabelField(rect, label, "列表为空或未加载");
            return;
        }

        _selectedIndex = options.IndexOf(property.stringValue);
        if (_selectedIndex < 0) _selectedIndex = 0;

        _selectedIndex = EditorGUI.Popup(rect, label, _selectedIndex, options.ToArray());
        property.stringValue = options[_selectedIndex];
    }

    private void LoadAllLists()
    {
        // 加载变量
        string variablesJsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
        if (System.IO.File.Exists(variablesJsonPath))
        {
            string json = System.IO.File.ReadAllText(variablesJsonPath);
            var data = JsonUtility.FromJson<VariableData>(json);
            _variableNames = data.VariableNames ?? new List<string>();
            _statNames = data.StatNames ?? new List<string>();
        }
        else
        {
            _variableNames = new List<string>();
            _statNames = new List<string>();
        }
        
        _listsLoaded = true;
    }

    // 辅助数据结构
    [System.Serializable] private class VariableData { public List<string> VariableNames; public List<string> StatNames; }
} 