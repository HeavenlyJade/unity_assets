using MiGame.Commands;
using UnityEngine;
using MiGame.Items;
using MiGame.Skills;

namespace MiGame.Commands
{
    [Command("mail", "邮件相关")]
    public class MailCommand : AbstractCommand
    {
        public enum MailSenderType
        {
            系统,
            玩家
        }

        public enum MailRecipientType
        {
            系统,
            玩家
        }

        public MailSenderType 发件人类型;

        // ConditionalField属性需要您项目中已经有对应的实现
        // [ConditionalField("发件人类型", MailSenderType.玩家)]
        public string 发件人ID;

        public MailRecipientType 收件人类型;
        
        // ConditionalField属性需要您项目中已经有对应的实现
        // [ConditionalField("收件人类型", MailRecipientType.玩家)] 
        public string 收件人ID;
        
        public string 标题;
        public string 内容;
        
        [CommandParamDesc("邮件中包含的附件和数量")]
        public ItemTypeIntDictionary 附件;
        
        [CommandParamDesc("邮件中包含的技能")]
        public SkillIntDictionary 技能;

        public override void Execute()
        {
            // 在这里实现邮件命令的具体逻辑
            Debug.Log($"正在执行邮件命令: 发件人类型={发件人类型}, 收件人类型={收件人类型}, 标题={标题}");
        }
    }
} 