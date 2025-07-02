using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;

public enum SkillRank { 
    UR,
    SSR,
    SR,
    R
}

public enum 目标模式 {
    自己,     // 作用于自身
    敌人,     // 作用于敌对目标
    友军,     // 作用于友方目标
    地点,     // 作用于指定地点
    全体      // 作用于所有目标
}

// 消耗类型枚举 - 简化为两大类
public enum 消耗类型
{
    玩家属性,
    物品
}

// 前置条件类型枚举
public enum 前置条件类型
{
    等级需求,
    消耗条件
}

// 玩家属性类型枚举
public enum 玩家属性类型
{
    生命值,
    魔法值,
    经验值
}

// 消耗方式枚举
public enum 消耗方式
{
    数值消耗,
    百分比消耗
}

// 选择类型枚举
public enum 选择类型 {
    单体,   // 单个目标
    范围,   // 范围内所有目标
    链式,   // 跳跃到多个目标
    随机,   // 随机选择目标
    全部,   // 所有符合条件的目标
    扇形,   // 扇形范围内目标
    直线    // 直线路径上目标
}

// 重新设计的前置条件结构体 - 整合等级需求和消耗条件
[System.Serializable]
public struct 前置条件
{
    [Header("条件类型")]
    public 前置条件类型 条件类型;
    
    [Header("等级需求")]
    [ConditionalField("条件类型", 前置条件类型.等级需求)]
    public int 需求玩家等级;
    
    [ConditionalField("条件类型", 前置条件类型.等级需求)]
    public int 需求技能等级;
    
    [Header("消耗类型")]
    [ConditionalField("条件类型", 前置条件类型.消耗条件)]
    public 消耗类型 消耗类型;
    
    [Header("玩家属性消耗")]
    [ConditionalField(new[] { "条件类型", "消耗类型" }, new object[] { 前置条件类型.消耗条件, 消耗类型.玩家属性 })]
    public 玩家属性类型 属性类型;
    
    [ConditionalField(new[] { "条件类型", "消耗类型" }, new object[] { 前置条件类型.消耗条件, 消耗类型.玩家属性 })]
    public 消耗方式 消耗方式;
    
    [ConditionalField(new[] { "条件类型", "消耗类型" }, new object[] { 前置条件类型.消耗条件, 消耗类型.玩家属性 })]
    public int 消耗数值;
    
    [ConditionalField(new[] { "条件类型", "消耗类型" }, new object[] { 前置条件类型.消耗条件, 消耗类型.玩家属性 })]
    [Range(0f, 100f)]
    public float 消耗百分比;
    
    [Header("物品消耗")]
    [ConditionalField(new[] { "条件类型", "消耗类型" }, new object[] { 前置条件类型.消耗条件, 消耗类型.物品 })]
    public string 物品ID;
    
    [ConditionalField(new[] { "条件类型", "消耗类型" }, new object[] { 前置条件类型.消耗条件, 消耗类型.物品 })]
    public string 物品名称;
    
    [ConditionalField(new[] { "条件类型", "消耗类型" }, new object[] { 前置条件类型.消耗条件, 消耗类型.物品 })]
    public int 物品数量;
    
    /// <summary>
    /// 获取玩家属性的实际消耗数值
    /// </summary>
    /// <param name="当前值">当前属性值</param>
    /// <param name="最大值">最大属性值（用于百分比计算）</param>
    /// <returns>实际消耗数值</returns>
    public int 获取属性消耗数值(int 当前值, int 最大值)
    {
        if (条件类型 != 前置条件类型.消耗条件 || 消耗类型 != 消耗类型.玩家属性)
            return 0;
            
        switch (消耗方式)
        {
            case 消耗方式.数值消耗:
                return 消耗数值;
                
            case 消耗方式.百分比消耗:
                // 对于经验值，百分比基于当前值；对于生命值和魔法值，基于最大值
                int 基准值 = (属性类型 == 玩家属性类型.经验值) ? 当前值 : 最大值;
                return Mathf.RoundToInt(基准值 * (消耗百分比 / 100f));
                
            default:
                return 0;
        }
    }
    
    /// <summary>
    /// 获取条件描述文本
    /// </summary>
    public string 获取描述()
    {
        switch (条件类型)
        {
            case 前置条件类型.等级需求:
                return 获取等级需求描述();
                
            case 前置条件类型.消耗条件:
                return 获取消耗描述();
                
            default:
                return "未知条件";
        }
    }
    
    /// <summary>
    /// 获取等级需求描述
    /// </summary>
    private string 获取等级需求描述()
    {
        if (需求玩家等级 <= 0 && 需求技能等级 <= 0)
            return "无等级需求";
            
        string 描述 = "";
        if (需求玩家等级 > 0)
            描述 += $"玩家等级: {需求玩家等级}";
        if (需求技能等级 > 0)
        {
            if (!string.IsNullOrEmpty(描述))
                描述 += " | ";
            描述 += $"技能等级: {需求技能等级}";
        }
        
        return 描述;
    }
    
    /// <summary>
    /// 获取消耗描述文本
    /// </summary>
    private string 获取消耗描述()
    {
        switch (消耗类型)
        {
            case 消耗类型.玩家属性:
                string 属性名 = 获取属性显示名();
                if (消耗方式 == 消耗方式.百分比消耗)
                {
                    return $"{属性名}: {消耗百分比}%";
                }
                else
                {
                    return $"{属性名}: {消耗数值}";
                }
                
            case 消耗类型.物品:
                return $"{物品名称} x{物品数量}";
                
            default:
                return "未知消耗";
        }
    }
    
    /// <summary>
    /// 获取属性显示名称
    /// </summary>
    private string 获取属性显示名()
    {
        switch (属性类型)
        {
            case 玩家属性类型.生命值:
                return "HP";
            case 玩家属性类型.魔法值:
                return "MP";
            case 玩家属性类型.经验值:
                return "EXP";
            default:
                return 属性类型.ToString();
        }
    }
}



[System.Serializable]
public struct RecoilConfig
{
    public float 垂直后坐力;
    public float 最大垂直后坐力;
    public float 垂直后坐力恢复;
    public float 水平后坐力;
    public float 最大水平后坐力;
    public float 水平后坐力恢复;
    public float 后坐力冷却时间;

    public static RecoilConfig Default => new RecoilConfig
    {
        垂直后坐力 = 0.3f,
        最大垂直后坐力 = 2f,
        垂直后坐力恢复 = 3f,
        水平后坐力 = 0.2f,
        最大水平后坐力 = 1f,
        水平后坐力恢复 = 2f,
        后坐力冷却时间 = 0f
    };
}

// 玩家属性数据结构
[System.Serializable]
public struct 玩家属性数据
{
    public int 当前生命值;
    public int 最大生命值;
    public int 当前魔法值;
    public int 最大魔法值;
    public int 当前经验值;
    public int 玩家等级;
}

[System.Serializable]
public struct 升级素材
{
    public string 物品ID;
    public string 物品名称;
    public int 物品数量;
}

// 临时的占位符属性
public class ServerNameAttrAttribute : PropertyAttribute { }

public class ConditionalFieldAttribute : PropertyAttribute { 
    public string fieldName;
    public object value;
    public string[] fieldNames;
    public object[] values;
    
    // 单条件构造函数
    public ConditionalFieldAttribute(string fieldName, object value) {
        this.fieldName = fieldName;
        this.value = value;
    }
    
    // 多条件构造函数
    public ConditionalFieldAttribute(string[] fieldNames, object[] values) {
        this.fieldNames = fieldNames;
        this.values = values;
    }
}

[CreateAssetMenu(fileName = "Skill", menuName = "Battle/Skill")]
public class Skill : ScriptableObject
{
    [ServerNameAttr]
    public string 技能名;
    public string 显示名;
    [ExportToXls]
    public int 最大等级;
    [TextArea(1, 8)]
    public string 技能描述;
    [TextArea(1, 8)]
    public string 技能详细;
    public string 技能图标;
    public string 技能小角标;
    [ExportToXls]
    [XlsSortingOrder(10)]
    public SkillRank 技能品级;
    public bool 是入口技能;
    public int 技能分类;
    [TextArea(1, 3)]
    public string 提升玩家等级;
    public Skill[] 下一技能;
    public bool 无需装备也可生效 = false;
    
    [Header("前置条件")]
    [ExportToXls]
    public 前置条件[] 前置条件;
    public float 冷却时间 = 0f;
    
    [Header("前摇阶段")]
    public 施法类型 施法类型 = 施法类型.瞬发;
    
    [ConditionalField("施法类型", 施法类型.蓄力)]
    public float 施法时间 = 3.0f;
    
    [ConditionalField("施法类型", 施法类型.蓄力)]
    public bool 可被打断 = true;

    [ConditionalField("施法类型", 施法类型.蓄力)]
    public bool 禁止移动 = true;

    [Header("效果计算")]
    [Tooltip("伤害数值或公式")]
    public string 基础伤害;
    [Tooltip("治疗数值或公式")]
    public string 基础治疗;
    public 伤害类型 伤害类型;
    public 元素类型 元素类型;
    public 属性修改[] 属性修改;
    [Tooltip("上方所有属性修改效果的统一持续时间")]
    public float 属性修改持续时间;

    [Header("升级")]
    [ExportToXls]
    public string 最大经验;
    public 升级素材[] 升级所需素材;
    
    [Header("主动技能")]
    [ExportToXls]
    [XlsSortingOrder(100)]
    public 目标模式 目标模式;
    [ExportToXls]
    public 选择类型 选择类型;
    
    [Header("目标选择参数")]
    public float 最大距离 = 100f;
    public int 最大目标数 = 1;

    [ConditionalField("选择类型", 选择类型.范围)]
    public float 范围半径 = 30f;

    [ConditionalField("选择类型", 选择类型.链式)]
    public float 链式距离 = 50f;

    [ConditionalField("选择类型", 选择类型.随机)]
    public float 随机半径 = 200f;

    [ConditionalField("选择类型", 选择类型.扇形)]
    public float 扇形角度 = 60f;

    [ConditionalField("选择类型", 选择类型.扇形)]
    public float 扇形半径 = 120f;

    [ConditionalField("选择类型", 选择类型.直线)]
    public float 直线宽度 = 20f;
    [ConditionalField("目标模式", 目标模式.自己)]
    public Vector3 位置偏移;
    public bool 启用后坐力;
    [ConditionalField("启用后坐力", true)]
    public RecoilConfig 后坐力 = RecoilConfig.Default;
    
    [Header("特效与音效")]
    public EffectConfig 技能特效;
    
    [Header("装备时更改外形")]
    public string 更改模型;
    public string 更改动画;
    public RuntimeAnimatorController 更改状态机 = null;
    public float 更改玩家尺寸 = 1;
    public Vector3 副卡挂机尺寸 = Vector3.one;
    public float 指示器半径 = 3;
    
    /// <summary>
    /// 检查玩家是否满足技能的所有前置条件
    /// </summary>
    /// <param name="玩家数据">玩家属性数据</param>
    /// <param name="物品检查函数">检查物品的委托函数</param>
    /// <returns>是否满足条件</returns>
    public bool 检查前置条件(玩家属性数据 玩家数据, System.Func<string, int, bool> 物品检查函数 = null)
    {
        if (前置条件 == null || 前置条件.Length == 0)
            return true;
            
        foreach (var 条件 in 前置条件)
        {
            switch (条件.条件类型)
            {
                case 前置条件类型.等级需求:
                    if (!检查等级需求(条件, 玩家数据))
                        return false;
                    break;
                    
                case 前置条件类型.消耗条件:
                    if (!检查消耗条件(条件, 玩家数据, 物品检查函数))
                        return false;
                    break;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查等级需求是否满足
    /// </summary>
    private bool 检查等级需求(前置条件 条件, 玩家属性数据 玩家数据)
    {
        if (玩家数据.玩家等级 < 条件.需求玩家等级)
        {
            Debug.Log($"技能 {技能名} 需要玩家等级 {条件.需求玩家等级}，当前等级 {玩家数据.玩家等级}");
            return false;
        }
        
        // 这里可以添加技能等级检查逻辑
        // if (当前技能等级 < 条件.需求技能等级)
        // {
        //     Debug.Log($"技能 {技能名} 需要技能等级 {条件.需求技能等级}，当前技能等级 {当前技能等级}");
        //     return false;
        // }
        
        return true;
    }
    
    /// <summary>
    /// 检查消耗条件是否满足
    /// </summary>
    private bool 检查消耗条件(前置条件 条件, 玩家属性数据 玩家数据, System.Func<string, int, bool> 物品检查函数)
    {
        switch (条件.消耗类型)
        {
            case 消耗类型.玩家属性:
                return 检查玩家属性消耗(条件, 玩家数据);
                
            case 消耗类型.物品:
                return 检查物品消耗(条件, 物品检查函数);
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 检查玩家属性消耗是否满足
    /// </summary>
    private bool 检查玩家属性消耗(前置条件 条件, 玩家属性数据 玩家数据)
    {
        int 当前值 = 0;
        int 最大值 = 0;
        string 属性名 = "";
        
        switch (条件.属性类型)
        {
            case 玩家属性类型.生命值:
                当前值 = 玩家数据.当前生命值;
                最大值 = 玩家数据.最大生命值;
                属性名 = "HP";
                break;
                
            case 玩家属性类型.魔法值:
                当前值 = 玩家数据.当前魔法值;
                最大值 = 玩家数据.最大魔法值;
                属性名 = "MP";
                break;
                
            case 玩家属性类型.经验值:
                当前值 = 玩家数据.当前经验值;
                最大值 = 玩家数据.当前经验值; // 经验值的最大值就是当前值
                属性名 = "EXP";
                break;
        }
        
        int 需要消耗 = 条件.获取属性消耗数值(当前值, 最大值);
        
        if (当前值 < 需要消耗)
        {
            Debug.Log($"技能 {技能名} 需要 {需要消耗} {属性名}，当前 {属性名} {当前值}");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查物品消耗是否满足
    /// </summary>
    private bool 检查物品消耗(前置条件 条件, System.Func<string, int, bool> 物品检查函数)
    {
        if (物品检查函数 == null)
        {
            Debug.LogWarning($"技能 {技能名} 需要物品检查函数，但未提供");
            return false;
        }
        
        bool 满足条件 = 物品检查函数(条件.物品ID, 条件.物品数量);
        
        if (!满足条件)
        {
            Debug.Log($"技能 {技能名} 需要物品：{条件.物品名称} x{条件.物品数量}");
        }
        
        return 满足条件;
    }
    
    /// <summary>
    /// 执行技能消耗
    /// </summary>
    /// <param name="玩家数据">玩家属性数据（引用传递，会被修改）</param>
    /// <param name="物品消耗函数">执行物品消耗的委托函数</param>
    /// <returns>是否成功执行消耗</returns>
    public bool 执行消耗(ref 玩家属性数据 玩家数据, System.Action<string, int> 物品消耗函数 = null)
    {
        if (前置条件 == null || 前置条件.Length == 0)
            return true;
            
        // 先检查是否满足条件
        if (!检查前置条件(玩家数据, (id, count) => 物品消耗函数 != null))
        {
            return false;
        }
        
        // 执行消耗
        foreach (var 条件 in 前置条件)
        {
            if (条件.条件类型 == 前置条件类型.消耗条件)
            {
                switch (条件.消耗类型)
                {
                    case 消耗类型.玩家属性:
                        执行玩家属性消耗(条件, ref 玩家数据);
                        break;
                        
                    case 消耗类型.物品:
                        物品消耗函数?.Invoke(条件.物品ID, 条件.物品数量);
                        break;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 执行玩家属性消耗
    /// </summary>
    private void 执行玩家属性消耗(前置条件 条件, ref 玩家属性数据 玩家数据)
    {
        switch (条件.属性类型)
        {
            case 玩家属性类型.生命值:
                int HP消耗 = 条件.获取属性消耗数值(玩家数据.当前生命值, 玩家数据.最大生命值);
                玩家数据.当前生命值 = Mathf.Max(0, 玩家数据.当前生命值 - HP消耗);
                break;
                
            case 玩家属性类型.魔法值:
                int MP消耗 = 条件.获取属性消耗数值(玩家数据.当前魔法值, 玩家数据.最大魔法值);
                玩家数据.当前魔法值 = Mathf.Max(0, 玩家数据.当前魔法值 - MP消耗);
                break;
                
            case 玩家属性类型.经验值:
                int EXP消耗 = 条件.获取属性消耗数值(玩家数据.当前经验值, 玩家数据.当前经验值);
                玩家数据.当前经验值 = Mathf.Max(0, 玩家数据.当前经验值 - EXP消耗);
                break;
        }
    }
    
    /// <summary>
    /// 获取技能消耗描述文本
    /// </summary>
    public string 获取消耗描述()
    {
        if (前置条件 == null || 前置条件.Length == 0)
        {
            string 基础描述 = "无消耗";
            if (冷却时间 > 0)
                基础描述 += $" | 冷却: {冷却时间}秒";
            if (施法类型 == 施法类型.蓄力 && 施法时间 > 0)
                基础描述 += $" | 蓄力: {施法时间}秒";
            return 基础描述;
        }
            
        string 描述 = "";
        foreach (var 条件 in 前置条件)
        {
            if (条件.条件类型 == 前置条件类型.消耗条件)
            {
                if (!string.IsNullOrEmpty(描述))
                    描述 += " | ";
                描述 += 条件.获取描述();
            }
        }
        
        if (冷却时间 > 0)
            描述 += $" | 冷却: {冷却时间}秒";
        if (施法类型 == 施法类型.蓄力 && 施法时间 > 0)
            描述 += $" | 蓄力: {施法时间}秒";
            
        return 描述;
    }
    
    /// <summary>
    /// 获取等级需求描述
    /// </summary>
    public string 获取等级需求描述()
    {
        if (前置条件 == null || 前置条件.Length == 0)
            return "无等级需求";
            
        string 描述 = "";
        foreach (var 条件 in 前置条件)
        {
            if (条件.条件类型 == 前置条件类型.等级需求)
            {
                if (!string.IsNullOrEmpty(描述))
                    描述 += " | ";
                描述 += 条件.获取描述();
            }
        }
        
        return string.IsNullOrEmpty(描述) ? "无等级需求" : 描述;
    }
    
    /// <summary>
    /// 获取所有前置条件描述
    /// </summary>
    public string 获取前置条件描述()
    {
        if (前置条件 == null || 前置条件.Length == 0)
            return "无前置条件";
            
        string 描述 = "";
        foreach (var 条件 in 前置条件)
        {
            if (!string.IsNullOrEmpty(描述))
                描述 += " | ";
            描述 += 条件.获取描述();
        }
        
        return 描述;
    }
    
    private void OnValidate()
    {
        技能名 = name;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class XlsSortingOrderAttribute : PropertyAttribute
{
    public int mult;
    public XlsSortingOrderAttribute(int mult) {
        this.mult = mult;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class ExportToXlsAttribute : PropertyAttribute
{
}

public enum 施法类型
{
    瞬发,
    蓄力
}

public enum 伤害类型
{
    物理, // 受物理防御影响
    魔法, // 受魔法防御影响
    真实  // 完全无视防御
}

public enum 元素类型
{
    火,
    雷,
    冰,
    暗,
    无
}

public enum 属性
{
    // 基础属性
    力量, 敏捷, 智力, 体质, 精神,
    // 战斗属性
    攻击力, 防御力, 法术防御, 生命值, 法力值, 移动速度, 攻击速度, 暴击率, 暴击伤害,
    // 元素攻击力
    火攻击力, 雷攻击力, 冰攻击力, 暗攻击力,
    // 元素抗性
    火抗性, 雷抗性, 冰抗性, 暗抗性, 物理抗性, 魔法抗性
}

public enum 修改方式
{
    数值,
    百分比
}

[System.Serializable]
public struct 属性修改
{
    public 属性 属性;
    public 修改方式 修改方式;

    [Tooltip("可输入具体数值或公式")]
    [ConditionalField("修改方式", 修改方式.数值)]
    public string 数值;

    [Tooltip("输入0到1之间的值，例如0.5代表50%")]
    [ConditionalField("修改方式", 修改方式.百分比)]
    [Range(0f, 1f)]
    public float 百分比;
}