using UnityEditor;
using UnityEngine;
using MiGame.Commands;

#if UNITY_EDITOR
namespace MiGame.Commands.Editor
{
    [CustomPropertyDrawer(typeof(ItemTypeIntDictionary))]
    public class ItemTypeIntDictionaryDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            var keysProp = property.FindPropertyRelative("keys");
            if (keysProp == null)
                return EditorGUIUtility.singleLineHeight * 2;
                
            return (keysProp.arraySize + 3) * (EditorGUIUtility.singleLineHeight + 2) + 10;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            try
            {
                // 展开/折叠
                Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

                if (property.isExpanded)
                {
                    var keysProp = property.FindPropertyRelative("keys");
                    var valuesProp = property.FindPropertyRelative("values");

                    if (keysProp == null || valuesProp == null)
                    {
                        EditorGUI.LabelField(position, "Error: Cannot find keys or values property");
                        return;
                    }

                    // 确保两个数组长度一致
                    if (keysProp.arraySize != valuesProp.arraySize)
                    {
                        valuesProp.arraySize = keysProp.arraySize;
                    }

                    EditorGUI.indentLevel++;
                    float currentY = position.y + EditorGUIUtility.singleLineHeight + 2;

                    // 显示数量信息
                    if (keysProp.arraySize > 0)
                    {
                        Rect countRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
                        EditorGUI.LabelField(countRect, $"物品数量: {keysProp.arraySize}", EditorStyles.miniLabel);
                        currentY += EditorGUIUtility.singleLineHeight + 2;
                    }

                    // 绘制现有条目
                    for (int i = 0; i < keysProp.arraySize; i++)
                    {
                        Rect lineRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
                        DrawDictionaryElement(lineRect, keysProp, valuesProp, i, property);
                        currentY += EditorGUIUtility.singleLineHeight + 2;
                    }

                    // 添加按钮
                    Rect addButtonRect = new Rect(position.x, currentY, 80, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(addButtonRect, "添加物品"))
                    {
                        AddNewElement(keysProp, valuesProp, property);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            catch (System.Exception e)
            {
                EditorGUI.LabelField(position, $"Error: {e.Message}");
                Debug.LogError($"ItemTypeIntDictionaryDrawer Error: {e}");
            }

            EditorGUI.EndProperty();
        }

        private void AddNewElement(SerializedProperty keysProp, SerializedProperty valuesProp, SerializedProperty parentProperty)
        {
            try
            {
                keysProp.arraySize++;
                valuesProp.arraySize++;
                
                // 设置默认值
                if (keysProp.arraySize > 0)
                {
                    var newKeyProp = keysProp.GetArrayElementAtIndex(keysProp.arraySize - 1);
                    var newValueProp = valuesProp.GetArrayElementAtIndex(valuesProp.arraySize - 1);
                    
                    newKeyProp.objectReferenceValue = null;
                    newValueProp.intValue = 1;
                }
                
                parentProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(parentProperty.serializedObject.targetObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error adding new element: {e}");
            }
        }

        private void DrawDictionaryElement(Rect position, SerializedProperty keysProp, SerializedProperty valuesProp, int index, SerializedProperty parentProperty)
        {
            if (index >= keysProp.arraySize || index >= valuesProp.arraySize)
                return;

            try
            {
                var keyProp = keysProp.GetArrayElementAtIndex(index);
                var valueProp = valuesProp.GetArrayElementAtIndex(index);

                float keyWidth = position.width * 0.6f;
                float valueWidth = position.width * 0.25f;
                float buttonWidth = 25f;
                float spacing = 2f;

                // 物品选择器
                Rect keyRect = new Rect(position.x, position.y, keyWidth - spacing, position.height);
                EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);

                // 数量输入
                Rect valueRect = new Rect(position.x + keyWidth, position.y, valueWidth - spacing, position.height);
                EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

                // 删除按钮
                Rect deleteRect = new Rect(position.x + keyWidth + valueWidth, position.y, buttonWidth, position.height);
                if (GUI.Button(deleteRect, "×"))
                {
                    DeleteElement(keysProp, valuesProp, index, parentProperty);
                }
            }
            catch (System.Exception e)
            {
                EditorGUI.LabelField(position, $"Error drawing element {index}: {e.Message}");
            }
        }

        private void DeleteElement(SerializedProperty keysProp, SerializedProperty valuesProp, int index, SerializedProperty parentProperty)
        {
            try
            {
                if (index < 0 || index >= keysProp.arraySize)
                    return;

                // 对于对象引用，需要先设置为null再删除
                var keyProp = keysProp.GetArrayElementAtIndex(index);
                if (keyProp.propertyType == SerializedPropertyType.ObjectReference && keyProp.objectReferenceValue != null)
                {
                    keyProp.objectReferenceValue = null;
                }
                
                keysProp.DeleteArrayElementAtIndex(index);
                if (index < valuesProp.arraySize)
                {
                    valuesProp.DeleteArrayElementAtIndex(index);
                }
                
                parentProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(parentProperty.serializedObject.targetObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error deleting element: {e}");
            }
        }
    }
}
#endif