using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiGame.Items
{
    [CreateAssetMenu(fileName = "物品", menuName = "公用/物品/物品")]
    public class ItemType : ScriptableObject {
        public string 名字;
        public 物品种类 物品类型;
        public string 描述;
        public string 图标;
        public ItemRank 品级;
        public int 战力;
        public float 强化倍率;
        public float 强化材料增加倍率;
        public int 最大强化等级;
        [NonSerialized]
        public ItemType 图鉴完成奖励;
        public int 图鉴完成奖励数量;
        public int 图鉴高级完成奖励数量;
        [NonSerialized]
        public ItemType 可进阶为;
        [NonSerialized]
        public ItemType 可售出为;
        public int 售出价格;
        public bool 在背包里显示 = true;
        /// <summary>是否可堆叠</summary>
        public bool 是否可堆叠 = false;
        /// <summary>最大可堆叠数量，范围1~100,000,000,000,000</summary>
        public long 最大数量 = 1;
        public string 获得音效;
        public bool 取消获得物品 = false;
        public List<string> 获得执行指令;
        public List<string> 使用执行指令;
        public List<ItemReward> 分解可得;

        public void OnValidate()
        {
            名字 = name;
            
#if UNITY_EDITOR
            // 货币序号相关校验已移除
#endif
        }

        public override string ToString() {
            return 名字;
        }
    }

    public enum ItemRank
    {
        普通,
        稀有,
        史诗,
        传说
    }

    public enum 物品种类
    {
        材料,
        物品,
        卡片,
        任务,
        货币
    }

    [Serializable]
    public struct ItemReward
    {
        public ItemType 物品;
        public int 数量;
    }
}