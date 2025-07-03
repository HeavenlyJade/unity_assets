using UnityEngine;
using MiGame.Commands;

/// <summary>
/// 命令的基类
/// </summary>
public abstract class AbstractCommand : ScriptableObject
{
    /// <summary>
    /// 执行命令
    /// </summary>
    public abstract void Execute();
} 