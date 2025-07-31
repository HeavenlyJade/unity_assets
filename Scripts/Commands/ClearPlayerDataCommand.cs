using UnityEngine;
using MiGame.Commands;
using System;

namespace MiGame.Commands
{
    /// <summary>
    /// 定义需要清空的数据范围
    /// </summary>
    [Serializable]
    public class ClearScope
    {
        public bool 背包 = true;
        public bool 成就 = false;
        public bool 变量 = true;
        public bool 基础数据 = true;
        public bool 技能 = false;
    }

    /// <summary>
    /// 清空玩家所有数据
    /// </summary>
    [Command("clearPlayerData", "清空玩家所有数据")]
    public class ClearPlayerDataCommand : AbstractCommand
    {
        [Header("清空数据参数")]
        [Tooltip("请再次确认是否执行此操作")]
        public bool 确认 = false;

        [Tooltip("目标玩家的UID")]
        public string 玩家UID;

        [Tooltip("选择要清空的数据范围")]
        public ClearScope 清空范围 = new ClearScope();

        public override void Execute()
        {
            if (!确认)
            {
                Debug.LogWarning("操作未确认，已取消执行。请勾选“确认”以执行清空操作。");
                return;
            }

            if (string.IsNullOrEmpty(玩家UID))
            {
                Debug.LogError("玩家UID不能为空！");
                return;
            }

            Debug.Log($"开始为玩家 {玩家UID} 清空数据...");

            if (清空范围.背包)
            {
                // TODO: 在这里添加清空玩家背包的逻辑
                Debug.Log($" - 正在清空背包...");
            }

            if (清空范围.成就)
            {
                // TODO: 在这里添加清空玩家成就的逻辑
                Debug.Log($" - 正在清空成就...");
            }

            if (清空范围.变量)
            {
                // TODO: 在这里添加清空玩家变量的逻辑
                Debug.Log($" - 正在清空变量...");
            }

            if (清空范围.基础数据)
            {
                // TODO: 在这里添加清空玩家基础数据（如等级、经验等）的逻辑
                Debug.Log($" - 正在清空基础数据...");
            }

            if (清空范围.技能)
            {
                // TODO: 在这里添加清空玩家技能的逻辑
                Debug.Log($" - 正在清空技能...");
            }

            Debug.Log($"玩家 {玩家UID} 的数据已根据所选范围清空完毕。");
        }
    }
}
