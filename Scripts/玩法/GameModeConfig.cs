using UnityEngine;

namespace MiGame.Data
{
    [CreateAssetMenu(fileName = "NewGameMode", menuName = "玩法配置")]
    public class GameModeConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("玩法的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 名字;
        
        [Tooltip("显示在UI上的玩法名称")]
        public string 玩法名称;

        [Tooltip("玩法的核心逻辑类型")]
        public GameModeType 玩法类型;

        [Tooltip("玩法的简短描述")]
        public string 玩法描述;

        [Tooltip("用于在UI上显示的图标资源路径")]
        public string 图标资源;

        private void OnValidate()
        {
            // 自动将资产文件名同步到"名字"字段
            if (name != 名字)
            {
                名字 = name;
            }
        }
    }
} 