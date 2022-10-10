using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayArcadeGameCustomization {
    public string customization_id = "";
    public string customization_base = "";
    public string customization_name = "";
    public string customization_type = "";
    public string customization_value = "";
    
    public PlayArcadeGameCustomization(
        string s_customization_id, 
        string s_customization_base, 
        string s_customization_name, 
        string s_customization_type, 
        string s_customization_value)
    {
        customization_id = s_customization_id;
        customization_base = s_customization_base;
        customization_name = s_customization_name;
        customization_type = s_customization_type;
        customization_value = s_customization_value;
    }
}