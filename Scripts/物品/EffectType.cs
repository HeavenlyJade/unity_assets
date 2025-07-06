using UnityEngine;

namespace MiGame.Items
{
    [CreateAssetMenu(fileName = "特效", menuName = "公用/物品/特效")]
    public class EffectType : ScriptableObject
    {
        public string 特效名;
        public GameObject 特效预制体;
        public float 持续时间;
        public bool 跟随目标;
        public Vector3 位置偏移;
        public Vector3 旋转偏移;
        public float 缩放倍率 = 1f;
        
        public string 施法音效;
        public string 击中音效;

        public float 震荡强度;
        public float 震荡时间;

        void OnValidate()
        {
            if (string.IsNullOrEmpty(特效名))
            {
                特效名 = this.name;
            }
        }
    }
} 