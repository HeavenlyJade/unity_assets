using System;

namespace MiGame.Commands
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string commandName { get; }
        public string description { get; }

        public CommandAttribute(string commandName, string description = "")
        {
            this.commandName = commandName;
            this.description = description;
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