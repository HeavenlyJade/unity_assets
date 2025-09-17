using UnityEngine;
using MiGame.Commands;
using MiGame.Pet;

namespace MiGame.Commands
{
    /// <summary>
    /// 翅膀操作类型
    /// </summary>
    public enum WingOperationType
    {
        新增,
        删除,
        设置,
        栏位设置,
        栏位新增,
        栏位减少
    }

    /// <summary>
    /// 翅膀操作指令
    /// </summary>
    [Command("wing", "翅膀相关操作")]
    public class PlayerWingCommand : AbstractCommand
    {
        [CommandParamDesc("操作类型 (新增/删除/设置/栏位设置)")]
        public WingOperationType 操作类型;

        [CommandParamDesc("目标玩家的UIN")]
        public string 玩家UID;

        [CommandParamDesc("目标玩家的名称")]
        public string 玩家;

        [CommandParamDesc("要操作的翅膀所在的槽位索引")]
        public int 槽位 = -1;

        [ConditionalField("操作类型", WingOperationType.新增)]
        [CommandParamDesc("要新增的翅膀 (从配置选择)")]
        public WingConfig 翅膀;

        [ConditionalField("操作类型", WingOperationType.设置)]
        [CommandParamDesc("设置翅膀的目标等级 (仅设置时有效)")]
        public int 等级 = -1;

        [ConditionalField("操作类型", WingOperationType.设置)]
        [CommandParamDesc("设置翅膀的目标星级 (仅设置时有效)")]
        public int 星级 = -1;
        
        [ConditionalField("操作类型", WingOperationType.栏位设置)]
        [CommandParamDesc("设置玩家可以同时装备的翅膀数量")]
        public int 可携带 = -1;

        [ConditionalField("操作类型", WingOperationType.栏位设置)]
        [CommandParamDesc("设置玩家翅膀背包的总容量")]
        public int 背包 = -1;

        [ConditionalField("操作类型", WingOperationType.栏位新增)]
        [CommandParamDesc("新增可携带栏位数量")]
        public int 新增可携带 = 0;

        [ConditionalField("操作类型", WingOperationType.栏位新增)]
        [CommandParamDesc("新增背包容量")]
        public int 新增背包 = 0;

        [ConditionalField("操作类型", WingOperationType.栏位减少)]
        [CommandParamDesc("减少可携带栏位数量")]
        public int 减少可携带 = 0;

        [ConditionalField("操作类型", WingOperationType.栏位减少)]
        [CommandParamDesc("减少背包容量")]
        public int 减少背包 = 0;

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
                case WingOperationType.新增:
                    if (翅膀 == null)
                    {
                        Debug.LogError("新增翅膀失败：必须选择一个翅膀配置。");
                        return;
                    }
                    Debug.Log($"向玩家 '{targetPlayerIdentifier}' 新增翅膀 '{翅膀.name}'，槽位: {(槽位 == -1 ? "自动" : 槽位.ToString())}。");
                    // TODO: 实现新增翅膀的逻辑
                    break;

                case WingOperationType.删除:
                    if (槽位 == -1)
                    {
                        Debug.LogError("删除翅膀失败：必须提供槽位索引。");
                        return;
                    }
                    if (string.IsNullOrEmpty(玩家UID))
                    {
                        Debug.LogError("删除翅膀失败：必须提供玩家UID。");
                        return;
                    }
                    Debug.Log($"从玩家 '{targetPlayerIdentifier}' 删除槽位 {槽位} 的翅膀。");
                    // TODO: 实现删除翅膀的逻辑
                    break;

                case WingOperationType.设置:
                    if (槽位 == -1)
                    {
                        Debug.LogError("设置翅膀属性失败：必须提供槽位索引。");
                        return;
                    }
                    if (等级 == -1 && 星级 == -1)
                    {
                        Debug.LogError("设置翅膀属性失败：必须至少提供等级或星级中的一个。");
                        return;
                    }

                    string changes = "";
                    if (等级 != -1) changes += $"等级设置为 {等级} ";
                    if (星级 != -1) changes += $"星级提升到 {星级} ";
                    Debug.Log($"设置玩家 '{targetPlayerIdentifier}' 槽位 {槽位} 的翅膀属性: {changes.Trim()}。");
                    // TODO: 实现设置翅膀属性的逻辑
                    break;
                
                case WingOperationType.栏位设置:
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

                case WingOperationType.栏位新增:
                    if (新增可携带 <= 0 && 新增背包 <= 0)
                    {
                        Debug.LogError("栏位新增失败：必须至少提供 '新增可携带' 或 '新增背包' 中的一个，并且值必须大于0。");
                        return;
                    }

                    string addSettings = "";
                    if (新增可携带 > 0) addSettings += $"新增可携带栏位 {新增可携带} ";
                    if (新增背包 > 0) addSettings += $"新增背包容量 {新增背包} ";
                    Debug.Log($"为玩家 '{targetPlayerIdentifier}' {addSettings.Trim()}。");
                    // TODO: 实现栏位新增的逻辑
                    break;

                case WingOperationType.栏位减少:
                    if (减少可携带 <= 0 && 减少背包 <= 0)
                    {
                        Debug.LogError("栏位减少失败：必须至少提供 '减少可携带' 或 '减少背包' 中的一个，并且值必须大于0。");
                        return;
                    }

                    string reduceSettings = "";
                    if (减少可携带 > 0) reduceSettings += $"减少可携带栏位 {减少可携带} ";
                    if (减少背包 > 0) reduceSettings += $"减少背包容量 {减少背包} ";
                    Debug.Log($"为玩家 '{targetPlayerIdentifier}' {reduceSettings.Trim()}。");
                    // TODO: 实现栏位减少的逻辑
                    break;
            }
        }
    }
} 