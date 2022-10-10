using UnityEngine;

/// <summary>
/// This is a mountable equipment item which can be installed on a ship. It applies
/// certain effects to the carrier ship.
/// </summary>
public abstract class Equipment: ScriptableObject
{
    // Cost of the item
    public abstract int Cost { get; }
    public string ItemName;
    [TextArea]
    public string Description;
    public bool isActivateable = false;
    public bool isSingleUse = false;
    [Tooltip("If sellable, item can be removed and sold. Otherwise, its permanently mounted.")]
    public bool isSellable = true;
    
    /// <summary>
    /// Initializes the mounted piece of equipment by applying the initial effects of the
    /// item to the ship.
    /// </summary>
    public abstract void InitItem(Ship ship);

    /// <summary>
    /// Removes the mounted piece of equipment by disabling its effects on the ship.
    /// </summary>
    public abstract void RemoveItem(Ship ship);

    /// <summary>
    /// Applies the effects of this equipment item to the ship it's mounted on.
    /// This method is called from the ship's ShipEquipment component.
    /// </summary>
    public abstract void UpdateItem(Ship ship);

}
