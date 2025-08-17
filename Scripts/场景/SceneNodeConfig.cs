using UnityEngine;
using System.Collections.Generic;
using MiGame.Data;
using System.ComponentModel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiGame.Scene
{
    /// <summary>
    /// 场景中特殊节点的类型
    /// </summary>
    public enum SceneNodeType
    {
        陷阱,
        跳台,
        安全区,
        飞行比赛,
        挂机点,
        抽奖点,
        复活点,
        传送点,
        导航点,
        比赛场景,
        区域节点
    }

    /// <summary>
    /// 教程所属场景类型
    /// </summary>
    public enum TutorialSceneType
    {
        [Description("init_map")]
        init_map,
        [Description("midLevelZone")]
        midLevelZone,
        [Description("advancedArea")]
        advancedArea,
        [Description("ultimateRealm")]
        ultimateRealm
    }

    /// <summary>
    /// 用于存储定时指令和其触发间隔的组合
    /// </summary>
    [System.Serializable]
    public class TimedCommand
    {
        [Tooltip("要执行的定时指令")]
        [TextArea(3, 10)] 
        public string 指令;

        [Tooltip("距离上一次执行的间隔时间（秒）")]
        public float 间隔 = 1f;
    }

    /// <summary>
    /// 进入条件项目
    /// </summary>
    [System.Serializable]
    public class 进入条件项目
    {
        [Tooltip("条件检查的公式，变量格式: [物品], $玩家变量$, {玩家属性}")]
        public string 条件公式;
    }

    /// <summary>
    /// 区域节点配置
    /// </summary>
    [System.Serializable]
    public class 区域节点配置
    {
        [Tooltip("作为触发区域的包围盒节点名字")]
        public string 包围盒节点;

        [Tooltip("该区域关联的复活点标识")]
        public string 复活节点;

        [Tooltip("该区域关联的传送门标识")]
        public string 传送节点;

        [Tooltip("该区域关联的导航节点标识")]
        public string 导航节点;

        [Tooltip("该区域关联的需求描述节点标识")]
        public string 需求描述节点;

        [Tooltip("该区域关联的作用描述节点标识")]
        public string 作用描述节点;

        [Tooltip("该区域关联的倒计时显示节点标识")]
        public string 倒计时显示节点;

        [Tooltip("该区域关联的比赛场景标识")]
        public string 比赛场景;
    }

    /// <summary>
    /// 场景节点配置
    /// </summary>
    [CreateAssetMenu(fileName = "NewSceneNodeConfig", menuName = "场景节点配置")]
    public class SceneNodeConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("节点的名称(根据文件名自动生成)")]
        [ReadOnly]
        public string 名字;

        [Tooltip("节点的唯一ID，在创建时自动生成")]
        [ReadOnly]
        public string 唯一ID;

        [Tooltip("场景中预制体的路径，用于加载")]
        public string 场景节点路径;

        [Header("节点属性")]
        [Tooltip("节点的类型")]
        public SceneNodeType 场景类型;

        [Tooltip("所属场景")]
        public TutorialSceneType 所属场景;

        [Tooltip("区域节点配置")]
        public 区域节点配置 区域节点配置 = new 区域节点配置();

        [Tooltip("该节点关联的关卡配置")]
        public LevelConfig 关联关卡;

        [Tooltip("具体的玩法规则，当此节点触发时生效")]
        public GameRule 玩法规则 = new GameRule();

        [Tooltip("节点触发时的音效，填写资源的路径")]
        public string 音效资源;

        [Header("描述信息")]
        [Tooltip("该节点的需求描述")]
        [TextArea(3, 5)]
        public string 需求描述;

        [Tooltip("该节点的作用描述")]
        [TextArea(3, 5)]
        public string 作用描述;

        [Header("进入条件")]
        [Tooltip("定义进入该节点需要满足的条件列表")]
        public List<进入条件项目> 进入条件列表;

        [Header("指令")]
        [Tooltip("进入节点时触发的指令")]
        public string 进入指令;

        [Tooltip("离开节点时触发的指令")]
        public string 离开指令;

        [Tooltip("定时触发的指令列表，可以添加多个")]
        public List<TimedCommand> 定时指令列表;

        private void OnValidate()
        {
#if UNITY_EDITOR
            // 当唯一ID为空时，我们认为这是一个新创建的资产。
            // 此时，我们自动生成ID，并将资产的文件名同步到"名字"字段。
            // 一旦ID被创建，这个过程就不会再自动发生，允许用户自由修改"名字"字段而不会被文件名覆盖。
            if (string.IsNullOrEmpty(唯一ID))
            {
                唯一ID = System.Guid.NewGuid().ToString();
                名字 = name;
                UnityEditor.EditorUtility.SetDirty(this);
            }

            // 校验进入条件列表
            ValidateEnterConditions();
#endif
        }

#if UNITY_EDITOR
        [System.Serializable]
        private class VariableData
        {
            public List<string> VariableNames = new List<string>();
            public List<string> StatNames = new List<string>();
        }

        [System.Serializable]
        private class ItemNameListWrapper
        {
            public List<string> ItemNames = new List<string>();
        }

        private void ValidateEnterConditions()
        {
            // 加载JSON数据
            LoadAllVariableNamesFromJson(out var allVariableNames, out var allStatNames);
            LoadAllItemNamesFromJson(out var allItemNames);

            if (allVariableNames == null || allStatNames == null || allItemNames == null) return;

            if (进入条件列表 == null) return;
            foreach (var condition in 进入条件列表)
            {
                // 校验条件公式中的变量引用
                if (!string.IsNullOrEmpty(condition.条件公式))
                {
                    ValidateFormulaString(condition.条件公式, "条件公式", allVariableNames, allStatNames, allItemNames);
                }
            }
        }

        private void LoadAllItemNamesFromJson(out HashSet<string> itemNames)
        {
            itemNames = null;
            string jsonPath = "Assets/GameConf/物品/ItemNames.json";
            if (System.IO.File.Exists(jsonPath))
            {
                string json = System.IO.File.ReadAllText(jsonPath);
                var data = JsonUtility.FromJson<ItemNameListWrapper>(json);
                if (data != null)
                {
                    itemNames = new HashSet<string>(data.ItemNames ?? new List<string>());
                }
            }
            else
            {
                Debug.LogWarning($"JSON 文件未找到: {jsonPath}. 物品名校验功能将不会执行。", this);
                itemNames = new HashSet<string>(); // 即使文件不存在，也返回一个空集合以避免null错误
            }
        }

        private void LoadAllVariableNamesFromJson(out HashSet<string> variableNames, out HashSet<string> statNames)
        {
            variableNames = null;
            statNames = null;
            
            string jsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
            if (System.IO.File.Exists(jsonPath))
            {
                string json = System.IO.File.ReadAllText(jsonPath);
                VariableData data = JsonUtility.FromJson<VariableData>(json);

                if (data != null)
                {
                    variableNames = new HashSet<string>(data.VariableNames ?? new List<string>());
                    statNames = new HashSet<string>(data.StatNames ?? new List<string>());
                }
            }
            else
            {
                Debug.LogWarning($"JSON 文件未找到: {jsonPath}. 变量名校验功能将不会执行。", this);
            }
        }

        private void ValidateFormulaString(string formula, string fieldName, HashSet<string> variableNames, HashSet<string> statNames, HashSet<string> itemNames)
        {
            if (string.IsNullOrEmpty(formula)) return;

            // 校验玩家变量: $...$
            var playerVarRegex = new System.Text.RegularExpressions.Regex(@"\$([^\$]+)\$");
            foreach (System.Text.RegularExpressions.Match match in playerVarRegex.Matches(formula))
            {
                var variableName = match.Groups[1].Value;
                if (!variableNames.Contains(variableName))
                {
                    Debug.LogError($"配置错误: 在 '{fieldName}' 字段中, 玩家变量 '${variableName}$' 在 VariableNames.json 中未定义!", this);
                }
            }

            // 校验玩家属性: {...}
            var statRegex = new System.Text.RegularExpressions.Regex(@"\{([^\}]+)\}");
            foreach (System.Text.RegularExpressions.Match match in statRegex.Matches(formula))
            {
                var variableName = match.Groups[1].Value;
                if (!statNames.Contains(variableName))
                {
                    Debug.LogError($"配置错误: 在 '{fieldName}' 字段中, 玩家属性 '{{{variableName}}}' 在 VariableNames.json 中未定义!", this);
                }
            }

            // 校验物品: [...]
            var itemRegex = new System.Text.RegularExpressions.Regex(@"\[([^\]]+)\]");
            foreach (System.Text.RegularExpressions.Match match in itemRegex.Matches(formula))
            {
                var itemName = match.Groups[1].Value;
                if (!itemNames.Contains(itemName))
                {
                    Debug.LogError($"配置错误: 在 '{fieldName}' 字段中, 物品 '[{itemName}]' 在 ItemNames.json 中未定义!", this);
                }
            }
        }
#endif
    }
}

#if UNITY_EDITOR
namespace MiGame.Scene
{
    [CustomPropertyDrawer(typeof(TutorialSceneType))]
    public class TutorialSceneTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // 获取枚举值
            TutorialSceneType currentValue = (TutorialSceneType)property.enumValueIndex;
            
            // 获取所有枚举值
            string[] enumNames = System.Enum.GetNames(typeof(TutorialSceneType));
            string[] displayNames = new string[enumNames.Length];
            
            // 使用Description特性或原始名称
            for (int i = 0; i < enumNames.Length; i++)
            {
                var field = typeof(TutorialSceneType).GetField(enumNames[i]);
                var descriptionAttr = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                displayNames[i] = descriptionAttr.Length > 0 
                    ? ((DescriptionAttribute)descriptionAttr[0]).Description 
                    : enumNames[i];
            }
            
            // 显示下拉菜单
            int currentIndex = property.enumValueIndex;
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, displayNames);
            
            if (newIndex != currentIndex)
            {
                property.enumValueIndex = newIndex;
            }
            
            EditorGUI.EndProperty();
        }
    }
}
#endif
