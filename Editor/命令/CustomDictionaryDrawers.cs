using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using MiGame.Commands;

// 为 ItemTypeIntDictionary 创建一个专用的、简单的 PropertyDrawer
[CustomPropertyDrawer(typeof(ItemTypeIntDictionary))]
public class ItemTypeIntDictionaryDrawer : PropertyDrawer
{
    private Dictionary<string, ReorderableList> reorderableLists = new Dictionary<string, ReorderableList>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var list = GetOrCreateList(property, label);
        list.DoList(position);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetOrCreateList(property, label).GetHeight();
    }

    private ReorderableList GetOrCreateList(SerializedProperty property, GUIContent label)
    {
        string key = property.propertyPath;
        if (reorderableLists.ContainsKey(key))
            return reorderableLists[key];

        var keysProp = property.FindPropertyRelative("keys");
        var valuesProp = property.FindPropertyRelative("values");

        var list = new ReorderableList(property.serializedObject, keysProp, true, true, true, true);

        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, label);
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var keyElement = keysProp.GetArrayElementAtIndex(index);
            var valueElement = valuesProp.GetArrayElementAtIndex(index);
            
            rect.y += 2;
            float halfWidth = rect.width * 0.7f;
            float spacing = 5f;

            keyElement.objectReferenceValue = EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, halfWidth, EditorGUIUtility.singleLineHeight),
                keyElement.objectReferenceValue, typeof(ItemType), false);
            
            valueElement.intValue = EditorGUI.IntField(
                new Rect(rect.x + halfWidth + spacing, rect.y, rect.width - halfWidth - spacing, EditorGUIUtility.singleLineHeight),
                valueElement.intValue);
        };

        list.onAddCallback = (ReorderableList l) => {
            keysProp.arraySize++;
            valuesProp.arraySize++;
            // 将新添加的元素设为 null 和 0
            keysProp.GetArrayElementAtIndex(keysProp.arraySize - 1).objectReferenceValue = null;
            valuesProp.GetArrayElementAtIndex(valuesProp.arraySize - 1).intValue = 0;
        };

        reorderableLists[key] = list;
        return list;
    }
}


// 为 技能IntDictionary 创建一个专用的、简单的 PropertyDrawer
[CustomPropertyDrawer(typeof(SkillIntDictionary))]
public class SkillIntDictionaryDrawer : PropertyDrawer
{
    private Dictionary<string, ReorderableList> reorderableLists = new Dictionary<string, ReorderableList>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var list = GetOrCreateList(property, label);
        list.DoList(position);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetOrCreateList(property, label).GetHeight();
    }

    private ReorderableList GetOrCreateList(SerializedProperty property, GUIContent label)
    {
        string key = property.propertyPath;
        if (reorderableLists.ContainsKey(key))
            return reorderableLists[key];

        var keysProp = property.FindPropertyRelative("keys");
        var valuesProp = property.FindPropertyRelative("values");

        var list = new ReorderableList(property.serializedObject, keysProp, true, true, true, true);

        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, label);
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var keyElement = keysProp.GetArrayElementAtIndex(index);
            var valueElement = valuesProp.GetArrayElementAtIndex(index);
            
            rect.y += 2;
            float halfWidth = rect.width * 0.7f;
            float spacing = 5f;

            keyElement.objectReferenceValue = EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, halfWidth, EditorGUIUtility.singleLineHeight),
                keyElement.objectReferenceValue, typeof(技能), false);
            
            valueElement.intValue = EditorGUI.IntField(
                new Rect(rect.x + halfWidth + spacing, rect.y, rect.width - halfWidth - spacing, EditorGUIUtility.singleLineHeight),
                valueElement.intValue);
        };
        
        list.onAddCallback = (ReorderableList l) => {
            keysProp.arraySize++;
            valuesProp.arraySize++;
            keysProp.GetArrayElementAtIndex(keysProp.arraySize - 1).objectReferenceValue = null;
            valuesProp.GetArrayElementAtIndex(valuesProp.arraySize - 1).intValue = 0;
        };

        reorderableLists[key] = list;
        return list;
    }
} 