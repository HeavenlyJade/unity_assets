using UnityEditor;
using UnityEngine;
using MiGame.Commands;
using MiGame.Items;
using MiGame.Pet;
using MiGame.Trail;

#if UNITY_EDITOR
namespace MiGame.CommandSystem.Editor
{
    /// <summary>
    /// 邮件附件配置的PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(邮件附件配置))]
    public class 邮件附件配置Drawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 4 + 6; // 4行高度 + 间距
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            try
            {
                var 类型Prop = property.FindPropertyRelative("类型");
                var 物品配置Prop = property.FindPropertyRelative("物品配置");
                var 宠物配置Prop = property.FindPropertyRelative("宠物配置");
                var 伙伴配置Prop = property.FindPropertyRelative("伙伴配置");
                var 尾迹配置Prop = property.FindPropertyRelative("尾迹配置");
                var 变量名称Prop = property.FindPropertyRelative("变量名称");
                var 数量Prop = property.FindPropertyRelative("数量");
                var 星级Prop = property.FindPropertyRelative("星级");
                var 数值Prop = property.FindPropertyRelative("数值");

                if (类型Prop == null)
                {
                    EditorGUI.LabelField(position, "Error: Cannot find 类型 property");
                    return;
                }

                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing = 2f;
                float currentY = position.y;

                // 第一行：类型选择
                Rect 类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(类型Rect, 类型Prop, new GUIContent("类型"));
                currentY += lineHeight + spacing;

                // 根据类型显示不同的配置
                附件类型 当前类型 = (附件类型)类型Prop.enumValueIndex;

                // 第二行：具体配置选择
                Rect 配置Rect = new Rect(position.x, currentY, position.width, lineHeight);
                switch (当前类型)
                {
                    case 附件类型.物品:
                        EditorGUI.PropertyField(配置Rect, 物品配置Prop, new GUIContent("物品"));
                        break;
                    case 附件类型.宠物:
                        EditorGUI.PropertyField(配置Rect, 宠物配置Prop, new GUIContent("宠物"));
                        break;
                    case 附件类型.伙伴:
                        EditorGUI.PropertyField(配置Rect, 伙伴配置Prop, new GUIContent("伙伴"));
                        break;
                    case 附件类型.尾迹:
                        EditorGUI.PropertyField(配置Rect, 尾迹配置Prop, new GUIContent("尾迹"));
                        break;
                    case 附件类型.玩家变量:
                        EditorGUI.PropertyField(配置Rect, 变量名称Prop, new GUIContent("变量名称"));
                        break;
                }
                currentY += lineHeight + spacing;

                // 第三行：数量和星级/数值
                Rect 数量Rect = new Rect(position.x, currentY, position.width * 0.5f - 2, lineHeight);
                EditorGUI.PropertyField(数量Rect, 数量Prop, new GUIContent("数量"));

                if (当前类型 == 附件类型.玩家变量)
                {
                    // 玩家变量显示数值
                    Rect 数值Rect = new Rect(position.x + position.width * 0.5f + 2, currentY, position.width * 0.5f - 2, lineHeight);
                    EditorGUI.PropertyField(数值Rect, 数值Prop, new GUIContent("数值"));
                }
                else
                {
                    // 其他类型显示星级
                    Rect 星级Rect = new Rect(position.x + position.width * 0.5f + 2, currentY, position.width * 0.5f - 2, lineHeight);
                    EditorGUI.PropertyField(星级Rect, 星级Prop, new GUIContent("星级"));
                }
                currentY += lineHeight + spacing;

                // 第四行：显示配置信息
                Rect 信息Rect = new Rect(position.x, currentY, position.width, lineHeight);
                string 显示信息 = GetDisplayInfo(当前类型, 物品配置Prop, 宠物配置Prop, 伙伴配置Prop, 尾迹配置Prop, 变量名称Prop, 数量Prop, 星级Prop, 数值Prop);
                EditorGUI.LabelField(信息Rect, 显示信息, EditorStyles.miniLabel);
            }
            catch (System.Exception e)
            {
                EditorGUI.LabelField(position, $"Error: {e.Message}");
                Debug.LogError($"邮件附件配置Drawer Error: {e}");
            }

            EditorGUI.EndProperty();
        }

        private string GetDisplayInfo(附件类型 类型, SerializedProperty 物品配置Prop, SerializedProperty 宠物配置Prop, 
            SerializedProperty 伙伴配置Prop, SerializedProperty 尾迹配置Prop, SerializedProperty 变量名称Prop, 
            SerializedProperty 数量Prop, SerializedProperty 星级Prop, SerializedProperty 数值Prop)
        {
            string 类型名称 = 类型.ToString();
            int 数量 = 数量Prop.intValue;

            switch (类型)
            {
                case 附件类型.物品:
                    var 物品 = 物品配置Prop.objectReferenceValue as ItemType;
                    int 星级 = 星级Prop.intValue;
                    return 物品 != null ? $"{类型名称}: {物品.名字} x{数量} (星级:{星级})" : $"{类型名称}: 未选择 x{数量}";
                
                case 附件类型.宠物:
                    var 宠物 = 宠物配置Prop.objectReferenceValue as PetConfig;
                    int 宠物星级 = 星级Prop.intValue;
                    return 宠物 != null ? $"{类型名称}: {宠物.宠物名称} x{数量} (星级:{宠物星级})" : $"{类型名称}: 未选择 x{数量}";
                
                case 附件类型.伙伴:
                    var 伙伴 = 伙伴配置Prop.objectReferenceValue as PartnerConfig;
                    int 伙伴星级 = 星级Prop.intValue;
                    return 伙伴 != null ? $"{类型名称}: {伙伴.宠物名称} x{数量} (星级:{伙伴星级})" : $"{类型名称}: 未选择 x{数量}";
                
                case 附件类型.尾迹:
                    var 尾迹 = 尾迹配置Prop.objectReferenceValue as BaseTrailConfig;
                    int 尾迹星级 = 星级Prop.intValue;
                    return 尾迹 != null ? $"{类型名称}: {尾迹.名称} x{数量} (星级:{尾迹星级})" : $"{类型名称}: 未选择 x{数量}";
                
                case 附件类型.玩家变量:
                    string 变量名 = 变量名称Prop.stringValue;
                    float 数值 = 数值Prop.floatValue;
                    return $"{类型名称}: {变量名} = {数值}";
                
                default:
                    return $"{类型名称}: 未知配置";
            }
        }
    }
}
#endif

