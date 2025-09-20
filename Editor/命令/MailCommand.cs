using System;
using System.Collections.Generic;
using UnityEngine;
using MiGame.Commands;
using MiGame.Items;

namespace MiGame.Commands
{
    [Command("mail", "邮件相关")]
    public class MailCommand : AbstractCommand
    {
        public enum DeliveryMethod
        {
            全服,
            个人
        }

        public enum SenderType
        {
            系统,
            玩家
        }

        [CommandParamDesc("邮件投递方式 (全服邮件/个人邮件)")]
        public DeliveryMethod 投递方式;
        
        [ConditionalField("投递方式", DeliveryMethod.个人)]
        [CommandParamDesc("收件人玩家ID")]
        public string 收件人;

        [CommandParamDesc("发件人类型 (决定客户端邮件分类)")]
        public SenderType 发件人;

        [ConditionalField("发件人", SenderType.玩家)]
        [CommandParamDesc("发件人玩家ID (当发件人是玩家时必填)")]
        public string 发件人ID;
        
        [CommandParamDesc("邮件标题")]
        public string 标题;
        
        [TextArea]
        [CommandParamDesc("邮件内容")]
        public string 内容;
        
        [CommandParamDesc("邮件过期时间（天）")]
        [Range(1, 100)]
        public int 过期天数 = 30;
        
        [CommandParamDesc("邮件中包含的附件配置列表")]
        public List<邮件附件配置> 附件 = new List<邮件附件配置>();

        public override void Execute()
        {
            // 在这里实现邮件命令的具体逻辑
            Debug.Log($"正在执行邮件命令: 投递方式={投递方式}, 发件人={发件人}, 标题={标题}");
            
            // 输出完整的邮件JSON（不包含instanceID）
            string 完整邮件JSON = 获取完整邮件JSON();
            Debug.Log("完整邮件JSON:");
            Debug.Log(完整邮件JSON);
        }

        /// <summary>
        /// 获取完整的邮件JSON（包含正确的附件格式）
        /// </summary>
        public string 获取完整邮件JSON()
        {
            var jsonBuilder = new System.Text.StringBuilder();
            jsonBuilder.Append("{");
            jsonBuilder.Append($"\"投递方式\":\"{投递方式}\",");
            
            if (投递方式 == DeliveryMethod.个人)
            {
                jsonBuilder.Append($"\"收件人\":{收件人},");
            }
            
            jsonBuilder.Append($"\"发件人\":\"{发件人}\",");
            jsonBuilder.Append($"\"标题\":\"{标题}\",");
            jsonBuilder.Append($"\"内容\":\"{内容}\",");
            jsonBuilder.Append($"\"过期天数\":{过期天数},");
            
            // 生成附件JSON
            jsonBuilder.Append("\"附件\":[");
            if (附件 != null && 附件.Count > 0)
            {
                for (int i = 0; i < 附件.Count; i++)
                {
                    jsonBuilder.Append(附件[i].导出为JSON());
                    if (i < 附件.Count - 1)
                    {
                        jsonBuilder.Append(",");
                    }
                }
            }
            jsonBuilder.Append("]");
            
            jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }
    }
} 