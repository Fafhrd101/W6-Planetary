using UnityEngine;

/// <summary>
/// This is a mountable equipment item which can be installed on a ship. It applies
/// certain effects to the carrier ship.
/// </summary>
public abstract class Consumables: ScriptableObject
{
    // Cost of the item
    public abstract int Cost { get; }
    public string ItemName;
    [TextArea]
    public string Description;

}
