using UnityEngine;
using UnityEditor;
using MiGame.Shop;

namespace MiGame.Shop.Editor
{
    /// <summary>
    /// 商城物品配置的自定义Inspector
    /// </summary>
    [CustomEditor(typeof(ShopItemConfig))]
    public class ShopItemConfigInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 绘制默认的Inspector，这样会自动使用我们的PriceConfigDrawer
            DrawDefaultInspector();

            // 添加一些便捷按钮和信息显示
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("工具", EditorStyles.boldLabel);
            
            ShopItemConfig config = (ShopItemConfig)target;
            
            // 显示当前价格信息
            if (config.价格 != null && !string.IsNullOrEmpty(config.价格.价格数量))
            {
                EditorGUILayout.LabelField("当前价格", config.价格.价格数量);
            }

            // 添加验证按钮
            if (GUILayout.Button("验证价格配置"))
            {
                if (config.价格 != null)
                {
                    config.价格.验证价格();
                    Debug.Log($"价格验证完成：{config.价格.价格数量}");
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
