using UnityEngine;
using MiGame.Data;

namespace MiGame.Config
{
    /// <summary>
    /// 传送点配置
    /// </summary>
    [CreateAssetMenu(fileName = "TeleportPointConfig", menuName = "配置/传送点配置")]
    public class TeleportPointConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("传送点名称")]
        [ReadOnly]
        public string 传送点名称;
        
        [Tooltip("传送节点标识")]
        public string 传送节点;
        
        [Tooltip("需求等级")]
        public int 需求等级;
        
        [Tooltip("权重")]
        public int 权重;
        
        [Header("资源")]
        [Tooltip("图片资源路径")]
        public string 图片资源路径;
        
        [Header("额外信息")]
        [Tooltip("传送点描述")]
        [TextArea(3, 5)]
        public string 传送点描述;
        
        [Tooltip("是否解锁")]
        public bool 是否解锁;
        
        [Tooltip("传送消耗")]
        public int 传送消耗;

        private void OnValidate()
        {
            if (name != 传送点名称)
            {
                传送点名称 = name;
            }
        }
    }
}
