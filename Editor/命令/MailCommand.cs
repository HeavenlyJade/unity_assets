using MiGame.Commands;
using UnityEngine;

[Command("mail", "邮件相关")]
public class MailCommand : AbstractCommand
{
    public enum MailType
    {
        系统,
        玩家
    }
    
    public MailType 类型;
    
    // ConditionalField属性需要您项目中已经有对应的实现
    // [ConditionalField("类型", MailType.玩家)] 
    public string 玩家ID;
    
    public string 标题;
    public string 内容;
    
    [CommandParamDesc("邮件中包含的物品")]
    public ItemTypeIntDictionary 物品;
    
    [CommandParamDesc("邮件中包含的技能")]
    public SkillIntDictionary 技能;

    public override void Execute()
    {
        // 在这里实现邮件命令的具体逻辑
        Debug.Log($"正在执行邮件命令: 类型={类型}, 标题={标题}");
    }
} 