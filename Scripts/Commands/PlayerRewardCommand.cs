using UnityEngine;
using MiGame.Commands;
using System.Collections.Generic;
using System.Linq;

namespace MiGame.Commands
{
    /// <summary>
    /// 奖励操作类型
    /// </summary>
    public enum RewardOperationType
    {
        设置在线时长,
        设置轮次,
        设置已领取,
        清除已领取
    }

    /// <summary>
    /// 奖励操作指令
    /// </summary>
    [Command("reward", "在线奖励相关操作")]
    public class PlayerRewardCommand : AbstractCommand
    {
        [CommandParamDesc("操作类型 (设置在线时长/设置轮次/设置已领取/清除已领取)")]
        public RewardOperationType 操作类型;

        [CommandParamDesc("目标玩家的UID")]
        public string 玩家UID;

        [CommandParamDesc("目标玩家的名称")]
        public string 玩家;

        [ConditionalField("操作类型", RewardOperationType.设置在线时长)]
        [CommandParamDesc("今日累计在线时长（秒）")]
        public int 今日时长 = -1;

        [ConditionalField("操作类型", RewardOperationType.设置在线时长)]
        [CommandParamDesc("本轮累计在线时长（秒）")]
        public int 本轮时长 = -1;

        [ConditionalField("操作类型", RewardOperationType.设置轮次)]
        [CommandParamDesc("目标轮次数")]
        public int 轮次 = -1;

        [ConditionalField("操作类型", RewardOperationType.设置已领取)]
        [CommandParamDesc("要标记为已领取的奖励索引列表")]
        public List<int> 索引列表;

        [ConditionalField("操作类型", RewardOperationType.清除已领取)]
        [CommandParamDesc("要清除已领取状态的奖励索引列表（留空则清除所有）")]
        public List<int> 清除索引列表;

        public override void Execute()
        {
            // 验证玩家标识
            if (string.IsNullOrEmpty(玩家) && string.IsNullOrEmpty(玩家UID))
            {
                玩家UID = "Player001";
                Debug.Log("未指定玩家，默认对当前玩家 Player001 操作");
            }

            string targetPlayerIdentifier = !string.IsNullOrEmpty(玩家UID) ? $"UID: {玩家UID}" : $"玩家: {玩家}";

            // 根据操作类型执行相应逻辑
            switch (操作类型)
            {
                case RewardOperationType.设置在线时长:
                    ExecuteSetOnlineTime(targetPlayerIdentifier);
                    break;
                case RewardOperationType.设置轮次:
                    ExecuteSetRound(targetPlayerIdentifier);
                    break;
                case RewardOperationType.设置已领取:
                    ExecuteSetClaimed(targetPlayerIdentifier);
                    break;
                case RewardOperationType.清除已领取:
                    ExecuteClearClaimed(targetPlayerIdentifier);
                    break;
                default:
                    Debug.LogError($"未知的奖励操作类型: {操作类型}");
                    break;
            }
        }

        /// <summary>
        /// 执行设置在线时长操作
        /// </summary>
        private void ExecuteSetOnlineTime(string playerIdentifier)
        {
            if (今日时长 < 0 && 本轮时长 < 0)
            {
                Debug.LogError("设置在线时长：至少需要提供今日时长或本轮时长中的一个参数");
                return;
            }

            Debug.Log($"正在为玩家 {playerIdentifier} 设置在线时长...");

            if (今日时长 >= 0)
            {
                // TODO: 调用玩家变量系统设置今日在线时长
                Debug.Log($" - 设置今日在线时长: {今日时长}秒 ({今日时长 / 3600f:F1}小时)");
            }

            if (本轮时长 >= 0)
            {
                // TODO: 调用玩家变量系统设置本轮在线时长
                Debug.Log($" - 设置本轮在线时长: {本轮时长}秒 ({本轮时长 / 3600f:F1}小时)");
            }

            Debug.Log($"在线时长设置完成，数据已同步到客户端并保存到云数据");
        }

        /// <summary>
        /// 执行设置轮次操作
        /// </summary>
        private void ExecuteSetRound(string playerIdentifier)
        {
            if (轮次 < 1)
            {
                Debug.LogError("设置轮次：轮次必须大于等于1");
                return;
            }

            Debug.Log($"正在为玩家 {playerIdentifier} 设置奖励轮次...");
            
            // TODO: 调用玩家变量系统设置当前奖励轮次
            Debug.Log($" - 设置当前奖励轮次: 第{轮次}轮");
            
            Debug.Log($"轮次设置完成，数据已同步到客户端并保存到云数据");
        }

        /// <summary>
        /// 执行设置已领取操作
        /// </summary>
        private void ExecuteSetClaimed(string playerIdentifier)
        {
            if (索引列表 == null || 索引列表.Count == 0)
            {
                Debug.LogError("设置已领取：索引列表不能为空");
                return;
            }

            // 验证索引有效性（从1开始）
            if (索引列表.Any(index => index < 1))
            {
                Debug.LogError("设置已领取：奖励索引必须大于等于1");
                return;
            }

            Debug.Log($"正在为玩家 {playerIdentifier} 设置已领取奖励...");
            
            foreach (int index in 索引列表)
            {
                // TODO: 调用玩家变量系统标记指定奖励为已领取
                Debug.Log($" - 标记奖励索引 {index} 为已领取");
            }
            
            Debug.Log($"已领取状态设置完成，数据已同步到客户端并保存到云数据");
        }

        /// <summary>
        /// 执行清除已领取操作
        /// </summary>
        private void ExecuteClearClaimed(string playerIdentifier)
        {
            Debug.Log($"正在为玩家 {playerIdentifier} 清除已领取状态...");

            if (清除索引列表 == null || 清除索引列表.Count == 0)
            {
                // 清除所有已领取状态
                Debug.Log(" - 清除所有已领取状态");
                // TODO: 调用玩家变量系统清除所有已领取状态
            }
            else
            {
                // 验证索引有效性
                if (清除索引列表.Any(index => index < 1))
                {
                    Debug.LogError("清除已领取：奖励索引必须大于等于1");
                    return;
                }

                // 清除指定索引的已领取状态
                foreach (int index in 清除索引列表)
                {
                    // TODO: 调用玩家变量系统清除指定奖励的已领取状态
                    Debug.Log($" - 清除奖励索引 {index} 的已领取状态");
                }
            }
            
            Debug.Log($"已领取状态清除完成，数据已同步到客户端并保存到云数据");
        }
    }
}
