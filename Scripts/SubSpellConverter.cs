using Newtonsoft.Json;
using System;
using UnityEngine;

// 假设 SubSpell 是一个 ScriptableObject 或具有唯一名称的普通类
public class SubSpellConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        // 这里需要确认 SubSpell 的确切类型
        // 如果它不是 ScriptableObject，可能需要修改
        return typeof(ScriptableObject).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        string name = (string)reader.Value;
        // 这里需要一种方法来通过名字找到 SubSpell 实例，Resources.LoadAll 是一个可行但效率不高的方法
        // 如果有更好的管理器，应替换此处的逻辑
        var allSpells = Resources.LoadAll(string.Empty, objectType);
        foreach (var spell in allSpells)
        {
            if (spell.name == name)
            {
                return spell;
            }
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        // 我们假设只序列化它的名字作为标识
        var spell = value as UnityEngine.Object;
        if (spell != null)
        {
            writer.WriteValue(spell.name);
        }
        else
        {
            writer.WriteNull();
        }
    }
} 