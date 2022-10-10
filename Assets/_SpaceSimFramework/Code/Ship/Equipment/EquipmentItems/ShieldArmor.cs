using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Shield")]
public class ShieldArmor : Equipment
{
    public float ShieldBonus = 500f;
    public int ItemCost;
    public string Name = "Shield";
    
    public override int Cost
    {
        get
        {
            return ItemCost;
        }
    }

    public override void InitItem(Ship ship)
    {
        ship.maxArmor = (int)(ship.maxArmor + ShieldBonus);
        ship.armor = Mathf.Clamp(ship.armor + ShieldBonus, 0, ship.maxArmor);
    }

    public override void RemoveItem(Ship ship)
    {
        ship.maxArmor = (int)(ship.maxArmor - ShieldBonus);
        ship.armor = Mathf.Clamp(ship.armor - ShieldBonus, 1, ship.maxArmor); // min 1 so it doesn't blow up upon removal
    }

    public override void UpdateItem(Ship ship)
    {
    }

}
