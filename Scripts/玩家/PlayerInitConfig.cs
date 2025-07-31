using System.Collections.Generic;
using UnityEngine;
using MiGame.Items;
using MiGame.Commands;

namespace MiGame.Data
{
    /// <summary>
    /// 单条货币的初始化数据
    /// </summary>
    [System.Serializable]
    public class InitialCurrency
    {
        [Tooltip("货币的ScriptableObject引用")]
        public ItemType 货币名称;
        
        [Tooltip("初始数量")]
        public int 初始数量 = 0;
    }

    /// <summary>
    /// 单条玩家变量的初始化数据
    /// </summary>
    [System.Serializable]
    public class InitialVariable
    {
        [Tooltip("变量的名称")]
        [VariableName] // 使用特性来显示下拉框
        public string 变量名称;
        
        [Tooltip("初始值")]
        public float 初始值 = 0;
    }

    /// <summary>
    /// 其他初始化设置
    /// </summary>
    [System.Serializable]
    public class OtherSettings
    {
        [Tooltip("标记是否为新手玩家")]
        public bool 是否新手 = true;
        
        [Tooltip("玩家的初始等级")]
        public int 初始等级 = 1;
    }

    /// <summary>
    /// 玩家初始化配置的ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "新玩家初始化", menuName = "配置/玩家初始化配置")]
    public class PlayerInitConfig : ScriptableObject
    {
        [Tooltip("此配置的描述信息")]
        public string 描述 = "新玩家首次进入游戏时的初始化配置";
        
        [Header("货币设置")]
        public List<InitialCurrency> 货币初始化 = new List<InitialCurrency>();
        
        [Header("变量设置")]
        public List<InitialVariable> 变量初始化 = new List<InitialVariable>();
        
        [Header("其他设置")]
        public OtherSettings 其他设置 = new OtherSettings();
    }
}
