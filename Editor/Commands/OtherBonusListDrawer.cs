using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MiGame.Commands;

[CustomPropertyDrawer(typeof(OtherBonusList))]
public class OtherBonusListDrawer : PropertyDrawer
{
    private static List<string> _otherBonusTypes;
    private static bool _listsLoaded = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 加载配置数据
        if (!_listsLoaded)
        {
            LoadOtherBonusTypes();
        }

        if (_otherBonusTypes == null || _otherBonusTypes.Count == 0)
        {
            EditorGUI.LabelField(position, label, "列表为空或未加载");
            return;
        }

        // 绘制列表标题
        EditorGUI.LabelField(position, label);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        position.height -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // 绘制列表内容
        DrawList(position, property);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!_listsLoaded)
        {
            LoadOtherBonusTypes();
        }

        float height = EditorGUIUtility.singleLineHeight; // 标题行
        height += EditorGUIUtility.standardVerticalSpacing;

        var itemsProperty = property.FindPropertyRelative("items");
        if (itemsProperty != null && itemsProperty.isArray)
        {
            // 每个元素的高度
            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            
            // 添加按钮的高度
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

    private void DrawList(Rect position, SerializedProperty property)
    {
        var itemsProperty = property.FindPropertyRelative("items");
        if (itemsProperty == null || !itemsProperty.isArray) return;

        float currentY = position.y;
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

        // 绘制现有元素
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            var element = itemsProperty.GetArrayElementAtIndex(i);
            var elementRect = new Rect(position.x, currentY, position.width - 60, singleLineHeight);
            
            // 绘制下拉框
            DrawOtherBonusPopup(elementRect, element, new GUIContent($"元素 {i + 1}"));
            
            // 绘制删除按钮
            var deleteRect = new Rect(position.x + position.width - 50, currentY, 50, singleLineHeight);
            if (GUI.Button(deleteRect, "删除"))
            {
                itemsProperty.DeleteArrayElementAtIndex(i);
                break;
            }
            
            currentY += singleLineHeight + verticalSpacing;
        }

        // 绘制添加按钮
        var addRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        if (GUI.Button(addRect, "添加其他加成"))
        {
            itemsProperty.arraySize++;
            var newElement = itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1);
            if (_otherBonusTypes != null && _otherBonusTypes.Count > 0)
            {
                newElement.stringValue = _otherBonusTypes[0];
            }
        }
    }

    private void DrawOtherBonusPopup(Rect position, SerializedProperty property, GUIContent label)
    {
        // 找到当前值在列表中的索引
        int selectedIndex = _otherBonusTypes.IndexOf(property.stringValue);
        if (selectedIndex < 0)
        {
            selectedIndex = 0; // 默认选择第一个
        }

        // 绘制下拉框
        int newSelectedIndex = EditorGUI.Popup(position, label, selectedIndex, _otherBonusTypes.ToArray());
        property.stringValue = _otherBonusTypes[newSelectedIndex];
    }

    private void LoadOtherBonusTypes()
    {
        string jsonPath = "Assets/Scripts/公共配置/OtherBonusTypes.json";
        if (System.IO.File.Exists(jsonPath))
        {
            string json = System.IO.File.ReadAllText(jsonPath);
            var data = JsonUtility.FromJson<OtherBonusTypesData>(json);
            _otherBonusTypes = new List<string>();
            
            if (data.OtherBonusTypes != null)
            {
                foreach (var bonusType in data.OtherBonusTypes)
                {
                    _otherBonusTypes.Add(bonusType.name);
                }
            }
        }
        else
        {
            _otherBonusTypes = new List<string>();
        }
        
        _listsLoaded = true;
    }

    // 辅助数据结构
    [System.Serializable] 
    private class OtherBonusType
    {
        public string id;
        public string name;
        public string description;
    }

    [System.Serializable] 
    private class OtherBonusTypesData
    {
        public OtherBonusType[] OtherBonusTypes;
    }
} 