# Unity配置系统设计文档

## 📋 目录
- [系统概述](#系统概述)
- [核心架构](#核心架构)
- [配置类设计模式](#配置类设计模式)
- [数据导出系统](#数据导出系统)
- [JSON配置管理](#json配置管理)
- [命名规范](#命名规范)
- [目录结构](#目录结构)
- [使用指南](#使用指南)
- [扩展开发](#扩展开发)
- [最佳实践](#最佳实践)

## 🎯 系统概述

本Unity配置系统是一个基于ScriptableObject的数据驱动配置管理框架，支持：
- **可视化配置编辑** - 通过Unity Inspector进行配置
- **多格式导出** - 支持导出为Lua、JSON等格式
- **数据验证** - 自动校验配置数据的有效性
- **批量生成** - 支持从JSON批量生成配置资源
- **模块化设计** - 易于扩展和维护

## 🏗️ 核心架构

### 系统组件关系图
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   JSON配置文件   │    │  ScriptableObject │    │   导出系统      │
│                 │◄──►│    配置类        │◄──►│                 │
│  - 变量名配置    │    │  - AchievementConfig│  │  - ConfigExporter│
│  - 公共配置      │    │  - PetConfig      │  │  - JsonToLua    │
│  - 游戏数据      │    │  - LotteryConfig  │  │  - 批量生成工具  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### 核心特性
- **数据驱动** - 配置与代码分离，支持热更新
- **类型安全** - 强类型配置，编译时检查
- **可视化编辑** - Unity Inspector友好界面
- **自动验证** - OnValidate机制确保数据正确性
- **多格式支持** - 统一配置，多端使用

## 🔧 配置类设计模式

### 1. 基础配置类模板

```csharp
using UnityEngine;
using System;
using System.Collections.Generic;

namespace YourProject.Config
{
    /// <summary>
    /// 配置类型枚举
    /// </summary>
    public enum 配置类型
    {
        [Tooltip("类型1")]
        类型1,
        [Tooltip("类型2")]
        类型2
    }

    /// <summary>
    /// 嵌套数据结构
    /// </summary>
    [Serializable]
    public class 嵌套结构
    {
        [Tooltip("字段描述")]
        public string 字段名;
        
        [Tooltip("数值字段")]
        public int 数值;
    }

    /// <summary>
    /// 主配置类
    /// </summary>
    [CreateAssetMenu(fileName = "NewXxx", menuName = "配置/xxx配置")]
    public class XxxConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("配置的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 名字;

        [Tooltip("配置描述")]
        [TextArea(2, 4)]
        public string 描述;

        [Tooltip("配置类型")]
        public 配置类型 类型 = 配置类型.类型1;

        [Header("数据配置")]
        [Tooltip("嵌套结构列表")]
        public List<嵌套结构> 数据列表 = new List<嵌套结构>();

        private void OnValidate()
        {
            // 自动同步文件名到名字字段
            if (name != 名字)
            {
                名字 = name;
            }

#if UNITY_EDITOR
            // 数据验证逻辑
            ValidateData();
#endif
        }

#if UNITY_EDITOR
        private void ValidateData()
        {
            // 实现数据验证逻辑
        }
#endif
    }
}
```

### 2. 字段属性使用规范

| 属性 | 用途 | 示例 |
|------|------|------|
| `[Header("分组名")]` | 字段分组 | `[Header("基础信息")]` |
| `[Tooltip("描述")]` | 字段说明 | `[Tooltip("配置描述")]` |
| `[ReadOnly]` | 只读字段 | `[ReadOnly] public string 名字;` |
| `[TextArea(2, 4)]` | 多行文本 | `[TextArea(2, 4)] public string 描述;` |
| `[Range(0, 100)]` | 数值范围 | `[Range(0, 100)] public int 数值;` |

### 3. 常用枚举模式

```csharp
// 稀有度枚举
public enum 稀有度
{
    [Tooltip("N级")]
    N,
    [Tooltip("R级")]
    R,
    [Tooltip("SR级")]
    SR,
    [Tooltip("SSR级")]
    SSR,
    [Tooltip("UR级")]
    UR
}

// 奖励类型枚举
public enum 奖励类型
{
    [Tooltip("物品")]
    物品,
    [Tooltip("宠物")]
    宠物,
    [Tooltip("伙伴")]
    伙伴,
    [Tooltip("翅膀")]
    翅膀,
    [Tooltip("尾迹")]
    尾迹
}
```

## 📤 数据导出系统

### 1. 导出器基类

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace YourProject.Editor.Exporter
{
    /// <summary>
    /// 配置导出器基类
    /// </summary>
    /// <typeparam name="T">要导出的ScriptableObject类型</typeparam>
    public abstract class ConfigExporter<T> : IConfigExporter where T : ScriptableObject
    {
        public Type TargetType => typeof(T);

        /// <summary>
        /// 获取资产文件路径
        /// </summary>
        public abstract string GetAssetPath();

        /// <summary>
        /// 导出配置到指定目录
        /// </summary>
        public virtual void Export(string exportDir)
        {
            var allConfigs = FindAssets();
            if (allConfigs == null || allConfigs.Count == 0) return;

            var configName = GetType().Name.Replace("Exporter", "");
            var sb = new StringBuilder();

            // 生成文件头
            sb.AppendLine($"-- {configName}.lua");
            sb.AppendLine("-- Generated by ConfigExporter");
            sb.AppendLine();
            sb.AppendLine($"local {configName} = {{Data = {{}}}}");
            sb.AppendLine();

            // 生成配置数据
            sb.AppendLine($"{configName}.Data = {{");
            foreach (var config in allConfigs)
            {
                if (config == null) continue;
                sb.AppendLine($"    ['{config.name}'] = {{");
                ObjectToLua(config, sb, 3);
                sb.AppendLine("    },");
            }
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine($"return {configName}");

            // 写入文件
            var outputPath = Path.Combine(exportDir, configName + ".lua");
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            Debug.Log($"导出 {typeof(T).Name} 配置到: {outputPath}");
        }

        protected void ObjectToLua(object obj, StringBuilder sb, int indentLevel)
        {
            // 反射序列化逻辑
            // 支持基本类型、枚举、列表、嵌套对象等
        }

        protected List<T> FindAssets()
        {
            // 查找指定路径下的所有配置资源
        }
    }

    public interface IConfigExporter
    {
        Type TargetType { get; }
        void Export(string exportDir);
        string GetAssetPath();
    }
}
```

### 2. 具体导出器实现

```csharp
namespace YourProject.Editor.Exporter
{
    /// <summary>
    /// 宠物配置导出器
    /// </summary>
    public class PetConfigExporter : ConfigExporter<PetConfig>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/宠物";
        }
    }

    /// <summary>
    /// 成就配置导出器
    /// </summary>
    public class AchievementConfigExporter : ConfigExporter<AchievementConfig>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/成就天赋";
        }
    }
}
```

### 3. JSON到Lua导出器

```csharp
/// <summary>
/// JSON文件直接导出为Lua的专用工具
/// </summary>
public static class JsonToLuaExporter
{
    [MenuItem("工具/导出/导出玩家变量 (JSON to Lua)")]
    public static void ExportVariableNames()
    {
        string jsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
        string luaExportDir = EditorPrefs.GetString("ConfigExporter_OutputPath", "Assets/Lua/Config");
        
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"JSON文件未找到: {jsonPath}");
            return;
        }

        // 读取JSON并转换为Lua格式
        string jsonContent = File.ReadAllText(jsonPath);
        var data = JsonUtility.FromJson<VariableData>(jsonContent);
        
        // 构建Lua文件内容
        var sb = new StringBuilder();
        // ... Lua生成逻辑
        
        // 写入文件
        string outputPath = Path.Combine(luaExportDir, "VariableNameConfig.lua");
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        
        Debug.Log($"成功将 {jsonPath} 导出到 {outputPath}");
    }
}
```

## 📄 JSON配置管理

### 1. JSON配置文件结构

```json
{
    "VariableNames": [
        "加成_百分比_双倍训练",
        "加成_百分比_奖杯加成",
        "数据_固定值_战力值"
    ],
    "StatNames": [
        "数据_固定值_攻击力",
        "数据_固定值_移动速度"
    ],
    "PlayerAttributeNames": [
        "速度",
        "攻击",
        "防御"
    ]
}
```

### 2. JSON数据验证

```csharp
private void LoadAndValidateFromJson()
{
    string jsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
    if (File.Exists(jsonPath))
    {
        string json = File.ReadAllText(jsonPath);
        var data = JsonUtility.FromJson<VariableData>(json);
        
        if (data != null)
        {
            // 验证数据有效性
            ValidateVariableNames(data.VariableNames);
        }
    }
}

private void ValidateVariableNames(List<string> variableNames)
{
    // 实现具体的验证逻辑
    foreach (var variable in 等级效果)
    {
        if (!variableNames.Contains(variable.效果字段名称))
        {
            Debug.LogError($"配置错误: 变量 '{variable.效果字段名称}' 未定义!", this);
        }
    }
}
```

### 3. Vector3 JSON转换器

```csharp
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Vector3Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector3);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Vector3(
            (float)jo["x"],
            (float)jo["y"],
            (float)jo["z"]
        );
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector3 v = (Vector3)value;
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(v.x);
        writer.WritePropertyName("y");
        writer.WriteValue(v.y);
        writer.WritePropertyName("z");
        writer.WriteValue(v.z);
        writer.WriteEndObject();
    }
}
```

## 📝 命名规范

### 1. 类命名规范
- **配置类**: `XxxConfig` (如: `PetConfig`, `AchievementConfig`)
- **导出器**: `XxxConfigExporter` (如: `PetConfigExporter`)
- **嵌套结构**: 描述性中文名 (如: `升星材料`, `携带效果`)

### 2. 字段命名规范
- **使用中文描述性名称** (如: `宠物名称`, `稀有度`)
- **保持一致的命名风格** (如: `基础属性列表`, `成长率列表`)
- **重要字段使用`[ReadOnly]`保护** (如: `名字`, `ID`)

### 3. 枚举命名规范
- **使用中文标签** (如: `普通成就`, `天赋成就`)
- **添加Tooltip说明** (如: `[Tooltip("N级")]`)
- **保持简洁明了** (如: `N`, `R`, `SR`, `SSR`, `UR`)

## 📁 目录结构

```
Scripts/
├── 配置类/                    # 各种配置ScriptableObject
│   ├── 成就/
│   │   └── AchievementConfig.cs
│   ├── 宠物/
│   │   ├── BasePetConfig.cs
│   │   └── PetConfig.cs
│   ├── 奖励/
│   │   └── NewRewardConfig.cs
│   └── 抽奖/
│       └── LotteryConfig.cs
├── 配置导出/                  # 导出系统
│   ├── ConfigExporter.cs      # 导出基类
│   ├── JsonToLuaExporter.cs   # JSON转Lua工具
│   ├── PetConfigExporter.cs   # 宠物配置导出器
│   └── AchievementConfigExporter.cs
├── 工具脚本/                  # 批量生成和JSON处理工具
│   ├── PetConfigBatchGenerator.cs
│   ├── GeneratePartnerLotteryConfigs.cs
│   └── BatchGenerateTrailShopConfig.cs
├── 公共配置/                  # 通用JSON配置文件
│   ├── BonusCalculationMethods.json
│   └── OtherBonusTypes.json
└── Vector3Converter.cs        # JSON转换器
```

## 🚀 使用指南

### 1. 创建新配置类

1. **复制模板文件** - 使用基础配置类模板
2. **修改命名空间** - 替换为项目专用命名空间
3. **定义字段结构** - 根据业务需求添加字段
4. **添加验证逻辑** - 在`OnValidate()`中实现验证
5. **创建菜单项** - 使用`[CreateAssetMenu]`属性

### 2. 创建导出器

1. **继承基类** - 继承`ConfigExporter<T>`
2. **实现抽象方法** - 重写`GetAssetPath()`
3. **配置导出路径** - 指定资源文件位置
4. **测试导出功能** - 验证导出结果

### 3. 配置JSON文件

1. **创建JSON文件** - 在`Scripts/公共配置/`目录下
2. **定义数据结构** - 使用与C#类对应的结构
3. **添加验证逻辑** - 在配置类中实现JSON验证
4. **测试数据加载** - 验证JSON解析正确性

## 🔧 扩展开发

### 1. 自定义字段类型

```csharp
[Serializable]
public class 自定义字段
{
    [Tooltip("特殊字段")]
    public string 特殊字段;
    
    // 添加自定义验证
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(特殊字段))
        {
            Debug.LogWarning("特殊字段不能为空!");
        }
    }
}
```

### 2. 自定义导出格式

```csharp
public class CustomConfigExporter : ConfigExporter<CustomConfig>
{
    public override void Export(string exportDir)
    {
        // 自定义导出逻辑
        var configs = FindAssets();
        
        // 生成自定义格式文件
        GenerateCustomFormat(configs, exportDir);
    }
    
    private void GenerateCustomFormat(List<CustomConfig> configs, string exportDir)
    {
        // 实现自定义格式生成
    }
}
```

### 3. 自定义验证规则

```csharp
private void OnValidate()
{
    // 基础验证
    if (name != 名字) 名字 = name;
    
    // 自定义业务验证
    ValidateBusinessRules();
    
    // JSON数据验证
    ValidateFromJson();
}

private void ValidateBusinessRules()
{
    // 实现业务特定的验证逻辑
    if (最大等级 < 初始等级)
    {
        Debug.LogError("最大等级不能小于初始等级!", this);
    }
}
```

## 💡 最佳实践

### 1. 设计原则
- **单一职责** - 每个配置类只负责一种数据类型
- **开闭原则** - 对扩展开放，对修改封闭
- **数据驱动** - 配置与代码分离，支持热更新
- **类型安全** - 使用强类型，避免运行时错误

### 2. 性能优化
- **延迟加载** - 只在需要时加载JSON数据
- **缓存机制** - 缓存频繁访问的配置数据
- **批量操作** - 使用批量生成工具提高效率
- **内存管理** - 及时释放不需要的配置对象

### 3. 维护建议
- **版本控制** - 使用Git管理配置文件和代码
- **文档更新** - 及时更新配置字段说明
- **测试覆盖** - 为关键配置添加单元测试
- **代码审查** - 定期审查配置类设计

### 4. 常见问题解决

#### 问题1: 配置字段不显示
**解决方案**: 检查字段是否为`public`，是否添加了正确的属性标记

#### 问题2: JSON解析失败
**解决方案**: 检查JSON格式是否正确，字段名是否匹配

#### 问题3: 导出文件为空
**解决方案**: 检查资源路径是否正确，配置类是否继承自ScriptableObject

#### 问题4: 验证错误过多
**解决方案**: 优化验证逻辑，避免在每帧都执行验证

## 📚 总结

本Unity配置系统提供了一个完整的、可扩展的配置管理解决方案。通过ScriptableObject、JSON配置、自动导出等机制，实现了数据驱动的配置管理，大大提高了开发效率和代码质量。

系统设计遵循了良好的软件工程原则，具有良好的可维护性和扩展性，可以轻松适应不同项目的需求。通过遵循本文档的规范和最佳实践，可以快速在新项目中建立类似的配置管理系统。

---

*文档版本: 1.0*  
*最后更新: 2024年*  
*维护者: 开发团队*
