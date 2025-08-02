using System.Collections.Generic;
using UnityEngine;

namespace MiGame.Commands
{
    /// <summary>
    /// 其他加成列表，用于在 Inspector 中显示下拉选择
    /// </summary>
    [System.Serializable]
    public class OtherBonusList
    {
        [SerializeField]
        public List<string> items = new List<string>();
    }
} 