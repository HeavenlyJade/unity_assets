using UnityEngine;
using UnityEditor;
using MiGame.Data;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(消耗项目))]
public class 消耗项目Drawer : PropertyDrawer
{
    private List<string> _variableNames;
    private List<string> _statNames;
    private List<string> _itemNames;
    private int _selectedIndex;
    private bool _listsLoaded = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 获取所有序列化属性
        var 消耗类型Prop = property.FindPropertyRelative("消耗类型");
        var 消耗名称Prop = property.FindPropertyRelative("消耗名称");
        var 数量分段Prop = property.FindPropertyRelative("数量分段");

        // 计算基础行高和间距
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        float currentY = position.y;

        // 1. 绘制消耗类型
        var typeRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(typeRect, 消耗类型Prop, new GUIContent("消耗类型"));
        currentY += singleLineHeight + verticalSpacing;
        
        消耗类型 type = (消耗类型)消耗类型Prop.enumValueIndex;
        
        // 加载所有清单
        if (!_listsLoaded)
        {
            LoadAllLists();
        }

        // 2. 根据消耗类型绘制不同内容
        if (type == 消耗类型.物品)
        {
            var valueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            DrawStringPopup(valueRect, "消耗名称", 消耗名称Prop, _itemNames);
            currentY += singleLineHeight + verticalSpacing;
        }
        else if (type == 消耗类型.玩家变量)
        {
            var valueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            DrawStringPopup(valueRect, "消耗名称", 消耗名称Prop, _variableNames);
            currentY += singleLineHeight + verticalSpacing;
        }
        else if (type == 消耗类型.玩家属性)
        {
            var valueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            DrawStringPopup(valueRect, "消耗名称", 消耗名称Prop, _statNames);
            currentY += singleLineHeight + verticalSpacing;
        }


        // 3. 绘制数量分段
        var amountRect = new Rect(position.x, currentY, position.width, EditorGUI.GetPropertyHeight(数量分段Prop, true));
        EditorGUI.PropertyField(amountRect, 数量分段Prop, new GUIContent("数量分段"), true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;
        var 消耗类型Prop = property.FindPropertyRelative("消耗类型");
        var 数量分段Prop = property.FindPropertyRelative("数量分段");

        // 消耗类型行
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        消耗类型 type = (消耗类型)消耗类型Prop.enumValueIndex;
        if (type == 消耗类型.物品 || type == 消耗类型.玩家变量 || type == 消耗类型.玩家属性)
        {
            // 消耗名称行
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        
        // 数量分段列表
        height += EditorGUI.GetPropertyHeight(数量分段Prop, true);
        
        return height;
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

        // 加载物品
        string itemsJsonPath = "Assets/GameConf/物品/ItemNames.json";
        if (System.IO.File.Exists(itemsJsonPath))
        {
            string json = System.IO.File.ReadAllText(itemsJsonPath);
            var data = JsonUtility.FromJson<ItemNameListWrapper>(json);
            _itemNames = data.ItemNames ?? new List<string>();
        }
        else
        {
            _itemNames = new List<string>();
        }
        
        _listsLoaded = true;
    }

    // 辅助数据结构
    [System.Serializable] private class VariableData { public List<string> VariableNames; public List<string> StatNames; }
    [System.Serializable] private class ItemNameListWrapper { public List<string> ItemNames; }
}