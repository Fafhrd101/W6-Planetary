using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Armor")]
public class AppliqueArmor : Equipment
{
    public float ArmorMultiplier;
    public int ItemCost;

    public override int Cost
    {
        get
        {
            return ItemCost;
        }
    }

    public override void InitItem(Ship ship)
    {
        ship.maxArmor = (int)(ship.maxArmor * ArmorMultiplier);
        ship.armor = Mathf.Clamp(ship.armor * ArmorMultiplier, 0, ship.maxArmor);
    }

    public override void RemoveItem(Ship ship)
    {
        ship.maxArmor = (int)(ship.maxArmor / ArmorMultiplier);
        ship.armor = Mathf.Clamp(ship.armor / ArmorMultiplier, 0, ship.maxArmor);
    }

    public override void UpdateItem(Ship ship)
    {
    }

}
