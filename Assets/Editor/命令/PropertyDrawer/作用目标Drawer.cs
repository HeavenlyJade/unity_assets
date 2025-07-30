using UnityEngine;
using UnityEditor;
using MiGame.Data;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(作用目标))]
public class 作用目标Drawer : PropertyDrawer
{
    private List<string> _variableNames;
    private List<string> _statNames;
    private int _selectedIndex;
    private bool _listsLoaded = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 获取所有序列化属性
        var 目标类型Prop = property.FindPropertyRelative("目标类型");
        var 目标名称Prop = property.FindPropertyRelative("目标名称");
        var 作用数值Prop = property.FindPropertyRelative("作用数值");

        // 计算基础行高和间距
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        float currentY = position.y;

        // 1. 绘制目标类型
        var typeRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(typeRect, 目标类型Prop, new GUIContent("目标类型"));
        currentY += singleLineHeight + verticalSpacing;
        
        目标类型 type = (目标类型)目标类型Prop.enumValueIndex;
        
        // 加载所有清单
        if (!_listsLoaded)
        {
            LoadAllLists();
        }

        // 2. 根据目标类型绘制目标名称
        List<string> nameList = (type == 目标类型.玩家变量) ? _variableNames : _statNames;
        var valueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        DrawStringPopup(valueRect, "目标名称", 目标名称Prop, nameList);
        currentY += singleLineHeight + verticalSpacing;

        // 3. 绘制作用数值
        var effectValueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(effectValueRect, 作用数值Prop, new GUIContent("作用数值"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 目标类型行 + 目标名称行 + 作用数值行
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
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