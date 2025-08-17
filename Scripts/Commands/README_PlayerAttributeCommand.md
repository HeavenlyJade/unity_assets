# PlayerAttributeCommand 玩家属性操作指令

## 概述

`PlayerAttributeCommand` 是一个全新的玩家属性操作指令，支持对玩家属性进行各种操作，包括数值修改、加成测试等。该指令使用 `PlayerAttributeNames` 配置来管理可操作的属性名称。

## 主要特性

### 1. 支持的操作类型
- **新增** - 增加属性值
- **设置** - 设置属性值为指定数值  
- **减少** - 减少属性值
- **查看** - 查看属性值
- **恢复** - 恢复属性值到默认状态
- **刷新** - 刷新属性加成效果
- **测试加成** - 测试属性加成效果

### 2. 属性名称配置
- 使用 `[PlayerAttributeName]` 属性标记
- 自动从 `VariableNames.json` 的 `PlayerAttributeNames` 配置中读取
- 支持中文、英文、数字和下划线字符
- 在Inspector中显示为下拉选择框

### 3. 支持的数值类型
- 整数 (如: 100, -50)
- 小数 (如: 100.5, -50.75)
- 长整型 (如: 1000000000)
- 科学计数法 (如: 1.5e3)

### 4. 属性来源类型
- **装备** - 装备提供的属性
- **任务** - 任务奖励的属性
- **技能** - 技能效果的属性
- **COMMAND** - 指令直接操作的属性

### 5. 完整的加成系统
- **玩家属性加成** - 使用 `VariableNameType.PlayerAttribute` 类型
- **玩家变量加成** - 使用 `VariableNameType.Variable` 类型
- **其他加成** - 宠物、伙伴、尾迹、翅膀等

## 使用方法

### 1. 在Unity中创建指令
1. 在Project窗口中右键选择 `Create > 配置 > 变量名配置`
2. 在 `VariableNameConfig` 中配置 `PlayerAttributeNames` 列表
3. 创建 `PlayerAttributeCommand` 指令实例
4. 配置操作类型、玩家UID、属性名等参数

### 2. 配置属性名称
在 `VariableNames.json` 文件中配置 `PlayerAttributeNames`：

```json
{
  "PlayerAttributeNames": [
    "速度",
    "攻击",
    "生命",
    "魔法",
    "防御"
  ]
}
```

### 3. 使用示例

#### 新增攻击力
```csharp
// 操作类型: 新增
// 属性名: 攻击
// 数值: 100
// 来源: COMMAND
// 玩家属性加成: 攻击加成 (单独相加, 倍率1.5)
// 其他加成: 宠物, 伙伴
```

#### 设置生命值
```csharp
// 操作类型: 设置
// 属性名: 生命
// 数值: 1000
// 来源: 装备
// 玩家变量加成: 生命值倍率 (最终乘法, 倍率1.2)
```

#### 测试加成效果
```csharp
// 操作类型: 测试加成
// 属性名: 速度
// 来源: COMMAND
// 玩家属性加成: 速度加成 (基础相加, 倍率2.0)
// 其他加成: 宠物, 尾迹, 翅膀
```

## 技术实现

### 1. 属性标记
- `PlayerAttributeNameAttribute` - 标识玩家属性名称字段
- `PlayerAttributeNameDrawer` - 自定义属性绘制器

### 2. 类型系统
- `VariableNameType.PlayerAttribute` - 新增的玩家属性类型
- 与现有的 `Variable` 和 `Stat` 类型并列

### 3. 配置管理
- 自动从JSON文件加载配置
- 实时验证和自动保存
- 支持热重载配置变更

## 注意事项

1. **属性名称验证** - 只允许使用中文、英文、数字和下划线
2. **配置同步** - 修改 `VariableNameConfig` 后会自动保存到JSON文件
3. **错误处理** - 提供详细的错误日志和验证信息
4. **性能优化** - 使用缓存机制避免重复加载配置

## 扩展性

该指令设计具有良好的扩展性：
- 可以轻松添加新的操作类型
- 支持自定义加成计算方式
- 可以集成到其他游戏系统中
- 支持批量操作和脚本化配置

## 相关文件

- `Scripts/Commands/PlayerAttributeCommand.cs` - 主指令类
- `Scripts/Commands/Attributes/PlayerAttributeNameAttribute.cs` - 属性标记
- `Editor/Commands/PlayerAttributeNameDrawer.cs` - 编辑器绘制器
- `Scripts/玩家变量/VariableNameConfig.cs` - 配置管理
- `GameConf/玩家变量/VariableNames.json` - 配置文件
