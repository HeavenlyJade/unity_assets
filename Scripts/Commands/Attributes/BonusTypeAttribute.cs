using UnityEngine;

namespace MiGame
{
    /// <summary>
    /// This attribute is used on a List<PlayerBonus> to indicate
    /// what type of names should be loaded into the dropdown.
    /// It has no drawer itself; it's just a data carrier for PlayerBonusDrawer.
    /// </summary>
    public class BonusTypeAttribute : PropertyAttribute
    {
        public VariableNameType NameType { get; }

        public BonusTypeAttribute(VariableNameType nameType)
        {
            NameType = nameType;
        }
    }
}
