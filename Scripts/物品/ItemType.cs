using UnityEngine;
using System;
using System.Collections.Generic;

namespace MiGame.Items
{
    [CreateAssetMenu(fileName = "物品", menuName = "公用/物品/物品")]
    public class ItemType : ScriptableObject {
        public string 名字;
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
        public bool 是货币 = false;
        public int 货币序号 = 0;
        public string 获得音效;
        public bool 取消获得物品 = false;
        public List<string> 获得执行指令;

        public void OnValidate()
        {
            名字 = name;
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
}