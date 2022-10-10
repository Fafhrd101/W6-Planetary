using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
 
public static class GameInputManager
{
    static Dictionary<string, KeyCode> keyMapping;
    static string[] keyMaps = new string[6]
    {
        "Attack",
        "Block",
        "Forward",
        "Backward",
        "Left",
        "Right"
    };
    static KeyCode[] defaults = new KeyCode[6]
    {
        KeyCode.Q,
        KeyCode.E,
        KeyCode.W,
        KeyCode.S,
        KeyCode.A,
        KeyCode.D
    };
 
    static GameInputManager()
    {
        InitializeDictionary();
    }
 
    private static void InitializeDictionary()
    {
        keyMapping = new Dictionary<string, KeyCode>();
        for(int i=0;i<keyMaps.Length;++i)
        {
            keyMapping.Add(keyMaps[i], defaults[i]);
        }
    }
 
    public static void SetKeyMap(string keyMap,KeyCode key)
    {
        if (!keyMapping.ContainsKey(keyMap))
            throw new ArgumentException("Invalid KeyMap in SetKeyMap: " + keyMap);
        keyMapping[keyMap] = key;
    }
 
    public static bool GetKeyDown(string keyMap)
    {
        return Input.GetKeyDown(keyMapping[keyMap]);
    }
    
    
    // Other files....
    // void Update()
    // {
    //     if (GameInputManager.GetKeyDown("attack"))
    //     {
    //         // attack code
    //     }
    // }
}