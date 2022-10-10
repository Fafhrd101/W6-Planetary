using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SavedUniverse : Singleton<SavedUniverse>
{
    public List<SerializableUniverseSector> Sectors;
    public int UniverseSectorCount;
    public int SavedSectorCount;
    
    public void Awake()
    {
        //Universe.ClearUniverse();
        //Universe.LoadUniverse();
        UniverseSectorCount = Universe.Sectors.Count;
        Sectors = new List<SerializableUniverseSector>();
        foreach (var obj in Universe.Sectors)
        {
            SavedUniverse.Instance.Sectors.Add(obj.Value);
        }
        //Sectors = Sectors.OrderByDescending(ch => ch.Name).ToList();
        SavedSectorCount = SavedUniverse.Instance.Sectors.Count;
    }
}
