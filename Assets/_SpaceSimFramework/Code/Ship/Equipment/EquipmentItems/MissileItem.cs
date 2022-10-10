using UnityEngine;

[CreateAssetMenu(menuName = "Consumables/Missile")]
public class MissileItem : Equipment//Consumables
{
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
        RemoveItem(ship);
        ship.Equipment.UnmountEquipmentItem(this);
    }
    public override void RemoveItem(Ship ship){}
    public override void UpdateItem(Ship ship){}
}
