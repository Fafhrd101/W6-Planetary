using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayArcadeStoreItem {
    public string item_id = "";
    public string application_credit_item_id = "";
    public string friendly_name = "";
    public string description = "";
    public int item_cost = 0;
    public string display_order = "";
    public string item_image = "";
    public string tags = "";

    public PlayArcadeStoreItem(
        string s_item_id,
        string s_application_credit_item_id,
        string s_friendly_name = "",
        string s_description = "",
        int i_item_cost = 0,
        string s_display_order = "",
        string s_item_image = "",
        string s_tags = ""
    ){
        item_id = s_item_id;
        application_credit_item_id = s_application_credit_item_id;
        friendly_name = s_friendly_name;
        description = s_description;
        item_cost = i_item_cost;
        display_order = s_display_order;
        item_image = s_item_image;
        tags = s_tags;
    }

}