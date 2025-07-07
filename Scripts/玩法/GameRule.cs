using System;
using MiGame.Scene;
using UnityEngine;

namespace MiGame.Data
{
    [Serializable]
    public class GameRule
    {
        [Tooltip("比赛总时长（秒）")]
        public int 比赛时长 = 300;

        [Tooltip("玩家进入后的准备时间（秒）")]
        public int 准备时间 = 10;

        [Tooltip("指定作为入口的场景节点配置")]
        public SceneNodeConfig 入口节点;

        [Tooltip("胜利条件的逻辑标识")]
        public string 胜利条件 = "HIGHEST_SCORE";
    }
} 