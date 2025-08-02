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

        // 自定义序列化，直接输出数组格式
        public static implicit operator string[](OtherBonusList list)
        {
            return list?.items?.ToArray() ?? new string[0];
        }

        // 从数组创建 OtherBonusList
        public static implicit operator OtherBonusList(string[] array)
        {
            var list = new OtherBonusList();
            if (array != null)
            {
                list.items.AddRange(array);
            }
            return list;
        }

        // 重写 ToString 方法，输出数组格式
        public override string ToString()
        {
            if (items == null || items.Count == 0)
                return "[]";
            
            return "[" + string.Join(",", items) + "]";
        }
    }
} 