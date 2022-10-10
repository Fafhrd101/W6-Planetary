using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

//[ExecuteAlways]
public class PlayArcadeIntegration : MonoBehaviour
{
    
    public PlayArcade playArcade;
    [Tooltip("You can manually set this, or just let it auto-generate. Every time you turn ON dev build, it will advance the version slightly.")]
    public float gameVersion = 0.01f;
    public TMPro.TMP_Text versionText;

    [Tooltip("Start button begins the entire process. Use the included, or duplicate your own.")]
    public GameObject startGameButton;
    [Tooltip("Start button will auto-load this scene if present.")]
    public string startScene = "Game";
    [HideInInspector]
    public int currentBalance;

    [Tooltip("TEST_USER_ID can be your developer ID from the PlayArcade Dev Console https://theplayarcade.com/developer/games")]
    public string TEST_USER_ID = "";
    [Tooltip("From the PlayArcade Dev Console")]
    public string GAME_ID = ""; 
    [Tooltip("From the PlayArcade Dev Console")]
    public string API_KEY = "";
    // Best results if you just leave this as is. 
    private string TEST_USER_NAME = "DevPlayer"; 
    [Tooltip("Use PLAY or TEST coin for testing! Live games will auto-change this to that particular Creator's coin.")]
    public string creatorCoin = "PLAY";

    [Tooltip("The most important aspect, true if you're testing at home, false if you're building for an upload.")]
    public bool devBuild = false;
    [Tooltip("Wander-6 specific, load sectors from local copy, or playarcade server")]
    public bool localLoads = false;
    [HideInInspector]
    public bool loadAutosave = false;
    [HideInInspector]
    public bool isInitialized = false;
    [Tooltip("Your logged in name")]
    [HideInInspector]
    public string playerName = "";
    public TMPro.TMP_Text playerNameText;
    
    private string _k1,_k2,_k3,_k4,_k5;

    // Stored items, available to anyone at any time
    private JSONObject _gameCustomizationData;
    private JSONObject _storeData;
    private JSONObject _scribeData;
    private JSONObject _coinScoreData;
    private JSONObject _userCoinScoreData;
    public List <PlayArcadeStoreItem> storeItems = new List<PlayArcadeStoreItem>();
    public List <PlayArcadeGameCustomization> gameCustomizations = new List<PlayArcadeGameCustomization>();
    public List <PlayArcadeScoreItem> coinScores = new List<PlayArcadeScoreItem>();
    public List <PlayArcadeScoreItem> userCoinScores = new List<PlayArcadeScoreItem>();

    // Coins exist in every game, so we can grab them here using a common reference
    private const string CoinName = "coin_image";
    public Texture2D coinTexture;
    public Sprite coinSprite;
    [HideInInspector]
    public Material coinMaterial;
    public Texture2D nftTexture;
    public Sprite nftSprite;
    [HideInInspector]
    public Material nftMaterial;
    [Tooltip("How many NFTs player has in this collection")]
    public int nftInCollection;
    [HideInInspector]
    public int nftSelected;
    private int _numScoresToRetrieve = 10;
    [HideInInspector]
    public int lastScore;
    [HideInInspector]
    public int playerRank;
    [Tooltip("Let's you know the SubmitScore() call was successful.")]
    [HideInInspector]
    public bool scoresRetrieved;
    // [Tooltip("If you move the shop, let us know where it's at.")]
    // public PlayShop shop;
    // [Tooltip("A broadcast message will be sent to this object. Storelistener().")]
    // public GameObject storeListener;
    
    // Current instance management
    private static PlayArcadeIntegration _current;
    public static PlayArcadeIntegration Instance
    {
        get
        {
            if(_current == null)
                Debug.LogError("Attempting to access PlayArcadeIntegration before it's been created.");
            return _current;
        }
    }
 
    void Awake ()
    {
        if (_current != null)
            DestroyImmediate(_current.gameObject);

        _current = this;
    }
 
    void OnDestroy ()
    {
        _current = null;
    }
    
    void Start()
    {
        playerNameText.text = playerName;
        versionText.text = "V. " + gameVersion.ToString(CultureInfo.CurrentCulture);
        StartPlayArcade();
        isInitialized = true;
        scoresRetrieved = false;
        startGameButton.SetActive(true);
    }

    public void Update()
    {
        if (playArcade == null)
            playArcade = this.GetComponent<PlayArcade>();
    }

    private void StartPlayArcade()
    {
        if(playArcade == null){
            Debug.LogError("playArcade Object is not set");
            return;
        } 
        if(GAME_ID == ""){
            Debug.LogError("You must set the GAME_ID variable in PlayArcade class object.");
            return;
        }
        if(API_KEY == ""){
            Debug.LogError("You must set the API_KEY variable in PlayArcade class object.");
            return;
        }

        playArcade.pa_OnAppStarted = OnAppStarted;
        playArcade.pa_StartGameSession = StartGameSession;
        playArcade.pa_SessionUpdated = OnSessionUpdated;
        playArcade.pa_SessionEventSent = OnSessionEventSent;
        playArcade.pa_ScoreSubmitted = OnScoreSubmitted;
        
        if (devBuild == false)
        {
            playArcade.Init(GAME_ID, API_KEY, creatorCoin);
        }
        else
        {
            playArcade.Init(GAME_ID, API_KEY, creatorCoin, TEST_USER_ID, TEST_USER_NAME);
        }
        // This is the first log you should be seeing
        Debug.Log("Requesting initialization...");
    }

    // Play Arcade is fully initialized, we can begin reading data from the server
    private void OnAppStarted(bool success, string serverName, JSONObject gameInfo){

        Debug.Log("   ... granted");

        this.playerName = serverName;
        playerNameText.text = serverName;
        if (!success)
        {
            Debug.LogError("Yeah, something failed here.");
            return;
        }
        var jStoreData = gameInfo.GetField("store_items");
        _storeData = jStoreData;
        processStoreItems();

        var jCustomizationData = gameInfo.GetField("customizations");
        _gameCustomizationData = jCustomizationData;
        processCustomizations();

        var jCoinScoreData = gameInfo.GetField("game_scores_coin");
        _coinScoreData = jCoinScoreData;
        processCoinScores();

        var jUserScoreData = gameInfo.GetField("game_scores_user");
        _userCoinScoreData = jUserScoreData;
        processUserCoinScores();

        var jScribeData = gameInfo.GetField("scribe");
        _scribeData = jScribeData;
        processScribeData(0);

        MainMenu();
    }

    private void MainMenu()
    {
        var theCustomization = gameCustomizations.Find(x => x.customization_name.ToLower() == CoinName.ToLower());
        var theURL = theCustomization.customization_value;

        StartCoroutine(GetGameTexture(theURL, true, coinMaterial));
        Debug.Log("Completed Community Coin downloads");

        StartCoroutine(GetGameTexture(PlayNFT.Instance.imageURL, false, nftMaterial));
        Debug.Log("Completed Player SuperPassNFT downloads");
    }
    
    //Point your Start/Play button here to kick it off! Named Button<> for easier finding in the dropdown list
    public void ButtonStartGameRequest()
    {
        if (startScene != "")
            SceneManager.LoadScene(startScene);
    }

    //This is the callback from the StartGameRequest
    private void StartGameSession(string sk1, string sk2, string sk3, string sk4, string sk5){
        
        _k1=sk1;_k2=sk2;_k3=sk3;_k4=sk4;_k5=sk5;
        string someSessionDescription = "Lobby Level";
        playArcade.SendSessionStart(someSessionDescription, _k1);
    }

    public void SendGameEventTest(){
        SendGameEvent("Exp gained", playerName + " checkpointed", _k3, 0f);
    }

    private void SendGameEvent(string eventName, string eventDetails, string skey, float delay){
        playArcade.SendSessionEvent(eventName, eventDetails, skey, 0);
    }   

    //When player is done with round, send in the score
    public void SubmitScore(int score, string sessionStats="")
    {
        playArcade.SendSessionScore(score, _k3, 0);
        playArcade.SendSessionStats(sessionStats, _k4, 0);
        playArcade.SendSessionUpdate(score, _k2, 0);
    }

    private void OnSessionUpdated(){
        var msg = "Session Updated. Maybe. Dunno if this is ever seen...";
        playArcade.SendSessionEnd(msg, _k5, 0);
    }

    private void OnScoreSubmitted(JSONObject scoreData){
        var jScores = scoreData.GetField("game_scores_coin");
        var scoreRank = (int) scoreData.GetField("game_score_user_rank").i;
        playerRank = scoreRank;
        print("Score submitted. Players rank: "+ scoreRank);
        _coinScoreData = jScores;
        processCoinScores();
        scoresRetrieved = true;
    }

    private void OnSessionEventSent(string eventName){
        Debug.Log("Session Event Sent!: " + eventName);
    }

    //Customizations
    public void GetCustomizations(){
        playArcade.GetGameCustomizations(OnCustomizationsRetrieved);
    }

    private void OnCustomizationsRetrieved(bool success, JSONObject gameCustomizations)
    {
        if (success)
        {
            Debug.Log("Game customizations retrieved!");
            _gameCustomizationData = gameCustomizations;
            processCustomizations();
        }
        else
        {
            Debug.Log("Error fetching game customizations");
        }
    }
    
    private void processCustomizations(){
        gameCustomizations.Clear();
        JSONObject jItems = _gameCustomizationData;
        //Debug.Log(jItems.ToString());
        foreach (JSONObject jItem in jItems.list)
        {
            var itemID = jItem.GetField("id").str;
            var customization_id = jItem.GetField("id").str;
            var customization_base = jItem.GetField("base").str;
            var customization_name = jItem.GetField("name").str;
            var customization_type = jItem.GetField("type").str;
            var customization_value = jItem.GetField("value").str;
            PlayArcadeGameCustomization newCustomization = new PlayArcadeGameCustomization(customization_id, customization_base, customization_name, customization_type, customization_value);
            gameCustomizations.Add(newCustomization);

            if (customization_name == "UniverseFile")
                if (!localLoads)
                    StartCoroutine(GetUniverseFile(customization_value));
        }
        //print("Loaded "+jItems.list.Count+" customizations");
    }

    public void LoadSectorIntoScene(string which)
    {
        foreach (var t in gameCustomizations)
        {
            if (t.customization_name == which)
            {
                //print("LoadSectorIntoScene() Found sector file at "+GameCustomizations[i].customization_value);
                StartCoroutine(LoadSectorFile(t.customization_value, which));
                break;
            }
        }
        // print("Sector file not found! (Did the download work?)");
    }

    private IEnumerator LoadSectorFile(string theURL, string fileName) {
        var uwr = new UnityWebRequest(theURL, UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();
        if(uwr.result > UnityWebRequest.Result.Success)
            Debug.LogError("GetSectorFile():"+uwr.error);
        else
        {
            //Debug.Log("GetSectorFile() Loading " + path + " server sectorfile");
            SectorLoader.LoadSectorIntoScene(path);
        }
    }

    private IEnumerator GetUniverseFile(string theURL) {
        var uwr = new UnityWebRequest(theURL, UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath, "Universe");
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();
        if(uwr.result > UnityWebRequest.Result.Success)
            Debug.LogError(uwr.error);
        else
        {
            //Debug.Log("Universe successfully downloaded and saved to " + path);
            
            Dictionary<SerializableVector2, SerializableUniverseSector> data =
                (Dictionary<SerializableVector2, SerializableUniverseSector>)Utils.LoadBinaryFile(path);
            Universe.Sectors = data;
        }
    }
 
    public IEnumerator GetGameTexture(string theURL, bool isCoin, Material matToChange) {
        var uwr = UnityWebRequestTexture.GetTexture(theURL);
        yield return uwr.SendWebRequest();

        var tex = ((DownloadHandlerTexture) uwr.downloadHandler).texture;
        if(uwr.result > UnityWebRequest.Result.Success)
            Debug.LogError(uwr.error);
        else
        {
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(tex.width / 2f, tex.height / 2f));
            if (isCoin) coinSprite = sprite;
            else nftSprite = sprite;
            matToChange.mainTexture = tex;
        }
    }

    #region IAP
    
    //IAP Samples
    public void GetStoreData(){
        playArcade.GetStoreData(OnStoreDataRetrieved);
    }

    private void OnStoreDataRetrieved(bool success, JSONObject _storeData)
    {
        if (success)
        {
            Debug.Log("Store Data retrieved!");
            this._storeData = _storeData;
            processStoreItems();
        }
        else
        {
            Debug.Log("Error fetching Store Data");
        }
    }

    private void processStoreItems(){
        storeItems.Clear();
        JSONObject jStoreItems = _storeData; //.GetField("store_items");
        //Debug.Log(jStoreItems.ToString());
        foreach (var jStoreItem in jStoreItems.list)
        {
            //string item_id = jStoreItem.GetField("id").str.ToLower();
            string itemID = jStoreItem.GetField("application_credit_item_id").str;
            string applicationCreditItemID = jStoreItem.GetField("application_credit_item_id").str;
            string friendly_name = jStoreItem.GetField("friendly_name").str;
            string description = jStoreItem.GetField("description").str;
            int item_cost = (int)jStoreItem.GetField("item_cost").i;
            string display_order = jStoreItem.GetField("display_order").str;
            string item_image = jStoreItem.GetField("item_image").str;
            string tags = jStoreItem.GetField("tags").str;
            PlayArcadeStoreItem newStoreItem = new PlayArcadeStoreItem(itemID, applicationCreditItemID, friendly_name, description, item_cost, display_order, item_image, tags);
            storeItems.Add(newStoreItem);

            //Debug.Log("Store Item:" + application_credit_item_id + " Cost: " + item_cost.ToString());
        }
        //Debug.Log("Number store items retrieved:" + StoreItems.Count());
    }

    public void BuyAThing(string thingName){
        playArcade.BuyCreditItem(thingName, OnBuyItemComplete);
    }

    private void OnBuyItemComplete(bool success, string application_credit_item_id, string message)
    {
        //if(buyMessageText) buyMessageText.text = message;
        if (success)
        {
            // // string text = "Purchase of " + application_credit_item_id + " successful! " + message;
            // // print(text);
            // storeListener.BroadcastMessage("ShopListener", application_credit_item_id);
            // if (shop != null)
            // {
            //     shop.confirmationText.text = message;
            //     shop.StartCoroutine("CloseConfirmation");
            // }
        }
        else
        {
            Debug.Log("Purchase of " + application_credit_item_id + " failed! " + message);
        }
    }
    #endregion
        // Single player
    private void processCoinScores()
    {
        coinScores.Clear();
        JSONObject jScoreItems = _coinScoreData;
        //Debug.Log(jStoreItems.ToString());
        foreach (JSONObject jScoreItem in jScoreItems.list)
        {
            var game_session_id = jScoreItem.GetField("game_session_id").str.ToLower();
            var coin_kind = jScoreItem.GetField("coin_kind").str;
            var stats = jScoreItem.GetField("stats").str;
            var message = jScoreItem.GetField("message").str;
            var score = (int)jScoreItem.GetField("score").i;
            var publisher_id = jScoreItem.GetField("publisher_id").str;
            var user_name = jScoreItem.GetField("user_name").str;
            var user_id = jScoreItem.GetField("user_id").str;
            var date_created = jScoreItem.GetField("date_created").str;
            PlayArcadeScoreItem newScoreItem = new PlayArcadeScoreItem(
                game_session_id, 
                coin_kind, 
                score,
                stats,
                message,
                publisher_id,
                user_name,
                user_id,
                date_created
            );
            coinScores.Add(newScoreItem);
            //Debug.Log("Score: " + score + "User: "+user_name);
        }
        //Debug.Log("Number coin scores:" + CoinScores.Count());
    }
    
        // Leaderboard
    private void processUserCoinScores(){
         userCoinScores.Clear();
         JSONObject jScoreItems = _userCoinScoreData; //.GetField("store_items");
         //Debug.Log(jScoreItems.ToString());
         foreach (JSONObject jScoreItem in jScoreItems.list)
         {
             var game_session_id = jScoreItem.GetField("game_session_id").str.ToLower();
             var coin_kind = jScoreItem.GetField("coin_kind").str;
             var stats = jScoreItem.GetField("stats").str;
             var message = jScoreItem.GetField("message").str;
             var score = (int)jScoreItem.GetField("score").i;
             var publisher_id = jScoreItem.GetField("publisher_id").str;
             var user_name = jScoreItem.GetField("user_name").str;
             var user_id = jScoreItem.GetField("user_id").str;
             var date_created = jScoreItem.GetField("date_created").str;
             PlayArcadeScoreItem newScoreItem = new PlayArcadeScoreItem(
                 game_session_id, 
                 coin_kind, 
                 score,
                 stats,
                 message,
                 publisher_id,
                 user_name,
                 user_id,
                 date_created
             );
             userCoinScores.Add(newScoreItem);
             //Debug.Log("Score Item:" + application_credit_item_id + " Cost: " + item_cost.ToString());
         }
         //Debug.Log("Number user coin scores:" + UserCoinScores.Count());
     }

    public void GetHighScores(){
        playArcade.GetHighScores(OnHighScoreDataRetrieved, _numScoresToRetrieve);
    }

    private void OnHighScoreDataRetrieved(bool success, JSONObject _scoreData)
    {
        if (success)
        {
            Debug.Log("HighScore data retrieved!");
            JSONObject jScores = _scoreData.GetField("gamePublisherScores");
            _coinScoreData = jScores;
            processCoinScores();
        }
        else
        {
            Debug.Log("Error fetching Score Data");
        }
    }
    
    private void processUserRallyNFTs()
    {
        return;
        // UserNFTs_Rally.Clear();
        // JSONObject jUserNFTItems_Rally = userNFTData_Rally;
        // //Debug.Log(jUserNFTItems_Rally.ToString());
        // PlayArcadeUserNFTItemRally lastItem = null;
        // foreach (JSONObject jItem in jUserNFTItems_Rally.list)
        // {
        //
        //
        //     string nftID = jItem.GetField("id").str.ToLower();
        //     string nftTemplateId = jItem.GetField("nftTemplateId").str;
        //     string rallyNetworkWalletId = jItem.GetField("rallyNetworkWalletId").str;
        //     string status = jItem.GetField("status").str;
        //     int editionNumber = (int)jItem.GetField("editionNumber").i;
        //     string title = jItem.GetField("title").str;
        //     string description = jItem.GetField("description").str;
        //     string media_URL = jItem.GetField("media").GetField("url").str;
        //     string media_TYPE = jItem.GetField("media").GetField("type").str;
        //     PlayArcadeUserNFTItemRally newItem = new PlayArcadeUserNFTItemRally(
        //         nftID, 
        //         nftTemplateId, 
        //         rallyNetworkWalletId,
        //         status,
        //         editionNumber,
        //         title,
        //         description,
        //         media_URL,
        //         media_TYPE
        //     );
        //     UserNFTs_Rally.Add(newItem);
        //     lastItem = newItem;
        //     Debug.Log("NFT Item:" + nftID + " Media TYPE: " + media_TYPE + " Media URL: " + media_URL);
        // }
        // Debug.Log("Number user NFTS (Rally):" + UserNFTs_Rally.Count());
    }
    
    public void processScribeData(int which = 0)
    {
        JSONObject jScribeData = _scribeData;
        int userMetaverseLevel = (int)jScribeData.GetField("metaverse_level").i;
        int userMetaverseXP = (int)jScribeData.GetField("metaverse_xp").i;

        JSONObject jInventory = _scribeData.GetField("inventory");
        JSONObject jPowerUps = jInventory.GetField("powerups");
        JSONObject jArtifacts = jInventory.GetField("artifacts");
        
        nftInCollection = jArtifacts.Count;
        //print(nftInCollection+" NFTs in users collection");

        var count = 0;
        if (jArtifacts.Count > 0)
        {
            foreach (JSONObject jItem in jArtifacts.list)
            {
                var itemID = jItem.GetField("id").str;
                var name = jItem.GetField("name").str;
                var level = (int) jItem.GetField("level").i;
                var type = jItem.GetField("type").str;
                var iURL = jItem.GetField("image_url").str;

                PlayNFT.Instance.collectionName = name;
                PlayNFT.Instance.imageURL = iURL;

                JSONObject jTraits = jItem.GetField("traits");
                if (type == "Superpass")
                {
                    var sp_stat_xp = 1;
                    var sp_stat_power = 1;
                    var sp_stat_luck = 1;
                    var sp_stat_health = 1;
                    var sp_stat_speed = 1;
                    var sp_stat_score = 1;
                    var sp_stat_diamonds = 0;
                    var sp_background = "";
                    var sp_rosetta = "";
                    var sp_artifact = "";
                    foreach (JSONObject jTrait in jTraits.list)
                    {
                        string sTraitName = jTrait.GetField("name").str;
                        switch (sTraitName.ToLower())
                        {
                            case "xp":
                                sp_stat_xp = int.Parse(jTrait.GetField("value").str);
                                break;
                            case "power":
                                sp_stat_power = int.Parse(jTrait.GetField("value").str);
                                break;
                            case "luck":
                                sp_stat_luck = int.Parse(jTrait.GetField("value").str);
                                break;
                            case "health":
                                sp_stat_health = int.Parse(jTrait.GetField("value").str);
                                break;
                            case "speed":
                                sp_stat_speed = int.Parse(jTrait.GetField("value").str);
                                break;
                            case "score":
                                sp_stat_score = int.Parse(jTrait.GetField("value").str);
                                break;
                            case "diamonds":
                                sp_stat_diamonds = int.Parse(jTrait.GetField("value").str);
                                break;
                            case "background":
                                sp_background = jTrait.GetField("value").str;
                                break;
                            case "rosetta":
                                sp_rosetta = jTrait.GetField("value").str;
                                break;
                            case "artiface":
                                sp_artifact = jTrait.GetField("value").str;
                                break;
                        }
                    }

                    PlayNFT.Instance.health = sp_stat_health;
                    PlayNFT.Instance.xp = sp_stat_xp;
                    PlayNFT.Instance.luck = sp_stat_luck;
                    PlayNFT.Instance.power = sp_stat_power;
                    PlayNFT.Instance.score = sp_stat_score;
                    PlayNFT.Instance.speed = sp_stat_speed;
                }

                if (which == count)
                    break;

                count++;
            }
        }
        else
        {
            PlayNFT.Instance.health = 0;
            PlayNFT.Instance.xp = 0;
            PlayNFT.Instance.luck = 0;
            PlayNFT.Instance.power = 0;
            PlayNFT.Instance.score = 0;
            PlayNFT.Instance.speed = 0;
            PlayNFT.Instance.collectionName = "No SuperPass found";
            PlayNFT.Instance.imageURL = "";
            Sprite sprite = Sprite.Create(nftTexture, new Rect(0, 0, nftTexture.width, nftTexture.height),
                new Vector2(nftTexture.width / 2f, nftTexture.height / 2f));
            nftSprite = sprite;
        }
        
        PlayNFT.Instance.ApplyValues();
    }

}
