using UnityEngine;
using UnityEditor;
using MiGame.Shop;
using MiGame.Pet;
using MiGame.Trail;
using System;
using System.IO;
using System.Collections.Generic;

namespace MiGame.Shop.Editor
{
    /// <summary>
    /// 商品奖励配置自定义编辑器
    /// </summary>
    [CustomPropertyDrawer(typeof(商品奖励配置))]
    public class 商品奖励配置Drawer : PropertyDrawer
    {
        private static List<string> playerVariables;
        private static List<string> playerStats;
        private static bool isDataLoaded = false;

        /// <summary>
        /// 加载VariableNames.json数据
        /// </summary>
        private void LoadVariableNames()
        {
            if (isDataLoaded) return;

            try
            {
                string jsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    var data = JsonUtility.FromJson<VariableNamesData>(jsonContent);
                    
                    playerVariables = new List<string>(data.VariableNames);
                    playerStats = new List<string>(data.StatNames);
                    isDataLoaded = true;
                }
                else
                {
                    Debug.LogWarning($"VariableNames.json文件不存在: {jsonPath}");
                    // 使用默认值
                    playerVariables = new List<string>();
                    playerStats = new List<string>();
                    isDataLoaded = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载VariableNames.json失败: {e.Message}");
                // 使用默认值
                playerVariables = new List<string>();
                playerStats = new List<string>();
                isDataLoaded = true;
            }
        }

        /// <summary>
        /// 创建玩家变量和玩家属性的下拉菜单
        /// </summary>
        private void CreateVariableMenu(GenericMenu menu, List<string> options, string currentValue, SerializedProperty property)
        {
            foreach (string option in options)
            {
                bool isSelected = currentValue == option;
                menu.AddItem(new GUIContent(option), isSelected, () => SetStringValueForStringProperty(property, option));
            }
        }

        /// <summary>
        /// 为字符串属性设置值
        /// </summary>
        private void SetStringValueForStringProperty(SerializedProperty property, string value)
        {
            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 加载变量名数据
            LoadVariableNames();

            // 计算布局 - 不使用PrefixLabel，直接使用完整位置
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 商品类型Prop = property.FindPropertyRelative("商品类型");
            var 商品名称Prop = property.FindPropertyRelative("商品名称");
            var 变量名称Prop = property.FindPropertyRelative("变量名称");
            var 数量Prop = property.FindPropertyRelative("数量");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = position.y;

            // 商品类型
            var 商品类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(商品类型Rect, 商品类型Prop, new GUIContent("商品类型"));
            currentY += lineHeight + spacing;

            // 根据商品类型显示对应的配置选择器
            商品类型 selectedType = (商品类型)商品类型Prop.enumValueIndex;
            
            if (selectedType == 商品类型.玩家变量 || selectedType == 商品类型.玩家属性)
            {
                // 参照BasePetConfig的逻辑，使用变量名称字符串字段
                var 变量名称Rect = new Rect(position.x, currentY, position.width, lineHeight);
                string labelText = GetConfigLabel(selectedType);
                
                // 创建下拉选择器
                var dropdownRect = new Rect(position.x, currentY, position.width, lineHeight);
                
                // 显示当前值或"请选择"
                string currentValue = 变量名称Prop.stringValue;
                string displayValue = string.IsNullOrEmpty(currentValue) ? "请选择" : currentValue;
                if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayValue), FocusType.Keyboard))
                {
                    var menu = new GenericMenu();
                    
                    if (selectedType == 商品类型.玩家变量)
                    {
                        // 动态创建玩家变量选项
                        CreateVariableMenu(menu, playerVariables, currentValue, 变量名称Prop);
                    }
                    else if (selectedType == 商品类型.玩家属性)
                    {
                        // 动态创建玩家属性选项
                        CreateVariableMenu(menu, playerStats, currentValue, 变量名称Prop);
                    }
                    
                    menu.ShowAsContext();
                }
                currentY += lineHeight + spacing;
                
                // 清空商品名称字段
                商品名称Prop.objectReferenceValue = null;
            }
            else
            {
                // 商品名称配置选择器
                var 商品名称Rect = new Rect(position.x, currentY, position.width, lineHeight);
                string labelText = GetConfigLabel(selectedType);
                System.Type configType = GetConfigType(selectedType);
                
                if (selectedType == 商品类型.物品)
                {
                    // 为物品类型提供ItemType选择器
                    var itemType = 商品名称Prop.objectReferenceValue as ScriptableObject;
                    商品名称Prop.objectReferenceValue = EditorGUI.ObjectField(商品名称Rect, new GUIContent(labelText), itemType, typeof(ScriptableObject), false);
                }
                else
                {
                    商品名称Prop.objectReferenceValue = EditorGUI.ObjectField(商品名称Rect, new GUIContent(labelText), 商品名称Prop.objectReferenceValue, configType, false);
                }
                currentY += lineHeight + spacing;
                
                // 清空变量名称字段
                变量名称Prop.stringValue = "";
            }

            // 数量字段（所有类型都显示）
            var 数量Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(数量Rect, 数量Prop, new GUIContent("数量"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private string GetConfigLabel(商品类型 type)
        {
            switch (type)
            {
                case 商品类型.物品: return "物品配置";
                case 商品类型.翅膀: return "翅膀配置";
                case 商品类型.宠物: return "宠物配置";
                case 商品类型.伙伴: return "伙伴配置";
                case 商品类型.尾迹: return "尾迹配置";
                case 商品类型.玩家变量: return "玩家变量名称";
                case 商品类型.玩家属性: return "玩家属性名称";
                default: return "商品名称";
            }
        }

        private System.Type GetConfigType(商品类型 type)
        {
            switch (type)
            {
                case 商品类型.物品: return typeof(ScriptableObject); // 使用ScriptableObject作为基类
                case 商品类型.翅膀: return typeof(WingConfig);
                case 商品类型.宠物: return typeof(PetConfig);
                case 商品类型.伙伴: return typeof(PartnerConfig);
                case 商品类型.尾迹: return typeof(BaseTrailConfig);
                case 商品类型.玩家变量: return null; // 字符串输入
                case 商品类型.玩家属性: return null; // 字符串输入
                default: return null;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            // 固定高度：商品类型 + 商品名称/变量名称 + 数量 + 四个间距
            return lineHeight * 3 + spacing * 4;
        }

        private void SetStringValue(SerializedProperty property, string value)
        {
            // 创建一个临时的ScriptableObject来存储字符串值
            var tempObj = ScriptableObject.CreateInstance<ScriptableObject>();
            tempObj.name = value;
            property.objectReferenceValue = tempObj;
        }
    }

    /// <summary>
    /// VariableNames.json数据结构
    /// </summary>
    [Serializable]
    public class VariableNamesData
    {
        public string[] VariableNames;
        public string[] StatNames;
    }
}
