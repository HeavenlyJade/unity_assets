using System;
using UnityEngine;
using System.Collections.Generic;
using MiGame.Items;
using MiGame.Skills;
using MiGame.Pet;
using MiGame.Trail;

namespace MiGame.Commands
{
    // 前向声明，如果这些类在其他地方定义
    public class Spell : ScriptableObject { }
    public class SubSpell : ScriptableObject { }
    public class FocusOnUI : ScriptableObject { }
    public class Graphic : ScriptableObject { }

    [Serializable]
    public class ItemTypeIntDictionary : SerializableDictionary<ItemType, int>
    {
    }

    [Serializable]
    public class SkillIntDictionary : SerializableDictionary<Skill, int>
    {
    }

    /// <summary>
    /// 邮件附件类型枚举
    /// </summary>
    public enum 附件类型
    {
        物品,
        宠物,
        伙伴,
        尾迹,
        玩家变量
    }

    /// <summary>
    /// 邮件附件配置
    /// </summary>
    [Serializable]
    public class 邮件附件配置
    {
        [Tooltip("附件类型")]
        public 附件类型 类型 = 附件类型.物品;
        
        [Tooltip("物品配置（类型为物品时使用）")]
        public ItemType 物品配置;
        
        [Tooltip("宠物配置（类型为宠物时使用）")]
        public PetConfig 宠物配置;
        
        [Tooltip("伙伴配置（类型为伙伴时使用）")]
        public PartnerConfig 伙伴配置;
        
        [Tooltip("尾迹配置（类型为尾迹时使用）")]
        public BaseTrailConfig 尾迹配置;
        
        [Tooltip("变量名称（类型为玩家变量时使用）")]
        public string 变量名称 = "";
        
        [Tooltip("数量")]
        public int 数量 = 1;
        
        [Tooltip("星级（物品、宠物、伙伴、尾迹时使用）")]
        public int 星级 = 1;
        
        [Tooltip("数值（玩家变量时使用）")]
        public float 数值 = 0f;

        /// <summary>
        /// 获取配置的名称（用于JSON导出）
        /// </summary>
        public string 获取名称()
        {
            switch (类型)
            {
                case 附件类型.物品:
                    return 物品配置 != null ? 物品配置.名字 : "未选择";
                case 附件类型.宠物:
                    return 宠物配置 != null ? 宠物配置.宠物名称 : "未选择";
                case 附件类型.伙伴:
                    return 伙伴配置 != null ? 伙伴配置.宠物名称 : "未选择";
                case 附件类型.尾迹:
                    return 尾迹配置 != null ? 尾迹配置.名称 : "未选择";
                case 附件类型.玩家变量:
                    return 变量名称;
                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 导出为JSON格式
        /// </summary>
        public string 导出为JSON()
        {
            var jsonBuilder = new System.Text.StringBuilder();
            jsonBuilder.Append("{");
            jsonBuilder.Append($"\"类型\":\"{类型}\",");

            if (类型 == 附件类型.玩家变量)
            {
                jsonBuilder.Append($"\"变量名\":\"{变量名称}\",");
                jsonBuilder.Append($"\"数值\":{数值}");
            }
            else
            {
                jsonBuilder.Append($"\"名称\":\"{获取名称()}\",");
                jsonBuilder.Append($"\"数量\":{数量},");
                jsonBuilder.Append($"\"星级\":{星级}");
            }

            jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }
    }
}