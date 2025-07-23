using UnityEngine;
using MiGame.Items;
using MiGame.Commands;

namespace MiGame.Commands
{
    /// <summary>
    /// 操作类型枚举
    /// </summary>
    public enum ItemOperationType
    {
        新增,
        修改,
        减少
    }

    /// <summary>
    /// 玩家物品操作指令
    /// </summary>
    [Command("PlayerItem", "给玩家新增、修改、减少物品")]
    public class PlayerItemCommand : AbstractCommand
    {
        [Header("物品操作参数")]
        [Tooltip("玩家UID")]
        public string 玩家UID;
        
        [Tooltip("要操作的物品类型")]
        public ItemType 物品类型;
        
        [Tooltip("操作数量")]
        public int 数量;
        
        [Tooltip("操作类型")]
        public ItemOperationType 操作类型;

        [Header("可选参数")]
        [Tooltip("是否显示操作结果提示")]
        public bool 显示提示 = true;

        public override void Execute()
        {
            if (string.IsNullOrEmpty(玩家UID))
            {
                Debug.LogError("物品操作指令：玩家UID不能为空");
                return;
            }

            if (物品类型 == null)
            {
                Debug.LogError("物品操作指令：物品类型不能为空");
                return;
            }

            if (数量 <= 0)
            {
                Debug.LogWarning($"物品操作指令：数量必须大于0，当前数量：{数量}");
                return;
            }

            // 执行物品操作
            bool success = ExecuteItemOperation();

            // 显示操作结果
            if (显示提示)
            {
                string operationText = GetOperationText();
                string resultText = success ? "成功" : "失败";
                Debug.Log($"玩家[{玩家UID}]物品操作{resultText}：{operationText} {物品类型.name} x{数量}");
            }
        }

        /// <summary>
        /// 执行具体的物品操作
        /// </summary>
        private bool ExecuteItemOperation()
        {
            // TODO: 这里需要连接到实际的玩家物品系统
            // 示例实现，你需要根据实际的物品系统进行修改
            
            switch (操作类型)
            {
                case ItemOperationType.新增:
                    return AddItem();
                    
                case ItemOperationType.修改:
                    return SetItem();
                    
                case ItemOperationType.减少:
                    return RemoveItem();
                    
                default:
                    Debug.LogError($"未知的操作类型：{操作类型}");
                    return false;
            }
        }

        /// <summary>
        /// 新增物品
        /// </summary>
        private bool AddItem()
        {
            // TODO: 调用玩家物品系统的添加方法
            // 例如：PlayerInventory.Instance.AddItem(玩家UID, 物品类型, 数量);
            Debug.Log($"玩家[{玩家UID}]新增物品：{物品类型.name} x{数量}");
            return true;
        }

        /// <summary>
        /// 设置物品数量
        /// </summary>
        private bool SetItem()
        {
            // TODO: 调用玩家物品系统的设置方法
            // 例如：PlayerInventory.Instance.SetItem(玩家UID, 物品类型, 数量);
            Debug.Log($"玩家[{玩家UID}]设置物品：{物品类型.name} x{数量}");
            return true;
        }

        /// <summary>
        /// 减少物品
        /// </summary>
        private bool RemoveItem()
        {
            // TODO: 调用玩家物品系统的移除方法
            // 例如：PlayerInventory.Instance.RemoveItem(玩家UID, 物品类型, 数量);
            Debug.Log($"玩家[{玩家UID}]减少物品：{物品类型.name} x{数量}");
            return true;
        }

        /// <summary>
        /// 获取操作类型的中文描述
        /// </summary>
        private string GetOperationText()
        {
            switch (操作类型)
            {
                case ItemOperationType.新增:
                    return "新增";
                case ItemOperationType.修改:
                    return "修改";
                case ItemOperationType.减少:
                    return "减少";
                default:
                    return "未知操作";
            }
        }
    }
} 