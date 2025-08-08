using UnityEngine;
using MiGame.Commands;
using System.Collections;

namespace MiGame.Commands
{
    /// <summary>
    /// 动画操作类型
    /// </summary>
    public enum AnimationOperationType
    {
        启动飞行,
        取消飞行,
        设置动画,
        设置重力,
        设置移动速度,
        强制停止
    }

    /// <summary>
    /// 玩家动画控制指令
    /// </summary>
    [Command("animation", "玩家动画控制指令")]
    public class PlayerAnimationCommand : AbstractCommand
    {
        [CommandParamDesc("操作类型 (启动飞行/取消飞行/设置动画/设置重力/设置移动速度/强制停止)")]
        public AnimationOperationType 操作类型;

        [CommandParamDesc("目标玩家的UID")]
        public string 玩家UID;

        [CommandParamDesc("目标玩家的名称")]
        public string 玩家;

        [ConditionalField("操作类型", AnimationOperationType.启动飞行)]
        [CommandParamDesc("飞行动画名称")]
        public string 动画名称 = "Base Layer.fei";

        [ConditionalField("操作类型", AnimationOperationType.启动飞行)]
        [CommandParamDesc("重力值 (负值为向下重力，正值为向上重力，0为无重力)")]
        public float 重力值 = 0f;

        [ConditionalField("操作类型", AnimationOperationType.启动飞行)]
        [CommandParamDesc("是否禁用WASD移动控制")]
        public bool 禁用移动 = true;

        [ConditionalField("操作类型", AnimationOperationType.启动飞行)]
        [CommandParamDesc("飞行持续时间（秒），0表示无限期")]
        public float 持续时间 = 0f;

        [ConditionalField("操作类型", AnimationOperationType.设置动画)]
        [CommandParamDesc("要设置的动画名称")]
        public string 设置动画名称 = "Base Layer.Idle";

        [ConditionalField("操作类型", AnimationOperationType.设置重力)]
        [CommandParamDesc("重力值")]
        public float 设置重力值 = -9.8f;

        [ConditionalField("操作类型", AnimationOperationType.设置移动速度)]
        [CommandParamDesc("移动速度值")]
        public float 移动速度 = 10f;

        public override void Execute()
        {
            // 确定目标玩家
            string targetPlayer = GetTargetPlayer();
            if (string.IsNullOrEmpty(targetPlayer))
            {
                Debug.LogError("动画控制指令：未指定目标玩家");
                return;
            }

            // 执行相应的动画操作
            switch (操作类型)
            {
                case AnimationOperationType.启动飞行:
                    ExecuteStartFlying(targetPlayer);
                    break;
                case AnimationOperationType.取消飞行:
                    ExecuteStopFlying(targetPlayer);
                    break;
                case AnimationOperationType.设置动画:
                    ExecuteSetAnimation(targetPlayer);
                    break;
                case AnimationOperationType.设置重力:
                    ExecuteSetGravity(targetPlayer);
                    break;
                case AnimationOperationType.设置移动速度:
                    ExecuteSetMoveSpeed(targetPlayer);
                    break;
                case AnimationOperationType.强制停止:
                    ExecuteForceStop(targetPlayer);
                    break;
                default:
                    Debug.LogError($"动画控制指令：未知的操作类型 {操作类型}");
                    break;
            }
        }

        /// <summary>
        /// 获取目标玩家标识
        /// </summary>
        private string GetTargetPlayer()
        {
            if (!string.IsNullOrEmpty(玩家UID))
                return 玩家UID;
            if (!string.IsNullOrEmpty(玩家))
                return 玩家;
            return null;
        }

        /// <summary>
        /// 执行启动飞行
        /// </summary>
        private void ExecuteStartFlying(string playerId)
        {
            Debug.Log($"正在为玩家 {playerId} 启动飞行模式");
            Debug.Log($" - 动画名称: {动画名称}");
            Debug.Log($" - 重力值: {重力值}");
            Debug.Log($" - 禁用移动: {禁用移动}");
            Debug.Log($" - 持续时间: {(持续时间 > 0 ? $"{持续时间}秒" : "无限期")}");

            // TODO: 实现具体的飞行逻辑
            // 1. 设置玩家重力
            // 2. 播放飞行动画
            // 3. 禁用移动控制（如果需要）
            // 4. 设置定时器（如果指定了持续时间）

            if (持续时间 > 0)
            {
                Debug.Log($"飞行模式将在 {持续时间} 秒后自动取消");
            }
        }

        /// <summary>
        /// 执行取消飞行
        /// </summary>
        private void ExecuteStopFlying(string playerId)
        {
            Debug.Log($"正在为玩家 {playerId} 取消飞行模式");
            Debug.Log(" - 恢复原始重力值");
            Debug.Log(" - 恢复原始移动速度");
            Debug.Log(" - 恢复原始跳跃力");
            Debug.Log(" - 播放默认idle动画");
            Debug.Log(" - 恢复WASD移动控制");

            // TODO: 实现具体的取消飞行逻辑
            // 1. 恢复玩家原始重力值
            // 2. 恢复玩家原始移动速度
            // 3. 恢复玩家原始跳跃力
            // 4. 播放默认idle动画
            // 5. 恢复WASD移动控制
        }

        /// <summary>
        /// 执行设置动画
        /// </summary>
        private void ExecuteSetAnimation(string playerId)
        {
            Debug.Log($"正在为玩家 {playerId} 设置动画");
            Debug.Log($" - 动画名称: {设置动画名称}");

            // TODO: 实现具体的设置动画逻辑
            // 只改变玩家动画，不影响重力、移动控制等物理状态
        }

        /// <summary>
        /// 执行设置重力
        /// </summary>
        private void ExecuteSetGravity(string playerId)
        {
            Debug.Log($"正在为玩家 {playerId} 设置重力");
            Debug.Log($" - 重力值: {设置重力值}");

            // TODO: 实现具体的设置重力逻辑
            // 单独设置玩家重力，不影响其他状态
        }

        /// <summary>
        /// 执行设置移动速度
        /// </summary>
        private void ExecuteSetMoveSpeed(string playerId)
        {
            Debug.Log($"正在为玩家 {playerId} 设置移动速度");
            Debug.Log($" - 移动速度: {移动速度}");

            // TODO: 实现具体的设置移动速度逻辑
            // 单独设置玩家移动速度，不影响其他状态
        }

        /// <summary>
        /// 执行强制停止
        /// </summary>
        private void ExecuteForceStop(string playerId)
        {
            Debug.Log($"正在为玩家 {playerId} 强制停止所有动画控制");
            Debug.Log(" - 立即停止所有动画控制");
            Debug.Log(" - 恢复所有原始状态");

            // TODO: 实现具体的强制停止逻辑
            // 1. 立即停止所有动画控制
            // 2. 恢复所有原始状态
            // 3. 用于紧急情况或管理员干预
        }
    }
}
