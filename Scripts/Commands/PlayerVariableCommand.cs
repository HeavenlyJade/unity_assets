using UnityEngine;
using MiGame;
using System.Collections.Generic;
using MiGame.Core;

namespace MiGame.Commands
{
    public enum VariableOperationType
    {
        新增,
        设置,
        减少,
        查看
    }

    [Command("variable", "用于操作玩家变量的指令")]
    public class PlayerVariableCommand : AbstractCommand
    {
        [Header("操作类型")]
        public VariableOperationType 操作类型;

        [Header("玩家UID")]
        public string 玩家UID;

        [Header("变量名")]
        [VariableName]
        public string 变量名;

        [Header("数值")]
        [Tooltip("支持输入整数、小数或长整型数值")]
        public string 数值;

        [Header("玩家加成")]
        [Tooltip("为玩家提供的属性加成")]
        [BonusType(VariableNameType.Stat)]
        public List<PlayerBonus> 玩家属性加成;

        [Tooltip("为玩家提供的变量加成")]
        [BonusType(VariableNameType.Variable)]
        public List<PlayerBonus> 玩家变量加成;

        [Tooltip("为玩家提供的其他加成（宠物、伙伴、尾迹、翅膀等）")]
        [SerializeField]
        public OtherBonusList 其他加成 = new OtherBonusList();

        public override void Execute()
        {
            if (string.IsNullOrEmpty(玩家UID))
            {
                Debug.LogError("玩家UID 不能为空");
                return;
            }

            if (操作类型 != VariableOperationType.查看 && string.IsNullOrEmpty(变量名))
            {
                Debug.LogError("变量名 不能为空");
                return;
            }

            // 数值类型判断和转换
            object 转换后数值 = null;
            if (!string.IsNullOrEmpty(数值))
            {
                if (long.TryParse(数值, out long longValue))
                {
                    转换后数值 = longValue;
                    Debug.Log($"数值 {数值} 已转换为长整型: {longValue}");
                }
                else if (double.TryParse(数值, out double doubleValue))
                {
                    转换后数值 = doubleValue;
                    Debug.Log($"数值 {数值} 已转换为浮点型: {doubleValue}");
                }
                else
                {
                    Debug.LogError($"无法解析数值: {数值}，请输入有效的数字格式");
                    return;
                }
            }

            // 在这里编写具体的操作逻辑
            switch (操作类型)
            {
                case VariableOperationType.新增:
                    Debug.Log($"新增变量: {变量名}, 数值: {数值} (类型: {转换后数值?.GetType().Name})");
                    // TODO: 调用游戏逻辑，使用 转换后数值
                    break;
                case VariableOperationType.设置:
                    Debug.Log($"设置变量: {变量名}, 数值: {数值} (类型: {转换后数值?.GetType().Name})");
                    // TODO: 调用游戏逻辑，使用 转换后数值
                    break;
                case VariableOperationType.减少:
                    Debug.Log($"减少变量: {变量名}, 数值: {数值} (类型: {转换后数值?.GetType().Name})");
                    // TODO: 调用游戏逻辑，使用 转换后数值
                    break;
                case VariableOperationType.查看:
                    if (string.IsNullOrEmpty(变量名))
                    {
                        Debug.Log($"查看玩家 {玩家UID} 的所有变量");
                        // TODO: 调用游戏逻辑
                    }
                    else
                    {
                        Debug.Log($"查看玩家 {玩家UID} 的变量: {变量名}");
                        // TODO: 调用游戏逻辑
                    }
                    break;
            }
        }
    }
}
