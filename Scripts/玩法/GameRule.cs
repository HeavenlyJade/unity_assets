using System;
using UnityEngine;

namespace MiGame.Data
{
    public enum 胜利条件类型
    {
        比赛时长结束
    }

    [Serializable]
    public class GameRule
    {
        [Tooltip("比赛总时长（秒）")]
        public int 比赛时长 = 300;

        [Tooltip("玩家进入后的准备时间（秒）")]
        public int 准备时间 = 10;

        [Tooltip("胜利条件的逻辑标识")]
        public 胜利条件类型 胜利条件;
    }
} 