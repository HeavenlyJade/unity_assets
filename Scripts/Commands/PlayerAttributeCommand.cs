using UnityEngine;
using MiGame;
using System.Collections.Generic;
using MiGame.Core;

namespace MiGame.Commands
{
    /// <summary>
    /// 玩家属性操作类型枚举
    /// </summary>
    public enum AttributeOperationType
    {
        新增,      // 增加属性值
        设置,      // 设置属性值为指定数值
        减少,      // 减少属性值
        查看,      // 查看属性值
        恢复,      // 恢复属性值到默认状态
        刷新,      // 刷新属性加成效果
        测试加成,  // 测试属性加成效果
        仅加成新增 // 仅增加加成效果，不修改基础属性值
    }

    /// <summary>
    /// 属性来源枚举
    /// </summary>
    public enum AttributeSource
    {
        装备,      // 装备提供的属性
        任务,      // 任务奖励的属性
        技能,      // 技能效果的属性
        COMMAND    // 指令直接操作的属性
    }

    /// <summary>
    /// 玩家属性操作指令
    /// 支持对玩家属性进行各种操作，包括数值修改、加成测试等
    /// </summary>
    [Command("attribute", "用于操作玩家属性的指令")]
    public class PlayerAttributeCommand : AbstractCommand
    {
        [Header("基础参数")]
        [Tooltip("操作类型")]
        public AttributeOperationType 操作类型;

        [Tooltip("目标玩家UID")]
        public string 玩家UID;

        [Tooltip("属性名称")]
        [PlayerAttributeName]
        public string 属性名;

        [Tooltip("数值（支持整数、小数、长整型）")]
        public string 数值;

        [Tooltip("最终倍率")]
        public float 最终倍率 = 1.0f;

        [Tooltip("属性来源")]
        public AttributeSource 来源 = AttributeSource.COMMAND;

        [Header("加成配置")]
        [Tooltip("玩家属性加成列表")]
        [BonusType(VariableNameType.Stat)]
        public List<PlayerBonus> 玩家属性加成;

        [Tooltip("玩家变量加成列表")]
        [BonusType(VariableNameType.Variable)]
        public List<PlayerBonus> 玩家变量加成;

        [Tooltip("其他加成类型（宠物、伙伴、尾迹、翅膀等）")]
        [SerializeField]
        public OtherBonusList 其他加成 = new OtherBonusList();

        public override void Execute()
        {
            // 基础参数验证
            if (string.IsNullOrEmpty(玩家UID))
            {
                Debug.LogError("玩家UID 不能为空");
                return;
            }

            if (操作类型 != AttributeOperationType.查看 && string.IsNullOrEmpty(属性名))
            {
                Debug.LogError("属性名 不能为空");
                return;
            }

            // 数值类型判断和转换
            object 转换后数值 = null;
            if (!string.IsNullOrEmpty(数值))
            {
                转换后数值 = ParseNumericValue(数值);
                if (转换后数值 == null)
                {
                    return; // 解析失败，错误已在ParseNumericValue中输出
                }
            }

            // 执行具体操作
            ExecuteAttributeOperation(转换后数值);
        }

        /// <summary>
        /// 解析数值字符串为对应的数值类型
        /// </summary>
        private object ParseNumericValue(string valueString)
        {
            // 尝试解析为长整型
            if (long.TryParse(valueString, out long longValue))
            {
                Debug.Log($"数值 {valueString} 已转换为长整型: {longValue}");
                return longValue;
            }
            
            // 尝试解析为双精度浮点型
            if (double.TryParse(valueString, out double doubleValue))
            {
                Debug.Log($"数值 {valueString} 已转换为浮点型: {doubleValue}");
                return doubleValue;
            }
            
            // 尝试解析为单精度浮点型
            if (float.TryParse(valueString, out float floatValue))
            {
                Debug.Log($"数值 {valueString} 已转换为单精度浮点型: {floatValue}");
                return floatValue;
            }
            
            // 尝试解析为整数
            if (int.TryParse(valueString, out int intValue))
            {
                Debug.Log($"数值 {valueString} 已转换为整数: {intValue}");
                return intValue;
            }

            Debug.LogError($"无法解析数值: {valueString}，请输入有效的数字格式");
            return null;
        }

        /// <summary>
        /// 执行属性操作
        /// </summary>
        private void ExecuteAttributeOperation(object numericValue)
        {
            string 玩家标识 = $"玩家 {玩家UID}";
            string 属性标识 = string.IsNullOrEmpty(属性名) ? "所有属性" : 属性名;
            string 来源标识 = 来源.ToString();

            switch (操作类型)
            {
                case AttributeOperationType.新增:
                    ExecuteAddOperation(玩家标识, 属性标识, numericValue, 来源标识);
                    break;
                    
                case AttributeOperationType.设置:
                    ExecuteSetOperation(玩家标识, 属性标识, numericValue, 来源标识);
                    break;
                    
                case AttributeOperationType.减少:
                    ExecuteReduceOperation(玩家标识, 属性标识, numericValue, 来源标识);
                    break;
                    
                case AttributeOperationType.查看:
                    ExecuteViewOperation(玩家标识, 属性标识);
                    break;
                    
                case AttributeOperationType.恢复:
                    ExecuteRestoreOperation(玩家标识, 属性标识);
                    break;
                    
                case AttributeOperationType.刷新:
                    ExecuteRefreshOperation(玩家标识, 属性标识);
                    break;
                    
                case AttributeOperationType.测试加成:
                    ExecuteTestBonusOperation(玩家标识, 属性标识);
                    break;
                    
                case AttributeOperationType.仅加成新增:
                    ExecuteBonusOnlyAddOperation(玩家标识, 属性标识, numericValue, 来源标识);
                    break;
                    
                default:
                    Debug.LogError($"未知的操作类型：{操作类型}");
                    break;
            }
        }

        /// <summary>
        /// 执行新增操作
        /// </summary>
        private void ExecuteAddOperation(string playerId, string attributeId, object value, string source)
        {
            Debug.Log($"{playerId} 新增属性: {attributeId}, 数值: +{value} (来源: {source})");
            
            // 应用加成效果
            ApplyBonusEffects(playerId, attributeId, value, source);
            
            // TODO: 调用游戏逻辑，增加玩家属性值
            Debug.Log($"属性新增操作完成: {attributeId} +{value}");
        }

        /// <summary>
        /// 执行设置操作
        /// </summary>
        private void ExecuteSetOperation(string playerId, string attributeId, object value, string source)
        {
            Debug.Log($"{playerId} 设置属性: {attributeId}, 数值: {value} (来源: {source})");
            
            // 应用加成效果
            ApplyBonusEffects(playerId, attributeId, value, source);
            
            // TODO: 调用游戏逻辑，设置玩家属性值
            Debug.Log($"属性设置操作完成: {attributeId} = {value}");
        }

        /// <summary>
        /// 执行减少操作
        /// </summary>
        private void ExecuteReduceOperation(string playerId, string attributeId, object value, string source)
        {
            Debug.Log($"{playerId} 减少属性: {attributeId}, 数值: -{value} (来源: {source})");
            
            // 应用加成效果
            ApplyBonusEffects(playerId, attributeId, value, source);
            
            // TODO: 调用游戏逻辑，减少玩家属性值
            Debug.Log($"属性减少操作完成: {attributeId} -{value}");
        }

        /// <summary>
        /// 执行查看操作
        /// </summary>
        private void ExecuteViewOperation(string playerId, string attributeId)
        {
            if (string.IsNullOrEmpty(属性名))
            {
                Debug.Log($"{playerId} 查看所有属性");
                // TODO: 调用游戏逻辑，获取所有属性值
            }
            else
            {
                Debug.Log($"{playerId} 查看属性: {attributeId}");
                // TODO: 调用游戏逻辑，获取指定属性值
            }
        }

        /// <summary>
        /// 执行恢复操作
        /// </summary>
        private void ExecuteRestoreOperation(string playerId, string attributeId)
        {
            Debug.Log($"{playerId} 恢复属性: {attributeId} 到默认状态");
            
            // TODO: 调用游戏逻辑，恢复属性到默认值
            Debug.Log($"属性恢复操作完成: {attributeId} 已恢复到默认状态");
        }

        /// <summary>
        /// 执行刷新操作
        /// </summary>
        private void ExecuteRefreshOperation(string playerId, string attributeId)
        {
            Debug.Log($"{playerId} 刷新属性: {attributeId} 的加成效果");
            
            // 重新计算所有加成效果
            RecalculateBonusEffects(playerId, attributeId);
            
            // TODO: 调用游戏逻辑，刷新属性加成
            Debug.Log($"属性刷新操作完成: {attributeId} 加成效果已刷新");
        }

        /// <summary>
        /// 执行测试加成操作
        /// </summary>
        private void ExecuteTestBonusOperation(string playerId, string attributeId)
        {
            Debug.Log($"{playerId} 测试属性: {attributeId} 的加成效果");
            
            // 显示当前加成配置
            DisplayBonusConfiguration();
            
            // 模拟加成效果计算
            SimulateBonusCalculation(playerId, attributeId);
            
            Debug.Log($"属性加成测试完成: {attributeId}");
        }

        /// <summary>
        /// 应用加成效果
        /// </summary>
        private void ApplyBonusEffects(string playerId, string attributeId, object value, string source)
        {
            if (玩家属性加成 != null && 玩家属性加成.Count > 0)
            {
                Debug.Log($"{playerId} 应用属性加成: {玩家属性加成.Count} 项");
                foreach (var bonus in 玩家属性加成)
                {
                    Debug.Log($"  - {bonus.Name}: 计算方式={bonus.Calculation}, 倍率={bonus.缩放倍率}");
                }
            }

            if (玩家变量加成 != null && 玩家变量加成.Count > 0)
            {
                Debug.Log($"{playerId} 应用变量加成: {玩家变量加成.Count} 项");
                foreach (var bonus in 玩家变量加成)
                {
                    Debug.Log($"  - {bonus.Name}: 计算方式={bonus.Calculation}, 倍率={bonus.缩放倍率}");
                }
            }

            if (其他加成 != null && 其他加成.items.Count > 0)
            {
                Debug.Log($"{playerId} 应用其他加成: {string.Join(", ", 其他加成.items)}");
            }
        }

        /// <summary>
        /// 重新计算加成效果
        /// </summary>
        private void RecalculateBonusEffects(string playerId, string attributeId)
        {
            Debug.Log($"重新计算 {playerId} 的 {attributeId} 加成效果");
            
            // TODO: 实现加成效果重新计算逻辑
            // 1. 清除旧的加成效果
            // 2. 重新应用所有加成
            // 3. 更新最终属性值
        }

        /// <summary>
        /// 显示加成配置
        /// </summary>
        private void DisplayBonusConfiguration()
        {
            Debug.Log("=== 当前加成配置 ===");
            
            if (玩家属性加成 != null && 玩家属性加成.Count > 0)
            {
                Debug.Log("玩家属性加成:");
                for (int i = 0; i < 玩家属性加成.Count; i++)
                {
                    var bonus = 玩家属性加成[i];
                    Debug.Log($"  [{i + 1}] {bonus.Name} - {bonus.Calculation} x{bonus.缩放倍率}");
                }
            }
            else
            {
                Debug.Log("玩家属性加成: 无");
            }

            if (玩家变量加成 != null && 玩家变量加成.Count > 0)
            {
                Debug.Log("玩家变量加成:");
                for (int i = 0; i < 玩家变量加成.Count; i++)
                {
                    var bonus = 玩家变量加成[i];
                    Debug.Log($"  [{i + 1}] {bonus.Name} - {bonus.Calculation} x{bonus.缩放倍率}");
                }
            }
            else
            {
                Debug.Log("玩家变量加成: 无");
            }

            if (其他加成 != null && 其他加成.items.Count > 0)
            {
                Debug.Log($"其他加成: [{string.Join(", ", 其他加成.items)}]");
            }
            else
            {
                Debug.Log("其他加成: 无");
            }
        }

        /// <summary>
        /// 执行仅加成新增操作
        /// </summary>
        private void ExecuteBonusOnlyAddOperation(string playerId, string attributeId, object value, string source)
        {
            Debug.Log($"{playerId} 仅加成新增: {attributeId}, 数值: +{value} (来源: {source})");
            
            // 仅应用加成效果，不修改基础属性值
            ApplyBonusEffects(playerId, attributeId, value, source);
            
            // 显示当前加成配置
            DisplayBonusConfiguration();
            
            // TODO: 调用游戏逻辑，仅增加加成效果，不修改基础属性值
            Debug.Log($"仅加成新增操作完成: {attributeId} 的加成效果已增加，基础属性值未改变");
        }

        /// <summary>
        /// 模拟加成效果计算
        /// </summary>
        private void SimulateBonusCalculation(string playerId, string attributeId)
        {
            Debug.Log($"=== {playerId} {attributeId} 加成效果模拟 ===");
            
            // 模拟基础属性值
            double 基础值 = 100.0;
            Debug.Log($"基础值: {基础值}");
            
            // 模拟属性加成计算
            if (玩家属性加成 != null && 玩家属性加成.Count > 0)
            {
                double 属性加成总值 = 0;
                foreach (var bonus in 玩家属性加成)
                {
                    double 加成值 = 50.0 * bonus.缩放倍率; // 模拟加成值
                    属性加成总值 += 加成值;
                    Debug.Log($"属性加成: {bonus.Name} = {加成值} ({bonus.Calculation})");
                }
                Debug.Log($"属性加成总值: {属性加成总值}");
                基础值 += 属性加成总值;
            }
            
            // 模拟变量加成计算
            if (玩家变量加成 != null && 玩家变量加成.Count > 0)
            {
                double 变量加成总值 = 0;
                foreach (var bonus in 玩家变量加成)
                {
                    double 加成值 = 30.0 * bonus.缩放倍率; // 模拟加成值
                    变量加成总值 += 加成值;
                    Debug.Log($"变量加成: {bonus.Name} = {加成值} ({bonus.Calculation})");
                }
                Debug.Log($"变量加成总值: {变量加成总值}");
                基础值 += 变量加成总值;
            }
            
            // 模拟其他加成效果
            if (其他加成 != null && 其他加成.items.Count > 0)
            {
                double 其他加成总值 = 其他加成.items.Count * 20.0; // 每个其他加成提供20点
                Debug.Log($"其他加成总值: {其他加成总值} ({string.Join(", ", 其他加成.items)})");
                基础值 += 其他加成总值;
            }
            
            Debug.Log($"最终属性值: {基础值}");
            Debug.Log("=== 加成效果模拟完成 ===");
        }
    }
}
