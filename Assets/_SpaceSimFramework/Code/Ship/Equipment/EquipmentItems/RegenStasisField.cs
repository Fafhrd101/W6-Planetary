using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/RegenerativeStasisField")]
public class RegenStasisField : ActivatableEquipment
{
    public int ItemCost;
    public float Duration;
    public float RegenPercentage;

    private float _timer;
    private float _regenAmount, _initialArmor;

    public override int Cost
    {
        get
        {
            return ItemCost;
        }
    }


    public override void InitItem(Ship ship)
    {
        base.InitItem(ship);
        isSingleUse = true;
        _isActive = false;       
    }

    public override void RemoveItem(Ship ship)
    {
        base.RemoveItem(ship);
    }

    public override void UpdateItem(Ship ship)
    {
        if (_isActive)
        {
            _timer += Time.deltaTime;
            ship.armor = _initialArmor + _timer / Duration * _regenAmount;
            ship.armor = Mathf.Clamp(ship.armor, 0, ship.maxArmor);

            if (_timer > Duration)
            {
                // When finished with operation
                RemoveItem(ship);
            }
        }
    }

    public override bool SetActive(bool isActive, Ship ship)
    {
        // Dont use item if there is nothing to heal
        if (ship.armor == ship.maxArmor)
            return false;

        _isActive = true;
        _timer = 0;
        _regenAmount = RegenPercentage * ship.maxArmor * 0.01f;
        _initialArmor = ship.armor;
        return true;
    }
}
