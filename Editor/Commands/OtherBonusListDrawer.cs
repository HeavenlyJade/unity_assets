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

        // 使用标准的 Foldout 展开/折叠
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), 
            property.isExpanded, 
            label, 
            true
        );

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++; // 标准缩进
            
            var itemsProperty = property.FindPropertyRelative("items");
            if (itemsProperty != null && itemsProperty.isArray)
            {
                // 绘制列表内容
                DrawList(position, itemsProperty);
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!_listsLoaded)
        {
            LoadOtherBonusTypes();
        }

        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        var itemsProperty = property.FindPropertyRelative("items");
        if (itemsProperty == null || !itemsProperty.isArray)
            return EditorGUIUtility.singleLineHeight;

        // 计算展开后的高度：标题 + 每个元素 + 添加按钮
        return (itemsProperty.arraySize + 2) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + 5;
    }

    private void DrawList(Rect position, SerializedProperty itemsProperty)
    {
        float currentY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        float indentWidth = EditorGUI.indentLevel * 15f; // 标准缩进宽度

        // 绘制现有元素
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            var element = itemsProperty.GetArrayElementAtIndex(i);
            var elementRect = new Rect(position.x + indentWidth, currentY, position.width - indentWidth - 25, singleLineHeight);
            
            // 绘制下拉框
            DrawOtherBonusPopup(elementRect, element, new GUIContent($"元素 {i + 1}"));
            
            // 绘制删除按钮（使用 "×" 符号）
            var deleteRect = new Rect(position.x + position.width - 20, currentY, 20, singleLineHeight);
            if (GUI.Button(deleteRect, "×"))
            {
                itemsProperty.DeleteArrayElementAtIndex(i);
                break;
            }
            
            currentY += singleLineHeight + verticalSpacing;
        }

        // 绘制添加按钮（使用 "+" 符号）
        var addRect = new Rect(position.x + indentWidth, currentY, position.width - indentWidth, singleLineHeight);
        if (GUI.Button(addRect, "+ 添加其他加成"))
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
        int newSelectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, _otherBonusTypes.ToArray());
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