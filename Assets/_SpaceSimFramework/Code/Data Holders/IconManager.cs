using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "DataHolders/Icon Manager")]
public class IconManager : ScriptableObject
{
    [Serializable]
    public struct NamedIcon
    {
        public string name;
        public Sprite icon;
    }

    public Sprite Placeholder;
    public NamedIcon[] Wares;
    public NamedIcon[] Equipment;
    public NamedIcon[] Ships;
    public NamedIcon[] Missions;
    public Sprite[] Weapons;
    public NamedIcon[] MarkerIcons;
    public Sprite[] players;

    private Dictionary<string, Sprite> _wareIcons;
    private Dictionary<string, Sprite> _equipmentIcons;
    private Dictionary<string, Sprite> _shipIcons;
    private Dictionary<string, Sprite> _missionIcons;
    private Dictionary<string, Sprite> _markerIcons;
    private Dictionary<string, Sprite> _playerIcons;
    private static IconManager _instance;

    public enum EquipmentIcons { Gun = 0, Turret = 1, Equipment = 2 }

    public static IconManager Instance
    {
        get
        {
            if(_instance == null)
                _instance = Resources.Load<IconManager>("IconManager");

            if (_instance == null)
                Debug.LogError("ERROR: IconManager not found! Asset must be in the Resources folder!");
            return _instance;
        }
    }

    private void init()
    {
        // Create a hashmap for O(1) access
        _wareIcons = new Dictionary<string, Sprite>();
        foreach(var pair in Wares)
        {
            _wareIcons.Add(pair.name, pair.icon);
        }

        _equipmentIcons = new Dictionary<string, Sprite>();
        foreach (var pair in Equipment)
        {
            _equipmentIcons.Add(pair.name, pair.icon);
        }

        _missionIcons = new Dictionary<string, Sprite>();
        foreach (var pair in Missions)
        {
            _missionIcons.Add(pair.name, pair.icon);
        }

        _shipIcons = new Dictionary<string, Sprite>();
        foreach (var pair in Ships)
        {
            _shipIcons.Add(pair.name, pair.icon);
        }

        _markerIcons = new Dictionary<string, Sprite>();
        foreach (var pair in MarkerIcons)
        {
            _markerIcons.Add(pair.name, pair.icon);
        }
    }

    public Sprite GetWareIcon(string itemName)
    {
        if (_wareIcons == null)
            init();

        return _wareIcons.ContainsKey(itemName) && _wareIcons[itemName] != null
            ? _wareIcons[itemName] : Placeholder;
    }

    public Sprite GetEquipmentIcon(string itemName)
    {
        if (_equipmentIcons == null)
            init();

        return _equipmentIcons.ContainsKey(itemName) && _equipmentIcons[itemName] != null
            ? _equipmentIcons[itemName] : Placeholder;
    }

    public Sprite GetMissionIcon(string jobName)
    {
        if (_missionIcons == null)
            init();

        return _missionIcons.ContainsKey(jobName) && _missionIcons[jobName] != null
            ? _missionIcons[jobName] : Placeholder;
    }

    public Sprite GetShipIcon(string shipName)
    {
        if (_shipIcons == null)
            init();

        return _shipIcons.ContainsKey(shipName) && _shipIcons[shipName] != null
            ? _shipIcons[shipName] : Placeholder;
    }

    public Sprite GetMarkerIcon(string objectTag)
    {
        if (_markerIcons == null)
            init();

        // Returning null here is desired behaviour due to different placeholder marker
        return _markerIcons.ContainsKey(objectTag) && _markerIcons[objectTag] != null
            ? _markerIcons[objectTag] : null;
    }

    public Sprite GetWeaponIcon(int index)
    {
        return Weapons[index];
    }
}
