using UnityEngine;
using UnityEditor;
using MiGame.Achievement;
using System.Collections.Generic;
using MiGame.Pet;
using MiGame.Items;

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
        var 效果等级配置Prop = property.FindPropertyRelative("效果等级配置");
        var 效果描述Prop = property.FindPropertyRelative("效果描述");
        var 加成类型Prop = property.FindPropertyRelative("加成类型");
        var 物品目标Prop = property.FindPropertyRelative("物品目标");
        var 目标变量Prop = property.FindPropertyRelative("目标变量");

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
        if (type == PlayerVariableType.玩家变量)
        {
            // 当效果类型为玩家变量时，直接显示玩家变量列表
            var fieldNameRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            DrawStringPopup(fieldNameRect, "效果字段名称", 效果字段名称Prop, _variableNames);
            currentY += singleLineHeight + verticalSpacing;
        }
        else if (type == PlayerVariableType.玩家属性)
        {
            // 当效果类型为玩家属性时，显示玩家属性列表
            var fieldNameRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            DrawStringPopup(fieldNameRect, "效果字段名称", 效果字段名称Prop, _statNames);
            currentY += singleLineHeight + verticalSpacing;
        }

        // 3. 绘制基础数值
        var baseValueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(baseValueRect, 基础数值Prop, new GUIContent("基础数值"));
        currentY += singleLineHeight + verticalSpacing;

        // 4. 绘制效果数值
        var effectValueRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(effectValueRect, 效果数值Prop, new GUIContent("效果数值"));
        currentY += singleLineHeight + verticalSpacing;

        // 5. 绘制效果等级配置
        var effectLevelConfigRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(effectLevelConfigRect, 效果等级配置Prop, new GUIContent("效果等级配置"));
        currentY += singleLineHeight + verticalSpacing;

        // 6. 绘制加成类型
        var bonusTypeRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(bonusTypeRect, 加成类型Prop, new GUIContent("加成类型"));
        currentY += singleLineHeight + verticalSpacing;

        // 7. 当加成类型为 物品 时，绘制物品目标
        var currentBonusType = (加成类型)加成类型Prop.enumValueIndex;
        if (currentBonusType == 加成类型.物品)
        {
            var itemTargetRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(itemTargetRect, 物品目标Prop, new GUIContent("物品目标"));
            currentY += singleLineHeight + verticalSpacing;
        }

        // 8. 绘制目标变量（根据加成类型显示对应的变量列表）
        if (currentBonusType == 加成类型.玩家变量)
        {
            // 当加成类型为玩家变量时，显示玩家变量列表
            var targetRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            DrawStringPopup(targetRect, "目标变量", 目标变量Prop, _variableNames);
            currentY += singleLineHeight + verticalSpacing;
        }
        else if (currentBonusType == 加成类型.玩家属性)
        {
            // 当加成类型为玩家属性时，显示玩家属性列表
            var targetRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            DrawStringPopup(targetRect, "目标变量", 目标变量Prop, _statNames);
            currentY += singleLineHeight + verticalSpacing;
        }
        else if (currentBonusType == 加成类型.物品)
        {
            // 当加成类型为物品时，显示物品类型列表
            var targetRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            DrawItemTypePopup(targetRect, "目标变量", 目标变量Prop);
            currentY += singleLineHeight + verticalSpacing;
        }

        // 8. 绘制效果描述
        var descriptionRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(descriptionRect, 效果描述Prop, new GUIContent("效果描述"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 基础：效果类型 + 效果字段名称 + 基础数值 + 效果数值 + 效果等级配置 + 加成类型 + 效果描述 = 7 行
        // 若加成类型为 物品，再加 1 行（物品目标）
        // 目标变量：根据加成类型显示，总是显示 1 行
        float baseLines = 8f; // 7 + 1 (目标变量)

        var 加成类型Prop = property.FindPropertyRelative("加成类型");
        if (加成类型Prop != null && 加成类型Prop.propertyType == SerializedPropertyType.Enum)
        {
            var currentBonusType = (加成类型)加成类型Prop.enumValueIndex;
            if (currentBonusType == 加成类型.物品)
            {
                baseLines += 1f; // 物品目标
            }
        }

        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * baseLines;
    }

    private void DrawStringPopup(Rect rect, string label, SerializedProperty property, List<string> options)
    {
        if (options == null || options.Count == 0)
        {
            EditorGUI.LabelField(rect, label, "列表为空或未加载");
            return;
        }

        // 找到当前值的索引，如果不在列表中则显示为"请选择"
        int currentIndex = options.IndexOf(property.stringValue);
        string displayValue = currentIndex >= 0 ? property.stringValue : "请选择";
        
        // 创建下拉菜单
        if (EditorGUI.DropdownButton(rect, new GUIContent(displayValue), FocusType.Keyboard))
        {
            var menu = new GenericMenu();
            
            // 添加"请选择"选项
            menu.AddItem(new GUIContent("请选择"), currentIndex < 0, () => {
                property.stringValue = "";
            });
            
            // 添加分隔线
            menu.AddSeparator("");
            
            // 添加所有选项
            for (int i = 0; i < options.Count; i++)
            {
                string option = options[i];
                bool isSelected = option == property.stringValue;
                menu.AddItem(new GUIContent(option), isSelected, () => {
                    property.stringValue = option;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            
            menu.ShowAsContext();
        }
    }

    private void DrawItemTypePopup(Rect rect, string label, SerializedProperty property)
    {
        // 获取所有ItemType资源
        var itemTypes = LoadAllItemTypes();
        
        // 找到当前值的索引
        int currentIndex = itemTypes.IndexOf(property.stringValue);
        string displayValue = currentIndex >= 0 ? property.stringValue : "请选择";
        
        // 创建下拉菜单
        if (EditorGUI.DropdownButton(rect, new GUIContent(displayValue), FocusType.Keyboard))
        {
            var menu = new GenericMenu();
            
            // 添加"请选择"选项
            menu.AddItem(new GUIContent("请选择"), currentIndex < 0, () => {
                property.stringValue = "";
            });
            
            // 添加分隔线
            menu.AddSeparator("");
            
            // 添加所有ItemType选项
            for (int i = 0; i < itemTypes.Count; i++)
            {
                string option = itemTypes[i];
                bool isSelected = option == property.stringValue;
                menu.AddItem(new GUIContent(option), isSelected, () => {
                    property.stringValue = option;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            
            menu.ShowAsContext();
        }
    }

    private List<string> LoadAllItemTypes()
    {
        var itemTypeNames = new List<string>();
        
        // 查找所有ItemType资源
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemType");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var itemType = UnityEditor.AssetDatabase.LoadAssetAtPath<MiGame.Items.ItemType>(path);
            if (itemType != null && !string.IsNullOrEmpty(itemType.名字))
            {
                itemTypeNames.Add(itemType.名字);
            }
        }
        
        // 按名称排序
        itemTypeNames.Sort();
        
        return itemTypeNames;
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