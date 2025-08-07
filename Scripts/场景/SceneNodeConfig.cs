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
#endif
        }
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
