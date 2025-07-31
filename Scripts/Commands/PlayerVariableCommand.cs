using UnityEngine;

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
        public float 数值;

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

            // 在这里编写具体的操作逻辑
            switch (操作类型)
            {
                case VariableOperationType.新增:
                    Debug.Log($"新增变量: {变量名}, 数值: {数值}");
                    // TODO: 调用游戏逻辑
                    break;
                case VariableOperationType.设置:
                    Debug.Log($"设置变量: {变量名}, 数值: {数值}");
                    // TODO: 调用游戏逻辑
                    break;
                case VariableOperationType.减少:
                    Debug.Log($"减少变量: {变量名}, 数值: {数值}");
                    // TODO: 调用游戏逻辑
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
