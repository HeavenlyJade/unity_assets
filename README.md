# MiGameBase 项目

## 项目简介
这是一个基于Unity的游戏项目，包含宠物、伙伴、技能等游戏系统。

## 项目结构
- `Assets/Scripts/` - 主要脚本文件
- `Assets/GameConf/` - 游戏配置文件
- `Assets/Editor/` - 编辑器工具脚本
- `Assets/Lua/` - Lua脚本文件

## 主要功能
- 宠物系统
- 伙伴系统
- 技能系统
- 配置导出工具
- 批量生成工具

## 开发环境
- Unity 2022.3 LTS 或更高版本
- 支持URP渲染管线

## 使用说明
1. 打开Unity项目
2. 使用菜单栏的工具进行配置生成和导出
3. 查看控制台输出了解操作状态

## 注意事项
- 确保网络连接正常以避免Unity Connect相关错误
- 配置导出工具会自动处理资源刷新，避免在导入期间调用AssetDatabase.Refresh 