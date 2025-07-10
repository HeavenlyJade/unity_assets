using MiGame.Items;
using UnityEngine;

namespace MiGame.Data
{
    /// <summary>
    /// 奖励计算方式
    /// </summary>
    public enum RewardCalculationType
    {
        [Tooltip("固定数量：无视任何条件，给予一个固定值的奖励")]
        固定,
        [Tooltip("公式计算：根据一个数学公式来动态计算奖励数量")]
        公式,
        [Tooltip("飞车挑战赛专用，具体结算逻辑由游戏运行时实现")]
        飞车挑战赛
    }

    [System.Serializable]
    public class RewardRule
    {
       

        [Tooltip("奖励的物品是什么")]
        public ItemType 奖励物品;
        
        [Tooltip("选择奖励是给一个固定值，还是按公式计算")]
        public RewardCalculationType 计算方式;

        [Tooltip("如果计算方式是 [Fixed], 则直接奖励这个数量")]
        public int 固定数量 = 1;

        [Tooltip("如果计算方式是 [Formula], 则使用此公式字符串。\n" +
                 "该字符串将由游戏运行时解析，具体可用变量需参考游戏项目文档。\n" +
                 "示例: 'd * 0.1 + k * 5'")]
        public string 奖励公式 = "d * 0.1";
    }
} 