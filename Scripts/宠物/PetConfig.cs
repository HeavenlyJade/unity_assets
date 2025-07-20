using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Commands;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiGame.Pets
{
    [CreateAssetMenu(fileName = "宠物", menuName = "公用/宠物/宠物")]
    public class PetConfig : ScriptableObject
    {
        [Header("基础信息")]
        public string 宠物名称;
        public string 宠物描述;
        public string 宠物类型;
        public string 稀有度;
        public int 初始等级;
        public int 最大等级;
        public string 元素类型;

        [Header("基础属性")]
        public PetBaseStats 基础属性;

        [Header("成长率")]
        public PetGrowthRate 成长率;

        [Header("等级效果")]
        public List<PetLevelEffect> 等级效果 = new List<PetLevelEffect>();

        [Header("升星效果")]
        public List<PetStarEffect> 升星效果 = new List<PetStarEffect>();

        [Header("升星消耗")]
        public List<PetStarCost> 升星消耗 = new List<PetStarCost>();

        [Header("携带效果")]
        public List<PetCarryEffect> 携带效果 = new List<PetCarryEffect>();

        [Header("技能列表")]
        public List<string> 技能列表 = new List<string>();

        [Header("进化信息")]
        public PetEvolution 进化条件;
        public string 进化后形态;

        [Header("获取方式")]
        public List<string> 获取方式 = new List<string>();

        [Header("资源路径")]
        public string 模型资源;
        public string 头像资源;
        public string 音效资源;

        [Header("特殊标签")]
        public List<string> 特殊标签 = new List<string>();

        public void OnValidate()
        {
            宠物名称 = name;
        }

        public override string ToString()
        {
            return 宠物名称;
        }
    }

    [Serializable]
    public struct PetBaseStats
    {
        public int 生命值;
        public int 攻击力;
        public int 防御力;
        public int 敏捷;
        public int 智力;
    }

    [Serializable]
    public struct PetGrowthRate
    {
        public string 生命值成长;
        public string 攻击力成长;
        public string 防御力成长;
        public string 敏捷成长;
        public string 智力成长;
    }

    [Serializable]
    public struct PetLevelEffect
    {
        public int level;
        public string 解锁技能;
        public SerializableDictionary<string, float> 属性加成 = new SerializableDictionary<string, float>();
        public string 特殊效果;
    }

    [Serializable]
    public struct PetStarEffect
    {
        public int star;
        public SerializableDictionary<string, float> 属性倍率 = new SerializableDictionary<string, float>();
        public string 新增技能;
        public string 特殊能力;
        public SerializableDictionary<string, int> 升星材料 = new SerializableDictionary<string, int>();
    }

    [Serializable]
    public struct PetStarCost
    {
        public int star;
        public SerializableDictionary<string, int> 消耗物品 = new SerializableDictionary<string, int>();
        public float 成功率;
    }

    [Serializable]
    public struct PetCarryEffect
    {
        public int star;
        public string 效果名称;
        public string 效果描述;
        public string 效果数值;
        public string 触发条件;
        public string 作用范围;
    }

    [Serializable]
    public struct PetEvolution
    {
        public int 需要等级;
        public SerializableDictionary<string, int> 需要材料 = new SerializableDictionary<string, int>();
    }
} 