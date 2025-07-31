using UnityEngine;
using UnityEditor;
using MiGame.Achievement;
using System.Collections.Generic;
using System.IO;

[CustomPropertyDrawer(typeof(UpgradeCondition))]
public class 升级条件Drawer : PropertyDrawer
{
    private List<string> _variableNames;
    private List<string> _statNames;
    private List<string> _itemNames;
    private bool _listsLoaded = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (!_listsLoaded)
        {
            LoadAllLists();
        }

        var 消耗类型Prop = property.FindPropertyRelative("消耗类型");
        var 消耗名称Prop = property.FindPropertyRelative("消耗名称");
        var 消耗数量Prop = property.FindPropertyRelative("消耗数量");

        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        Rect contentPosition = EditorGUI.IndentedRect(position);
        contentPosition.height = singleLineHeight;

        // 绘制消耗类型
        EditorGUI.PropertyField(contentPosition, 消耗类型Prop, new GUIContent("消耗类型"));
        contentPosition.y += singleLineHeight + verticalSpacing;

        升级消耗类型 type = (升级消耗类型)消耗类型Prop.enumValueIndex;
        List<string> currentList = null;

        switch (type)
        {
            case 升级消耗类型.物品:
                currentList = _itemNames;
                break;
            case 升级消耗类型.玩家变量:
                currentList = _variableNames;
                break;
            case 升级消耗类型.玩家属性:
                currentList = _statNames;
                break;
        }

        // 绘制消耗名称下拉列表
        DrawStringPopup(contentPosition, "消耗名称", 消耗名称Prop, currentList);
        contentPosition.y += singleLineHeight + verticalSpacing;

        // 绘制消耗数量
        EditorGUI.PropertyField(contentPosition, 消耗数量Prop, new GUIContent("消耗数量"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
    }

    private void DrawStringPopup(Rect rect, string label, SerializedProperty property, List<string> options)
    {
        if (options == null || options.Count == 0)
        {
            EditorGUI.LabelField(rect, label, "列表为空或未加载");
            property.stringValue = ""; // 清空，因为没有有效选项
            return;
        }

        int selectedIndex = options.IndexOf(property.stringValue);
        // 如果当前值不在列表中，将其设为列表的第一个，或保持为空
        if (selectedIndex < 0)
        {
            selectedIndex = 0; // 默认选择第一个
        }

        int newSelectedIndex = EditorGUI.Popup(rect, label, selectedIndex, options.ToArray());
        property.stringValue = options[newSelectedIndex];
    }
    
    private void LoadAllLists()
    {
        // 加载变量和属性
        string variablesJsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
        if (File.Exists(variablesJsonPath))
        {
            string json = File.ReadAllText(variablesJsonPath);
            var data = JsonUtility.FromJson<VariableData>(json);
            _variableNames = data?.VariableNames ?? new List<string>();
            _statNames = data?.StatNames ?? new List<string>();
        }
        else
        {
            _variableNames = new List<string>();
            _statNames = new List<string>();
        }

        // 加载物品
        string itemsJsonPath = "Assets/GameConf/物品/ItemNames.json";
        if (File.Exists(itemsJsonPath))
        {
            string json = File.ReadAllText(itemsJsonPath);
            var data = JsonUtility.FromJson<ItemNameListWrapper>(json);
            _itemNames = data?.ItemNames ?? new List<string>();
        }
        else
        {
            _itemNames = new List<string>();
        }

        _listsLoaded = true;
    }

    [System.Serializable] private class VariableData { public List<string> VariableNames = new List<string>(); public List<string> StatNames = new List<string>(); }
    [System.Serializable] private class ItemNameListWrapper { public List<string> ItemNames = new List<string>(); }
}
