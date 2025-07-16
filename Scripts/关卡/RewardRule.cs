using MiGame.Items;
using UnityEngine;

namespace MiGame.Data
{
    /// <summary>
    /// 奖励计算方式
    /// </summary>
    public enum RewardCalculationType
    {
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

        [Tooltip("奖励公式或固定数量。\n" +
                 "输入纯数字（如：100）表示固定数量奖励\n" +
                 "输入公式（如：d * 0.1 + k * 5）表示按公式计算奖励")]
        public string 奖励公式 = "1";
    }

    /// <summary>
    /// 触发条件类
    /// </summary>
    [System.Serializable]
    public class TriggerCondition
    {
        [Tooltip("触发条件变量名称，例如：击杀数、完成时间、连击数等")]
        public string 触发条件变量;

        [Tooltip("达到该数值时触发奖励")]
        public float 触发条件数量;
    }

    /// <summary>
    /// 实时奖励触发规则
    /// </summary>
    [System.Serializable]
    public class RealtimeRewardRule
    {
        [Tooltip("触发条件")]
        public TriggerCondition 触发条件;

        [Tooltip("触发该条件时给予的奖励物品")]
        public ItemType 奖励物品;

        [Tooltip("奖励公式或固定数量。\n" +
                 "输入纯数字（如：100）表示固定数量奖励\n" +
                 "输入公式（如：score * 0.1 + combo * 2）表示按公式计算奖励")]
        public string 奖励公式 = "1";
    }
} 