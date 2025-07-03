using System;
using UnityEngine;
using System.Collections.Generic;
using MiGame.Items;

namespace MiGame.Commands
{

    public class Spell : ScriptableObject { }
    public class SubSpell : ScriptableObject { }
    public class 技能 : ScriptableObject { }
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