using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayArcadeScoreItem
{
    public string game_session_id = "";
    public string coin_kind = "";
    public int score = 0;
    public string stats = "";
    public string message = "";
    public string publisher_id = "";
    public string user_name = "";
    public string user_id = "";
    public string date_created = "";

    public PlayArcadeScoreItem(
        string s_game_session_id,
        string s_coin_kind,
        int i_score = 0,
        string s_stats = "",
        string s_message = "",
        string s_publisher_id = "",
        string s_user_name = "",
        string s_user_id = "",
        string s_date_created = ""
    ){
        game_session_id = s_game_session_id;
        coin_kind = s_coin_kind;
        score = i_score;
        stats = s_stats;
        message = s_message;
        publisher_id = s_publisher_id;
        user_name = s_user_name;
        user_id = s_user_id;
        date_created = s_date_created;
    }
}
