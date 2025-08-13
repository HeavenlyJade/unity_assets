using UnityEngine;
using MiGame.Commands;
using MiGame.Lottery;
using MiGame.Utils;

namespace MiGame.Commands
{
    /// <summary>
    /// UI操作类型
    /// </summary>
    public enum UIOperationType
    {
        打开界面,
        关闭界面,
        切换界面
    }

    /// <summary>
    /// 界面类型枚举
    /// </summary>
    public enum UIType
    {
        [Tooltip("抽奖界面")]
        LotteryGui,
        [Tooltip("商城界面")]
        ShopGui,
        [Tooltip("背包界面")]
        BagGui,
        [Tooltip("设置界面")]
        SettingGui,
        [Tooltip("成就界面")]
        AchievementGui,
        [Tooltip("邮件界面")]
        MailGui,
        [Tooltip("活动界面")]
        ActivityGui,
        [Tooltip("排行榜界面")]
        LeaderboardGui,
        [Tooltip("好友界面")]
        FriendGui,
        [Tooltip("聊天界面")]
        ChatGui,
        [Tooltip("传送点界面")]
        WaypointGui
    }

    /// <summary>
    /// 抽奖类型枚举
    /// </summary>
    public enum LotteryUIType
    {
        [Tooltip("翅膀抽奖")]
        翅膀,
        [Tooltip("宠物抽奖")]
        宠物,
        [Tooltip("伙伴抽奖")]
        伙伴,
        [Tooltip("尾迹抽奖")]
        尾迹
    }

    /// <summary>
    /// UI打开指令
    /// </summary>
    [Command("openui", "界面UI操作指令")]
    public class OpenUICommand : AbstractCommand
    {
        [CommandParamDesc("操作类型 (打开界面/关闭界面/切换界面)")]
        public UIOperationType 操作类型;

        [CommandParamDesc("界面名称")]
        public UIType 界面名;

        [ConditionalField("界面名", UIType.LotteryGui, UIType.WaypointGui, false)]
        [CommandParamDesc("抽奖类型 (翅膀/宠物/伙伴/尾迹)")]
        public LotteryUIType 抽奖类型;

        [CommandParamDesc("界面参数 (可选，用于传递额外参数)")]
        public string 界面参数;

        [CommandParamDesc("是否关闭其他界面")]
        public bool 关闭其他界面 = false;

        public override void Execute()
        {
            string operation = 操作类型.ToString();
            string uiName = 界面名.ToString();
            
            Debug.Log($"正在执行UI操作: {operation} - {uiName}");

            // 根据操作类型执行相应逻辑
            switch (操作类型)
            {
                case UIOperationType.打开界面:
                    ExecuteOpenUI();
                    break;
                case UIOperationType.关闭界面:
                    ExecuteCloseUI();
                    break;
                case UIOperationType.切换界面:
                    ExecuteSwitchUI();
                    break;
            }
        }

        private void ExecuteOpenUI()
        {
            string uiName = 界面名.ToString();
            string lotteryType = 界面名 == UIType.LotteryGui ? 抽奖类型.ToString() : "";
            
            if (界面名 == UIType.LotteryGui)
            {
                Debug.Log($"打开抽奖界面: {uiName}, 抽奖类型: {lotteryType}");
                // TODO: 实现具体的抽奖界面打开逻辑
                // 这里应该调用UI管理器打开对应的抽奖界面
            }
            else
            {
                Debug.Log($"打开界面: {uiName}");
                // TODO: 实现具体的界面打开逻辑
                // 这里应该调用UI管理器打开对应的界面
            }
        }

        private void ExecuteCloseUI()
        {
            string uiName = 界面名.ToString();
            Debug.Log($"关闭界面: {uiName}");
            // TODO: 实现具体的界面关闭逻辑
        }

        private void ExecuteSwitchUI()
        {
            string uiName = 界面名.ToString();
            Debug.Log($"切换界面: {uiName}");
            // TODO: 实现具体的界面切换逻辑
        }
    }
}
