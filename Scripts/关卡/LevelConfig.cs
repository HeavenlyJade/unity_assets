using UnityEngine;
using System.Collections.Generic;
using MiGame.Items;

namespace MiGame.Data
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "关卡配置")]
    public class LevelConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("关卡的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 名字;

        [Tooltip("显示在UI上的关卡名称")]
        public string 关卡名称;
        
        [Tooltip("该关卡默认使用的玩法配置")]
        public GameModeConfig 默认玩法;

        [Header("人数限制")]
        [Tooltip("进入该关卡所需的最少玩家数")]
        public int 最少人数 = 1;

        [Tooltip("该关卡的最大玩家容量")]
        public int 最多人数 = 8;

        [Header("玩法参数")]
        [Tooltip("该关卡的详细玩法规则")]
        public GameRule 玩法规则 = new GameRule();
        
        [Header("执行指令")]
        [Tooltip("游戏开始时执行的指令列表")]
        public List<string> 游戏开始指令;
        
        [Tooltip("游戏结算时执行的指令列表")]
        public List<string> 游戏结算指令;
        
        [Header("奖励信息")]
        [Tooltip("所有参与者都能获得的基础奖励")]
        public List<RewardRule> 基础奖励;

        [Tooltip("根据最终排名配置的奖励")]
        public List<LevelRankReward> 排名奖励;

        [Header("实时奖励")]
        [Tooltip("实时奖励触发规则列表，当满足指定条件时立即给予奖励")]
        public List<RealtimeRewardRule> 实时奖励规则;

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