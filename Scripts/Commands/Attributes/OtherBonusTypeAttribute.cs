using UnityEngine;

namespace MiGame.Commands
{
    /// <summary>
    /// 标记字段为其他加成类型，用于在 Inspector 中显示下拉选择
    /// </summary>
    public class OtherBonusTypeAttribute : PropertyAttribute
    {
        public OtherBonusTypeAttribute() { }
    }
} 