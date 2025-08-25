using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;

namespace MiGame.Level
{
    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum RewardType
    {
        [Tooltip("物品")]
        物品
    }

    /// <summary>
    /// 关卡节点奖励配置
    /// 用于配置关卡中各个节点的奖励内容
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelNodeReward", menuName = "配置/关卡节点奖励")]
    public class LevelNodeRewardConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("配置的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 配置名称;

        [Tooltip("配置描述")]
        [TextArea(2, 4)]
        public string 配置描述;

        [Tooltip("克隆的场景节点路径")]
        public string 克隆的场景节点路径;

        [Tooltip("所属场景")]
        public string 所属场景;

        [Header("节点奖励配置")]
        [Tooltip("关卡节点的奖励列表")]
        public List<LevelNodeReward> 节点列表 = new List<LevelNodeReward>();

        private void OnValidate()
        {
            // 自动将资产文件名同步到"配置名称"字段
            if (name != 配置名称)
            {
                配置名称 = name;
            }

            // 确保所有节点都有唯一ID
            if (节点列表 != null)
            {
                foreach (var node in 节点列表)
                {
                    if (node != null)
                    {
                        node.EnsureUniqueID();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 关卡节点奖励结构
    /// </summary>
    [Serializable]
    public class LevelNodeReward
    {
        [Tooltip("奖励类型")]
        public RewardType 奖励类型 = RewardType.物品;

        [Tooltip("奖励物品（奖励类型为资源物品时使用）")]
        [ConditionalField("奖励类型", RewardType.物品)]
        public ItemType 物品类型;

        [Tooltip("物品数量")]
        public int 物品数量 = 1;

        [Tooltip("奖励条件")]
        [TextArea(2, 4)]
        public string 奖励条件;

        [Tooltip("生成的距离配置")]
        public long 生成的距离配置 = 1;

        [Tooltip("唯一ID")]
        [ReadOnly]
        public string 唯一ID;

        /// <summary>
        /// 确保唯一ID存在，如果为空则生成新的
        /// </summary>
        public void EnsureUniqueID()
        {
            if (string.IsNullOrEmpty(唯一ID))
            {
                唯一ID = Guid.NewGuid().ToString();
            }
        }
    }
}
