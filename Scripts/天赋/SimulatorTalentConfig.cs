using UnityEngine;
using System.Collections.Generic;
using MiGame.Items;

namespace MiGame.Data
{
    public enum 天赋类型
    {
        栏位,
        属性
    }

    [System.Serializable]
    public struct TalentCostItem
    {
        public ItemType 物品;
        public string 数量公式;
    }

    [CreateAssetMenu(fileName = "NewTalentConfig", menuName = "天赋配置")]
    public class SimulatorTalentConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("配置的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 名字;

        public int 排序;

        [Header("天赋属性")]
        public 天赋类型 类型;
        public int 最大等级;
        public List<TalentCostItem> 消耗物品;
        public string 加成倍率公式;

        private void OnValidate()
        {
            // 自动将资产文件名同步到"名字"字段
            if (name != 名字)
            {
                名字 = name;
            }
        }
    }
} 