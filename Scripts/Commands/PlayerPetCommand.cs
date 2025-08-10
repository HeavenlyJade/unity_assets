using UnityEngine;
using MiGame.Commands;
using MiGame.Pet;

namespace MiGame.Commands
{
    /// <summary>
    /// 宠物操作类型
    /// </summary>
    public enum PetOperationType
    {
        新增,
        删除,
        设置,
        栏位设置,
        栏位新增,
        栏位减少
    }

    /// <summary>
    /// 宠物操作指令
    /// </summary>
    [Command("pet", "宠物相关操作")]
    public class PlayerPetCommand : AbstractCommand
    {
        [CommandParamDesc("操作类型 (新增/删除/设置/栏位设置/栏位新增/栏位减少)")]
        public PetOperationType 操作类型;

        [CommandParamDesc("目标玩家的UIN")]
        public string 玩家UID;

        [CommandParamDesc("目标玩家的名称")]
        public string 玩家;

        [ConditionalField("操作类型", PetOperationType.新增)]
        [CommandParamDesc("要新增的宠物 (从配置选择)")]
        public PetConfig 宠物;

        [ConditionalField("操作类型", PetOperationType.设置)]
        [CommandParamDesc("设置宠物的目标等级 (仅设置时有效)")]
        public int 等级 = -1;

        [ConditionalField("操作类型", PetOperationType.设置)]
        [CommandParamDesc("设置宠物的目标星级 (仅设置时有效)")]
        public int 星级 = -1;
        
        [ConditionalField("操作类型", PetOperationType.栏位设置)]
        [CommandParamDesc("设置玩家可以同时装备出战的宠物数量")]
        public int 可携带 = -1;

        [ConditionalField("操作类型", PetOperationType.栏位设置)]
        [CommandParamDesc("设置玩家宠物背包的总容量")]
        public int 背包 = -1;

        [ConditionalField("操作类型", PetOperationType.栏位新增)]
        [CommandParamDesc("新增可携带栏位数量")]
        public int 新增可携带 = 0;

        [ConditionalField("操作类型", PetOperationType.栏位新增)]
        [CommandParamDesc("新增背包容量")]
        public int 新增背包 = 0;

        [ConditionalField("操作类型", PetOperationType.栏位减少)]
        [CommandParamDesc("减少可携带栏位数量")]
        public int 减少可携带 = 0;

        [ConditionalField("操作类型", PetOperationType.栏位减少)]
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
                case PetOperationType.新增:
                    if (宠物 == null)
                    {
                        Debug.LogError("新增宠物失败：必须选择一个宠物配置。");
                        return;
                    }
                    Debug.Log($"向玩家 '{targetPlayerIdentifier}' 新增宠物 '{宠物.name}'。");
                    // TODO: 实现新增宠物的逻辑
                    break;

                case PetOperationType.删除:
                    Debug.Log($"从玩家 '{targetPlayerIdentifier}' 删除宠物。");
                    // TODO: 实现删除宠物的逻辑
                    break;

                case PetOperationType.设置:
                    if (等级 == -1 && 星级 == -1)
                    {
                        Debug.LogError("设置宠物属性失败：必须至少提供等级或星级中的一个。");
                        return;
                    }

                    string changes = "";
                    if (等级 != -1) changes += $"等级设置为 {等级} ";
                    if (星级 != -1) changes += $"星级提升到 {星级} ";
                    Debug.Log($"设置玩家 '{targetPlayerIdentifier}' 的宠物属性: {changes.Trim()}。");
                    // TODO: 实现设置宠物属性的逻辑
                    break;
                
                case PetOperationType.栏位设置:
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

                case PetOperationType.栏位新增:
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

                case PetOperationType.栏位减少:
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