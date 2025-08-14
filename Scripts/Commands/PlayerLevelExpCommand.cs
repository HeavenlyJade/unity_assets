using UnityEngine;
using MiGame.Commands;

namespace MiGame.Commands
{
    /// <summary>
    /// 等级经验操作类型
    /// </summary>
    public enum LevelExpOperationType
    {
        新增,
        设置,
        减小
    }

    /// <summary>
    /// 玩家等级经验操作指令
    /// </summary>
    [Command("levelExp", "玩家等级和经验相关操作")]
    public class PlayerLevelExpCommand : AbstractCommand
    {
        [CommandParamDesc("操作类型 (新增/设置/减小)")]
        public LevelExpOperationType 操作类型;

        [CommandParamDesc("目标玩家的UID")]
        public string 玩家UID;

        [CommandParamDesc("目标玩家的名称")]
        public string 玩家;

        [ConditionalField("操作类型", LevelExpOperationType.新增, LevelExpOperationType.设置, LevelExpOperationType.减小)]
        [CommandParamDesc("等级数值 (可选，留空则不操作等级)")]
        public int 等级 = -1;

        [ConditionalField("操作类型", LevelExpOperationType.新增, LevelExpOperationType.设置, LevelExpOperationType.减小)]
        [CommandParamDesc("经验数值 (可选，留空则不操作经验)")]
        public int 经验 = -1;

        public override void Execute()
        {
            if (string.IsNullOrEmpty(玩家) && string.IsNullOrEmpty(玩家UID))
            {
                玩家UID = "Player001";
                Debug.Log("未指定玩家，默认对当前玩家 Player001 操作");
            }
            
            string targetPlayerIdentifier = !string.IsNullOrEmpty(玩家UID) ? $"UID: {玩家UID}" : $"玩家: {玩家}";

            // 验证参数
            if (等级 == -1 && 经验 == -1)
            {
                Debug.LogError("等级经验操作失败：必须至少提供等级或经验中的一个数值。");
                return;
            }

            // 执行操作
            switch (操作类型)
            {
                case LevelExpOperationType.新增:
                    ExecuteAddOperation(targetPlayerIdentifier);
                    break;

                case LevelExpOperationType.设置:
                    ExecuteSetOperation(targetPlayerIdentifier);
                    break;

                case LevelExpOperationType.减小:
                    ExecuteReduceOperation(targetPlayerIdentifier);
                    break;

                default:
                    Debug.LogError($"未知的操作类型：{操作类型}");
                    break;
            }
        }

        /// <summary>
        /// 执行新增操作
        /// </summary>
        private void ExecuteAddOperation(string targetPlayerIdentifier)
        {
            string changes = "";
            
            if (等级 > 0)
            {
                changes += $"等级 +{等级} ";
                // TODO: 调用游戏逻辑增加玩家等级
                Debug.Log($"为玩家 '{targetPlayerIdentifier}' 新增等级: +{等级}");
            }
            
            if (经验 > 0)
            {
                changes += $"经验 +{经验} ";
                // TODO: 调用游戏逻辑增加玩家经验
                Debug.Log($"为玩家 '{targetPlayerIdentifier}' 新增经验: +{经验}");
            }
            
            if (!string.IsNullOrEmpty(changes))
            {
                Debug.Log($"玩家 '{targetPlayerIdentifier}' 新增操作完成: {changes.Trim()}");
            }
        }

        /// <summary>
        /// 执行设置操作
        /// </summary>
        private void ExecuteSetOperation(string targetPlayerIdentifier)
        {
            string changes = "";
            
            if (等级 >= 0)
            {
                changes += $"等级设置为 {等级} ";
                // TODO: 调用游戏逻辑设置玩家等级
                Debug.Log($"为玩家 '{targetPlayerIdentifier}' 设置等级: {等级}");
            }
            
            if (经验 >= 0)
            {
                changes += $"经验设置为 {经验} ";
                // TODO: 调用游戏逻辑设置玩家经验
                Debug.Log($"为玩家 '{targetPlayerIdentifier}' 设置经验: {经验}");
            }
            
            if (!string.IsNullOrEmpty(changes))
            {
                Debug.Log($"玩家 '{targetPlayerIdentifier}' 设置操作完成: {changes.Trim()}");
            }
        }

        /// <summary>
        /// 执行减小操作
        /// </summary>
        private void ExecuteReduceOperation(string targetPlayerIdentifier)
        {
            string changes = "";
            
            if (等级 > 0)
            {
                changes += $"等级 -{等级} ";
                // TODO: 调用游戏逻辑减少玩家等级
                Debug.Log($"为玩家 '{targetPlayerIdentifier}' 减少等级: -{等级}");
            }
            
            if (经验 > 0)
            {
                changes += $"经验 -{经验} ";
                // TODO: 调用游戏逻辑减少玩家经验
                Debug.Log($"为玩家 '{targetPlayerIdentifier}' 减少经验: -{经验}");
            }
            
            if (!string.IsNullOrEmpty(changes))
            {
                Debug.Log($"玩家 '{targetPlayerIdentifier}' 减少操作完成: {changes.Trim()}");
            }
        }
    }
}
