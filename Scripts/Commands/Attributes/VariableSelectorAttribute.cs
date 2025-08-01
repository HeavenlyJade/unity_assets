using UnityEngine;

namespace MiGame
{
    public class VariableSelectorAttribute : PropertyAttribute
    {
        public VariableNameType NameType { get; }

        public VariableSelectorAttribute(VariableNameType nameType)
        {
            NameType = nameType;
        }
    }
}
