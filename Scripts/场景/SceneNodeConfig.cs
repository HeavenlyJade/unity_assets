using UnityEngine;
using System.Collections.Generic;
using MiGame.Data;

namespace MiGame.Scene
{
    /// <summary>
    /// 场景中特殊节点的类型
    /// </summary>
    public enum SceneNodeType
    {
        陷阱,
        跳台,
        安全区,
        飞行比赛
    }

    /// <summary>
    /// 用于存储定时指令和其触发间隔的组合
    /// </summary>
    [System.Serializable]
    public class TimedCommand
    {
        [Tooltip("要执行的定时指令")]
        public string 指令;

        [Tooltip("距离上一次执行的间隔时间（秒）")]
        public float 间隔 = 1f;
    }

    /// <summary>
    /// 区域节点配置
    /// </summary>
    [System.Serializable]
    public class 区域节点配置
    {
        [Tooltip("区域的名字")]
        public string 名字;
    }


    /// <summary>
    /// 场景节点配置
    /// </summary>
    [CreateAssetMenu(fileName = "NewSceneNodeConfig", menuName = "场景节点配置")]
    public class SceneNodeConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("节点的名称(根据文件名自动生成)")]
        [ReadOnly]
        public string 名字;

        [Tooltip("节点的唯一ID，在创建时自动生成")]
        [ReadOnly]
        public string 唯一ID;

        [Tooltip("场景中预制体的路径，用于加载")]
        public string 场景节点路径;

        [Header("节点属性")]
        [Tooltip("节点的类型")]
        public SceneNodeType 场景类型;

        [Tooltip("区域节点配置")]
        public 区域节点配置 区域节点配置 = new 区域节点配置();

        [Tooltip("该节点关联的关卡配置")]
        public LevelConfig 关联关卡;

        [Tooltip("具体的玩法规则，当此节点触发时生效")]
        public GameRule 玩法规则 = new GameRule();

        [Tooltip("节点触发时的音效，填写资源的路径")]
        public string 音效资源;

        [Header("指令")]
        [Tooltip("进入节点时触发的指令")]
        public string 进入指令;

        [Tooltip("离开节点时触发的指令")]
        public string 离开指令;

        [Tooltip("定时触发的指令列表，可以添加多个")]
        public List<TimedCommand> 定时指令列表;

        private void OnValidate()
        {
            // 自动将资产文件名同步到"名字"字段
            if (name != 名字)
            {
                名字 = name;
            }

            if (string.IsNullOrEmpty(唯一ID))
            {
                唯一ID = System.Guid.NewGuid().ToString();
            }
        }
    }
} 