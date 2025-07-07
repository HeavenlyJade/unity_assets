using System;
using System.Collections.Generic;
using MiGame.Items;
using UnityEngine;

namespace MiGame.Data
{
    [Serializable]
    public class LevelRankReward
    {
        [Tooltip("排名名次，例如：1, 2, 3")]
        public int 名次;

        [Tooltip("该名次可以获得的奖励列表")]
        public List<ItemReward> 奖励列表;
    }
} 