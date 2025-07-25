using System;
using UnityEngine;
using System.Collections.Generic;
using MiGame.Items;
using MiGame.Skills;

namespace MiGame.Commands
{
    // 前向声明，如果这些类在其他地方定义
    public class Spell : ScriptableObject { }
    public class SubSpell : ScriptableObject { }
    public class FocusOnUI : ScriptableObject { }
    public class Graphic : ScriptableObject { }

    [Serializable]
    public class ItemTypeIntDictionary : SerializableDictionary<ItemType, int>
    {
    }

    [Serializable]
    public class SkillIntDictionary : SerializableDictionary<Skill, int>
    {
    }
}