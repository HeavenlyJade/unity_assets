using UnityEngine;
using MiGame.Commands;
using MiGame.Trail;

namespace MiGame.Commands
{
    /// <summary>
    /// 尾迹操作类型
    /// </summary>
    public enum TrailOperationType
    {
        新增,
        删除,
        装备,
        卸下,
        设置,
        栏位设置,
        信息
    }

    /// <summary>
    /// 尾迹操作指令
    /// </summary>
    [Command("trail", "尾迹相关操作")]
    public class PlayerTrailCommand : AbstractCommand
    {
        [CommandParamDesc("操作类型 (新增/删除/装备/卸下/设置/栏位设置/信息)")]
        public TrailOperationType 操作类型;

        [CommandParamDesc("目标玩家的UIN")]
        public string 玩家UID;

        [CommandParamDesc("目标玩家的名称")]
        public string 玩家;

        [CommandParamDesc("要操作的尾迹所在的槽位索引")]
        public int 槽位 = -1;

        [ConditionalField("操作类型", TrailOperationType.新增)]
        [CommandParamDesc("要新增的尾迹 (从配置选择)")]
        public BaseTrailConfig 尾迹;

        [ConditionalField("操作类型", TrailOperationType.装备)]
        [CommandParamDesc("装备栏位名称")]
        public string 装备栏;

        [ConditionalField("操作类型", TrailOperationType.卸下)]
        [CommandParamDesc("卸下装备栏位名称")]
        public string 卸下装备栏;

        [ConditionalField("操作类型", TrailOperationType.设置)]
        [CommandParamDesc("设置尾迹的自定义名称")]
        public string 自定义名称;

        [ConditionalField("操作类型", TrailOperationType.设置)]
        [CommandParamDesc("设置尾迹的锁定状态")]
        public string 锁定;
        
        [ConditionalField("操作类型", TrailOperationType.栏位设置)]
        [CommandParamDesc("设置玩家可以同时装备的尾迹数量")]
        public int 可携带 = -1;

        [ConditionalField("操作类型", TrailOperationType.栏位设置)]
        [CommandParamDesc("设置玩家尾迹背包的总容量")]
        public int 背包 = -1;

        public override void Execute()
        {
            if (string.IsNullOrEmpty(玩家) && string.IsNullOrEmpty(玩家UID))
            {
                // 在实际项目中，这里应替换为获取当前指令执行者的逻辑
                玩家UID = "Player001";
                Debug.Log("未指定玩家，默认对当前玩家 Player001 操作");
            }
            
            string targetPlayerIdentifier = !string.IsNullOrEmpty(玩家UID) ? $"UIN: {玩家UID}" : $"玩家: {玩家}";

            switch (操作类型)
            {
                case TrailOperationType.新增:
                    if (尾迹 == null)
                    {
                        Debug.LogError("新增尾迹失败：必须选择一个尾迹配置。");
                        return;
                    }
                    Debug.Log($"向玩家 '{targetPlayerIdentifier}' 新增尾迹 '{尾迹.name}'，槽位: {(槽位 == -1 ? "自动" : 槽位.ToString())}。");
                    // TODO: 实现新增尾迹的逻辑
                    break;

                case TrailOperationType.删除:
                    if (槽位 == -1)
                    {
                        Debug.LogError("删除尾迹失败：必须提供槽位索引。");
                        return;
                    }
                    Debug.Log($"从玩家 '{targetPlayerIdentifier}' 删除槽位 {槽位} 的尾迹。");
                    // TODO: 实现删除尾迹的逻辑
                    break;

                case TrailOperationType.装备:
                    if (槽位 == -1)
                    {
                        Debug.LogError("装备尾迹失败：必须提供槽位索引。");
                        return;
                    }
                    if (string.IsNullOrEmpty(装备栏))
                    {
                        Debug.LogError("装备尾迹失败：必须提供装备栏位名称。");
                        return;
                    }
                    Debug.Log($"为玩家 '{targetPlayerIdentifier}' 装备槽位 {槽位} 的尾迹到装备栏 '{装备栏}'。");
                    // TODO: 实现装备尾迹的逻辑
                    break;

                case TrailOperationType.卸下:
                    if (string.IsNullOrEmpty(卸下装备栏))
                    {
                        Debug.LogError("卸下尾迹失败：必须提供装备栏位名称。");
                        return;
                    }
                    Debug.Log($"为玩家 '{targetPlayerIdentifier}' 卸下装备栏 '{卸下装备栏}' 的尾迹。");
                    // TODO: 实现卸下尾迹的逻辑
                    break;

                case TrailOperationType.设置:
                    if (槽位 == -1)
                    {
                        Debug.LogError("设置尾迹属性失败：必须提供槽位索引。");
                        return;
                    }
                    if (string.IsNullOrEmpty(自定义名称) && string.IsNullOrEmpty(锁定))
                    {
                        Debug.LogError("设置尾迹属性失败：必须至少提供自定义名称或锁定状态中的一个。");
                        return;
                    }

                    string changes = "";
                    if (!string.IsNullOrEmpty(自定义名称)) changes += $"自定义名称设置为 '{自定义名称}' ";
                    if (!string.IsNullOrEmpty(锁定)) changes += $"锁定状态设置为 '{锁定}' ";
                    Debug.Log($"设置玩家 '{targetPlayerIdentifier}' 槽位 {槽位} 的尾迹属性: {changes.Trim()}。");
                    // TODO: 实现设置尾迹属性的逻辑
                    break;
                
                case TrailOperationType.栏位设置:
                    if (可携带 <= 0 && 背包 <= 0)
                    {
                        Debug.LogError("栏位设置失败：必须至少提供 '可携带' 或 '背包' 中的一个，并且值必须大于0。");
                        return;
                    }

                    string settings = "";
                    if (可携带 > 0) settings += $"可携带栏位设置为 {可携带} ";
                    if (背包 > 0) settings += $"背包容量设置为 {背包} ";
                    Debug.Log($"为玩家 '{targetPlayerIdentifier}' 设置: {settings.Trim()}。");
                    // TODO: 实现栏位设置的逻辑
                    break;

                case TrailOperationType.信息:
                    if (槽位 == -1)
                    {
                        Debug.Log($"显示玩家 '{targetPlayerIdentifier}' 的所有尾迹信息。");
                        // TODO: 实现显示所有尾迹信息的逻辑
                    }
                    else
                    {
                        Debug.Log($"显示玩家 '{targetPlayerIdentifier}' 槽位 {槽位} 的尾迹详细信息。");
                        // TODO: 实现显示指定槽位尾迹信息的逻辑
                    }
                    break;
            }
        }
    }
} 