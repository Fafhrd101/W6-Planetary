using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorObject : MonoBehaviour
{
    // Unique in-game object ID 
    public string ID;
    public string sectorName;
    public string ownerID;
    [Tooltip("SectorObject type data holder")]
    public SectorObjectLoadout loadout;
    
    private void Start()
    {
        if(loadout != null)
            SectorObjectLoadout.ApplyLoadoutToSectorObject(loadout, this);
    }
}
