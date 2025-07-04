using System;

namespace MiGame.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public CommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    // 自定义特性，用于在命令窗口中显示参数描述
    public class CommandParamDescAttribute : Attribute 
    {
        public string Description { get; }
        public CommandParamDescAttribute(string description)
        {
            Description = description;
        }
    }
} 