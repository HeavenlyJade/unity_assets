using UnityEditor;
using UnityEngine;

namespace MiGame.Pet
{
    /// <summary>
    /// BasePetConfig 自定义检查器
    /// 仅用于确保新加字段在检查器中正常显示；保持简单，使用默认绘制。
    /// </summary>
    [CustomEditor(typeof(BasePetConfig), true)]
    public class BasePetConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 使用默认绘制，字段顺序遵循声明顺序（特殊加成在携带效果之上）
            DrawDefaultInspector();
        }
    }
}


