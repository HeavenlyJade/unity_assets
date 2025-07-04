using MiGame.Commands;
using UnityEngine;
using MiGame.Skills;

namespace MiGame.Commands
{
    [Command("skillConf", "技能配置相关")]
    public class SkillConfCommand : AbstractCommand
    {
        public enum SkillConfType
        {
            学习,
            遗忘,
            升级,
            设置等级
        }
        
        public enum OnlineStatus
        {
            在线,
            不在线
        }
        
        [CommandParamDesc("目标玩家")]
        public string 玩家;
        
        [CommandParamDesc("技能ID")]
        public int ID;
        
        [CommandParamDesc("操作类型")]
        public SkillConfType 类型;
        
        [CommandParamDesc("技能名称")]
        public Skill 技能;
        
        [CommandParamDesc("玩家在线状态")]
        public OnlineStatus 在线;
        
        [CommandParamDesc("技能等级")]
        public int 等级;
        
        [CommandParamDesc("技能经验值")]
        public int 经验;

        public override void Execute()
        {
            // 在这里实现技能配置命令的具体逻辑
            Debug.Log($"正在执行技能配置命令: 玩家={玩家}, 类型={类型}, 技能={技能?.name}");
        }
    }
} 