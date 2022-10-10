
using System;  
using System.IO;  
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;  
using System.Text;  
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.WebSocket;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

struct SessionHash
{  
    public int n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, na;
    public string k1, k2, k3, k4, k5, k6, k7, k8, k9, k10, ka;
}   

public class CreatorCoin
{
    public string coinKind;
    public float coinBalance;
    public CreatorCoin(string _coinKind, float _coinBalance)
    {
        coinKind = _coinKind;
        coinBalance = _coinBalance;
    }
}

public class CreatorCredit
{
    public string kind;
    public float balance;
    public CreatorCredit(string _kind, float _balance)
    {
        kind = _kind;
        balance = _balance;
    }
}

public class PlayArcade : MonoBehaviour
{
    private List<CreatorCoin> userCoins = new List<CreatorCoin>();
    private List<CreatorCredit> userCredits = new List<CreatorCredit>();
    
    string application_user_id = "test"; 
    string application_user_name = "Player";

    //public Text buyMessageText;

    //private string endPoint = "https://theplayarcade.com/api";
    
    private bool localTest = false;

    private string APP_COIN_KIND = "";
    private string GAME_ID = "";
    private string API_KEY = "";
    private string SESSION_KEY = "";
    private string PUBLISHER_ID = ""; //NOTE: Change THIS

    public TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

    private Boolean doInteractiveGameSession = false;
    private Text gameSessionIDText;
    private Text gameSessionCommandText;

    private String GameSessionID = "";
    private int webSocketResetCounter=0;
    private List<WebSocket> webSocketList = new List<WebSocket>();
    //private int heartBeatCounter = 0;
    private DateTime lastHeartBeatTime;
    private bool bConnectedToNetwork = false;

    private string game_session_id="";
    private string game_session_code;

    private SessionHash session_hash;
    private List<int> k1n;
    private List<int> k2n;
    private List<int> k3n;
    private List<int> k4n;
    private List<int> k5n;

    public delegate void PA_OnAppStarted(bool success, string playerName, JSONObject gameInfo);
    public PA_OnAppStarted pa_OnAppStarted;

    public delegate void PA_EnableStartButton(bool enabled, string message="");
    public PA_EnableStartButton pa_EnableStartButton;

    public delegate void PA_StartGameSession(string k1, string k2, string k3, string k4, string k5);
    public PA_StartGameSession pa_StartGameSession;

    public delegate void PA_ScoreSubmitted(JSONObject scores);
    public PA_ScoreSubmitted pa_ScoreSubmitted;

    public delegate void PA_SessionUpdated();
    public PA_SessionUpdated pa_SessionUpdated;

    public delegate void PA_SessionEventSent(string eventValue);
    public PA_SessionEventSent pa_SessionEventSent;
    

    [DllImport("__Internal")]
    private static extern void OnAppReady();

    [DllImport("__Internal")]
    private static extern void RefreshUserInfo();

    void Awake()
    {
        DontDestroyOnLoad(this);
    }
    
    void Start()
    {
        #if (UNITY_WEBGL == true && UNITY_EDITOR == false)
            WebGLInput.captureAllKeyboardInput = false;
        #endif

        if(doInteractiveGameSession){
            InvokeRepeating(nameof(CheckNetwork), 0.5f, 5.0f);
        }

        if(gameSessionIDText) gameSessionIDText.text = "";
        if(gameSessionCommandText) gameSessionCommandText.text = "";
    }

    public void Init(string gameid, string apikey, string coinKind="PLAY", string appuserid="", string appusername=""){
        GAME_ID = gameid;
        API_KEY = apikey;

        if(GAME_ID == "" || GAME_ID == null){
            Debug.LogError("You must set the GAME_ID variable in PlayArcade class object.");
            return;
        }

        if (Application.isEditor)
        {
            APP_COIN_KIND = coinKind;
            application_user_id = appuserid;
            application_user_name = appusername;
            if(application_user_id == ""){
                Debug.LogError("You must set the GAME_ID variable in PlayArcade class object.");
                return;
            }
            
            PUBLISHER_ID = application_user_id;
            StartApp(application_user_id + "|" + application_user_name + "|" + APP_COIN_KIND + "|" + PUBLISHER_ID); 
        } else {
            StartCoroutine(Start_Delayed(0.25f));
        }
    }


    public IEnumerator Start_Delayed(float waitTime)
    {

        yield return new WaitForSeconds(waitTime);
        //This calls the hosting page to send us important context information
        OnAppReady();
    }


    //StartApp is called from the web page the game is hosted on
    public void StartApp(string startAppString)
    {
        string[] launchParams = startAppString.Split('|');
        string _userId = launchParams[0];
        string _userName = launchParams[1];
        string _coinKind = launchParams[2];
        string _publisherID = launchParams[3];
        
        TextInfo myTI = new CultureInfo("en-US",false).TextInfo;

        application_user_id = _userId.ToLower();
        application_user_name = myTI.ToTitleCase( _userName );
        APP_COIN_KIND = _coinKind.ToUpper();
        PUBLISHER_ID = _publisherID.ToLower();

        //Debug.Log("PlayArcade Init: " + startAppString);
        if(application_user_id != "free"){
            getGameInfo();
        } else {
            application_user_name = "Free Player";
            getGameInfo();
        }
    }

    private void OnInitialUserCredits(){
        //pa_OnAppStarted(application_user_name);
    }

    public void MainMenu()
    {
        if(doInteractiveGameSession){
            StartCoroutine(ConnectWhenReady(1.0f));
        }
    }

    //Continuously check the transaction to see if it's complete
    IEnumerator checkCoinTransaction(string transaction_id, float waitTime, bool repeat)
    {

        yield return new WaitForSeconds(waitTime);
        //Debug.Log("Checking transaction for: " + game_id + "  " + application_user_id + " " + transaction_id);

        string url = getEndPoint() + "/transaction/check";
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {
            Debug.Log(response.DataAsText);

            JSONObject jResponse = new JSONObject(response.DataAsText);
            bool bResponseSuccess = jResponse.GetField("success").b;
            if (bResponseSuccess)
            {
                JSONObject jTransactionData = jResponse.GetField("transaction_data"); //Array
                if (jTransactionData.Count > 0)
                {

                    string transactionStatus = jTransactionData[0].GetField("status").str.ToLower();

                    if (transactionStatus == "pending" || transactionStatus == "pendingapproval")
                    {
                        Debug.Log("Pending");
                        StartCoroutine(checkCoinTransaction(transaction_id, 3, true));
                    }
                    else
                    {
                        //buyMessageText.text = ti.ToTitleCase(transactionStatus);
                        Debug.Log("Complete");
                        getUserCredits();
                    }
                }
                else
                {
                    StartCoroutine(checkCoinTransaction(transaction_id, 3, true));
                }

            }
        });
        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("application_user_id", application_user_id);
        theRequest.AddField("transaction_id", transaction_id);

        theRequest.Send();

    }

    public float getCoinBalance(string coinKind)
    {
        float theBalance = 0.0f;

        foreach (CreatorCoin theCoin in userCoins)
        {
            if (theCoin.coinKind == coinKind)
            {
                return theCoin.coinBalance;
            }
        }

        return theBalance;
    }

    public float getCreditBalance(string kind)
    {
        float theBalance = 0.0f;
        foreach (CreatorCredit theCoin in userCredits)
        {
            if (theCoin.kind == kind.ToLower())
            {
                return theCoin.balance;
            }
        }

        return theBalance;
    }

    public void getGameInfo()
    {

        string url = getEndPoint() + "/game/info";
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {
            //Debug.Log(response.DataAsText);
            
            JSONObject jResponse = new JSONObject(response.DataAsText);
            JSONObject jData = jResponse.GetField("data");
            bool bResponseSuccess = true;

            string decString = DecryptString(jData.str, API_KEY);
            //Debug.Log(decString);
            JSONObject jResponseData = new JSONObject(decString);
            if (bResponseSuccess)
            {
                JSONObject jUserCredits = jResponseData.GetField("user_credits");
                processUserCredits(jUserCredits);
            }

            pa_OnAppStarted(bResponseSuccess, application_user_name, jResponseData);
            
        });
        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("publisher_id", PUBLISHER_ID);
        theRequest.AddField("application_user_id", application_user_id);
        theRequest.AddField("application_coin_kind", APP_COIN_KIND);
        theRequest.Send();
    }

    public void getUserCredits(System.Action callback = null)
    {

        string url = getEndPoint() + "/credits/balance";
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {
            //Debug.Log(response.DataAsText);

            JSONObject jResponse = new JSONObject(response.DataAsText);
            bool bResponseSuccess = true;

            if (bResponseSuccess)
            {
                JSONObject jUserCredits = jResponse.GetField("user_credits");
                processUserCredits(jUserCredits);
                
                #if (UNITY_WEBGL == true && UNITY_EDITOR == false)
                RefreshUserInfo();
                #endif

                if(callback != null){
                    callback();
                }
            }
        });
        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("application_user_id", application_user_id);
        theRequest.Send();
    }

    private void processUserCredits(JSONObject jUserCredits)
    {
        userCredits.Clear();
        for (int x = jUserCredits.list.Count - 1; x >= 0; x--)
        {
            JSONObject jUserCredit = jUserCredits.list[x];
            string kind = jUserCredit.GetField("coinKind").str.ToLower();
            float balance = jUserCredit.GetField("amount").f;

            //Debug.Log(kind + ": " + balance.ToString());
            CreatorCredit newCredit = new CreatorCredit(kind, balance);
            userCredits.Add(newCredit);
        }

        float mainBalance = getCreditBalance(APP_COIN_KIND);
        //print("Players balance is "+mainBalance);
        PlayArcadeIntegration.Instance.currentBalance = (int)mainBalance;
        //if(creditBalance) creditBalance.text = "$" + APP_COIN_KIND + " Credits: " + mainBalance.ToString("n0");
    }

    public void BuyCredits(int howMany)
    {
        //Debug.Log("Buying Credits:" + howMany.ToString());
        string url = getEndPoint() + "/oauth/buyItem";
        //if(buyMessageText) buyMessageText.text = "Authorizing Transaction...";
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {
            //Debug.Log(response.DataAsText);
            JSONObject jResponse = new JSONObject(response.DataAsText);
            bool bResponseSuccess = jResponse.GetField("success").b;
            if (bResponseSuccess)
            {
                //buyMessageText.text = "Waiting, check your Rally account email to approve the transaction.";
                JSONObject jResponseData = jResponse.GetField("data");
                JSONObject jResponseID = jResponseData.GetField("id");
                string transaction_id = jResponseID.str;
                Debug.Log("Incoming Transaction ID: " + transaction_id);
                StartCoroutine(checkCoinTransaction(transaction_id, 1, true));
            }
            else
            {
                //buyMessageText.text = "Problem with transaction, please try again.";
                Debug.Log("Invalid transaction");
            }

        });

        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("application_user_id", application_user_id);
        theRequest.AddField("application_coin_kind", APP_COIN_KIND);
        theRequest.AddField("item", APP_COIN_KIND + "_CREDITS_" + howMany.ToString());
        theRequest.AddField("notes", "Purchasing " + howMany.ToString() + " ");
        theRequest.Send();
    }

    IEnumerator EnableStartButton(float waitTime, string message = "")
    {
        yield return new WaitForSeconds(waitTime);
        pa_EnableStartButton(true, message);
    }

    IEnumerator FadeStatus(Text statusText, float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            statusText.CrossFadeAlpha(0.0f, 2.0f, false);
        }
    }

    public void StartGameRequest()
    {
        //Initiate a start game request on the server
        pa_EnableStartButton(false, "Requesting game start...");
        if(application_user_id == "free"){
            StartCoroutine(StartGame_Delayed(0));
        } else {
            BuyCreditItem("game_start", OnStartGameRequestComplete);
        }
    }


    private void OnStartGameRequestComplete(bool success, string application_credit_item_id, string message)
    {
        if (success)
        {
            pa_EnableStartButton(false, "Starting game...");
            StartCoroutine(StartGame_Delayed(0));
        }
        else
        {
            StartCoroutine(EnableStartButton(3, message));
        }
    }

    IEnumerator StartGame_Delayed(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        pa_EnableStartButton(false, "");
        StartGameSession();
    }

    private void StartGameSession(){
       
        pa_StartGameSession(session_hash.k1, session_hash.k2, session_hash.k3, session_hash.k4, session_hash.k5);
    }

    public void GetGameCustomizations(System.Action<bool, JSONObject> callback){
        
        //Debug.Log("Getting Game Customizations");
        string url = getEndPoint() + "/game/customizations_active"; 
        //Debug.Log(url);
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {

            Debug.Log(response.DataAsText);

            JSONObject jResponse = new JSONObject(response.DataAsText);
            JSONObject data = jResponse.GetField("data");
            bool bResponseSuccess = jResponse.GetField("success").b;

            callback(bResponseSuccess, data);

        });

        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("game_session_id", game_session_id);
        theRequest.AddField("publisher_id", PUBLISHER_ID);
        theRequest.AddField("application_user_id", application_user_id);
        theRequest.AddField("application_coin_kind", APP_COIN_KIND);
        theRequest.Send();
    }

    public void GetHighScores(System.Action<bool, JSONObject> callback, int limit = 25){
        
        Debug.Log("Getting HighScores");
        string url = getEndPoint() + "/game/scores"; 
        //Debug.Log(url);
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {
            //Debug.Log(response.DataAsText);
            JSONObject jResponse = new JSONObject(response.DataAsText);
            JSONObject data = jResponse.GetField("data");
            bool bResponseSuccess = jResponse.GetField("success").b;

            callback(bResponseSuccess, data);
        });

        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("limit", limit.ToString());
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("game_session_id", game_session_id);
        theRequest.AddField("publisher_id", PUBLISHER_ID);
        theRequest.AddField("application_user_id", application_user_id);
        theRequest.AddField("application_coin_kind", APP_COIN_KIND);
        theRequest.Send();
    }

    public void GetStoreData(System.Action<bool, JSONObject> callback){
        //Debug.Log("Getting Store Data");
        string url = getEndPoint() + "/game/store_items";
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {

            //Debug.Log(response.DataAsText);

            JSONObject jResponse = new JSONObject(response.DataAsText);
            JSONObject data = jResponse.GetField("data");
            bool bResponseSuccess = jResponse.GetField("success").b;
            callback(bResponseSuccess, data);

        });

        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("game_session_id", game_session_id);
        theRequest.AddField("publisher_id", PUBLISHER_ID);
        theRequest.AddField("application_user_id", application_user_id);
        theRequest.AddField("application_coin_kind", APP_COIN_KIND);
        theRequest.Send();
    }

    public void BuyCreditItem(string application_credit_item_id, System.Action<bool, string, string> callback)
    {
        //Debug.Log("Buying Credit Item:" + application_credit_item_id);
        string url = getEndPoint() + "/credits/buyCreditItem";
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {

            //Debug.Log(response.DataAsText);

            JSONObject jResponse = new JSONObject(response.DataAsText);
            string message = jResponse.GetField("message").str;

            bool bResponseSuccess = jResponse.GetField("success").b;
            if (bResponseSuccess)
            {
                JSONObject jUserCredits = jResponse.GetField("user_credits");
                processUserCredits(jUserCredits);
                
                #if (UNITY_WEBGL == true && UNITY_EDITOR == false)
                RefreshUserInfo();
                #endif

                if(application_credit_item_id.ToLower() == "game_start"){
                    
                    //On the game start, we also process the session info
                    JSONObject session_info = jResponse.GetField("session"); //Array
                    game_session_id = session_info.GetField("id").str;
                    game_session_code = session_info.GetField("md5").str;
                    SESSION_KEY = session_info.GetField("dev").str;
                    session_hash = processSessionCode(game_session_code);

                    callback(bResponseSuccess, application_credit_item_id, message);
                } else {
                    callback(bResponseSuccess, application_credit_item_id, message);
                }
                
            }
            else
            {
                callback(bResponseSuccess, application_credit_item_id, message);
            }

        });

        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("game_session_id", game_session_id);
        theRequest.AddField("publisher_id", PUBLISHER_ID);
        theRequest.AddField("application_user_id", application_user_id);
        theRequest.AddField("application_coin_kind", APP_COIN_KIND);
        theRequest.AddField("application_credit_item_id", application_credit_item_id);
        theRequest.Send();
    }


    /////////////////////////////////////////////
    /// Session/High Score Stuff
    /////////////////////////////////////////////
   
    private void SaveSessionData(string dataKey, string dataValue1, string key, string dataValue2=""){

        if(application_user_id.ToLower() == "free") {
            return;
        }
        
        if(!session_hash.ka.Contains(key)){
            Debug.Log("dataKey: " + dataKey );
            Debug.Log("Invalid Key");
            return;
        }
        
        //Debug.Log(dataValue1);
        string url = getEndPoint() + "/game/session/data";
        HTTPRequest theRequest = new HTTPRequest(new Uri(url), (request, response) =>
        {
            //Debug.Log(response.DataAsText);

            JSONObject jResponse = new JSONObject(response.DataAsText);
            if(jResponse.GetField("success")){
                bool bResponseSuccess = jResponse.GetField("success").b;
                if (bResponseSuccess)
                {
                    //Debug.Log("Session Data Saved");
                    if(jResponse.GetField("k")){
                        string operationName = jResponse.GetField("k").str;
                        //Debug.Log("Operation: " + operationName);
                        if(operationName == "SessionUpdate"){
                            pa_SessionUpdated();
                        }

                        if(operationName == "SessionEnd"){
                            JSONObject jScores = jResponse.GetField("scores");
                            pa_ScoreSubmitted(jScores);
                        }

                        if(operationName == "SessionEvent"){
                            string operationValue = jResponse.GetField("v").str;
                            pa_SessionEventSent(operationValue);
                        }
                    }
                }
            } else {
                Debug.Log("Invalid Session");
            }
        });

        string eDataKey = EncryptString(SESSION_KEY, dataKey);
        theRequest.MethodType = HTTPMethods.Post;
        theRequest.AddField("game_id", GAME_ID);
        theRequest.AddField("user_id", application_user_id);
        theRequest.AddField("session_id", game_session_id); 
        theRequest.AddField("k", eDataKey);
        theRequest.AddField("v1", dataValue1);
        theRequest.AddField("v2", dataValue2);
        theRequest.AddField("t", key);
        theRequest.Send();
    }


    private int CreateSessionData(int sessionData){
        int numParts = 2; //UnityEngine.Random.Range(3, 5);
        int numCores = 2; //UnityEngine.Random.Range(3, sessionData);
        k1n = AddCore(session_hash.n1, numParts);
        k2n = AddCore(session_hash.n2, numParts);
        k3n = AddCore(session_hash.n3, numParts);
        k4n = AddCore(sessionData, numParts);
        k5n = AddCore(numCores, numParts);
        return numParts;
    }

    public IEnumerator SendSessionKeys(float waitTime, string key){
        yield return new WaitForSeconds(waitTime);
        SaveSessionData("SessionKeys", session_hash.ka, key);
    }

    public void SendSessionUpdate(int numCores, string key, float waitTime = 0){
        StartCoroutine(_SendSessionUpdate(numCores, key, waitTime));
    }
    
    private IEnumerator _SendSessionUpdate(int numCores, string key, float waitTime){
        yield return new WaitForSeconds(waitTime);
        int numSegCore = CreateSessionData(numCores);
        string sendData = "";
        if(numCores >= 0) {
            for (int i = 0; i < numSegCore ; i++)
            {
                string sendData1 = CreateCoreSeg(i);
                //Debug.Log(sendData1);
                sendData = i > 0 ? sendData + "|" + sendData1 : sendData1;
            }
            sendData = EncryptString(SESSION_KEY, sendData);
            //Debug.Log(sendData);
            SaveSessionData("SessionUpdate", sendData, key);
        } else {
            sendData = EncryptString(SESSION_KEY, "0");
            SaveSessionData("SessionUpdate", sendData, key);
        }
    }

    public void SendSessionEvent(string eventName, string eventDetails, string key, float waitTime = 0){
        StartCoroutine(_SendSessionEvent(eventName, eventDetails, key, waitTime));
    }
    
    private IEnumerator _SendSessionEvent(string eventName, string eventDetails, string key, float waitTime){
        yield return new WaitForSeconds(waitTime);
        string esEventName = EncryptString(SESSION_KEY, eventName);
        string esDetails = EncryptString(SESSION_KEY, eventDetails);
        SaveSessionData("SessionEvent", esEventName, key, esDetails);
    }

    public void SendSessionScore(int score, string key, float waitTime = 1){
        StartCoroutine(_SendSessionScore(score, key, waitTime));
    }
    private IEnumerator _SendSessionScore(int score, string key, float waitTime = 0){
        yield return new WaitForSeconds(waitTime);
        SaveSessionData("SessionScore", score.ToString(), key);
    }

    public void SendSessionStats(string stats, string key, float waitTime = 0){
        StartCoroutine(_SendSessionStats(stats, key, waitTime));
    }
    private IEnumerator _SendSessionStats( string stats, string key, float waitTime = 0){
        yield return new WaitForSeconds(waitTime);
        SaveSessionData("SessionStats", stats, key);
    }

    public void SendSessionStart(string msg, string key, float waitTime = 0){
        StartCoroutine(_SendSessionStart(msg, key, waitTime));
    }
    private IEnumerator _SendSessionStart(string msg, string key, float waitTime = 0){
        yield return new WaitForSeconds(waitTime);
        SaveSessionData("SessionStart", "", key);
        StartCoroutine(SendSessionKeys(1,key));
    }

    
    public void SendSessionEnd(string msg, string key, float waitTime = 0){
        StartCoroutine(_SendSessionEnd(msg, key, waitTime));
    }
    private IEnumerator _SendSessionEnd(string msg, string key, float waitTime = 0){
        yield return new WaitForSeconds(waitTime);
        SaveSessionData("SessionEnd", msg, key);
    }

    private List<int> AddCore(int core, int seg){
        List<int> cores = new List<int>(); int segCore = 0;
        for (int i = 0; i < seg - 1; i++)
        {
            int newCore = UnityEngine.Random.Range(1, (core - segCore));
            cores.Add(newCore); segCore += newCore;
        }
        int finalCore = core - segCore; cores.Add(finalCore); cores.Sort();
        return cores;
    }

    private string CreateCoreSeg(int segCore){
        string coreDesc = ""; string vsp = "|"; string csp = ":";
        int instSplit = UnityEngine.Random.Range(1, 6);
        switch (instSplit){
            case 1: coreDesc = session_hash.k1 + csp + k1n[segCore] + vsp; coreDesc += session_hash.k2 + csp + k2n[segCore] + vsp; coreDesc += session_hash.k3 + csp + k3n[segCore] + vsp; coreDesc += session_hash.k4 + csp + k4n[segCore] + vsp; coreDesc += session_hash.k5 + csp + k5n[segCore]; break;
            case 2: coreDesc = session_hash.k2 + csp + k2n[segCore] + vsp; coreDesc += session_hash.k3 + csp + k3n[segCore] + vsp; coreDesc += session_hash.k4 + csp + k4n[segCore] + vsp; coreDesc += session_hash.k5 + csp + k5n[segCore] + vsp; coreDesc += session_hash.k1 + csp + k1n[segCore]; break;
            case 3: coreDesc = session_hash.k3 + csp + k3n[segCore] + vsp; coreDesc += session_hash.k4 + csp + k4n[segCore] + vsp; coreDesc += session_hash.k5 + csp + k5n[segCore] + vsp; coreDesc += session_hash.k1 + csp + k1n[segCore] + vsp; coreDesc += session_hash.k2 + csp + k2n[segCore]; break;
            case 4: coreDesc = session_hash.k4 + csp + k4n[segCore] + vsp; coreDesc += session_hash.k5 + csp + k5n[segCore] + vsp; coreDesc += session_hash.k1 + csp + k1n[segCore] + vsp; coreDesc += session_hash.k2 + csp + k2n[segCore] + vsp; coreDesc += session_hash.k3 + csp + k3n[segCore]; break;
            case 5: coreDesc = session_hash.k5 + csp + k5n[segCore] + vsp; coreDesc += session_hash.k1 + csp + k1n[segCore] + vsp; coreDesc += session_hash.k2 + csp + k2n[segCore] + vsp; coreDesc += session_hash.k3 + csp + k3n[segCore] + vsp; coreDesc += session_hash.k4 + csp + k4n[segCore]; break;
            default: coreDesc = session_hash.k3 + csp + k3n[segCore] + vsp; coreDesc += session_hash.k4 + csp + k4n[segCore] + vsp; coreDesc += session_hash.k5 + csp + k5n[segCore] + vsp; coreDesc += session_hash.k1 + csp + k1n[segCore] + vsp; coreDesc += session_hash.k2 + csp + k2n[segCore]; break;
        }
        return coreDesc;
    }

    private SessionHash processSessionCode(string code){
        //Debug.Log("Processing Session Hash");
        //Debug.Log(code);
        string blockOrder = code.Substring(0,1);
        string sessionInfo = code.Substring(1); string sep = "-";
        List<string> blocks = sessionInfo.Split('-').ToList();
        string block1 = ""; string block2 = ""; string block3 = ""; string block4 = ""; string block5 = "";
        
        switch(blockOrder){
            case "a": case "f": block1 = blocks[0]; block2 = blocks[1]; block3 = blocks[2]; block4 = blocks[3]; block5 = blocks[4]; break;
            case "b": case "g": block1 = blocks[4]; block2 = blocks[0]; block3 = blocks[1]; block4 = blocks[2]; block5 = blocks[3]; break;
            case "c": case "h": block1 = blocks[3]; block2 = blocks[4]; block3 = blocks[0]; block4 = blocks[1]; block5 = blocks[2]; break;
            case "d": block1 = blocks[2]; block2 = blocks[3]; block3 = blocks[4]; block4 = blocks[0]; block5 = blocks[1]; break;
            case "e": block1 = blocks[1]; block2 = blocks[2]; block3 = blocks[3]; block4 = blocks[4]; block5 = blocks[0]; break;
        }

        string n1 = Regex.Replace(block1, "[^0-9]", String.Empty); string n2 = Regex.Replace(block4, "[^0-9]", String.Empty); string n3 = Regex.Replace(block3, "[^0-9]", String.Empty);
        string k1 = block4.Substring(3 + n2.Length, 3); string k2 = block2.Substring(4,3); string k3 = block2.Substring(0,3); string k4 = block3.Substring(n3.Length +1, 3); string k5 = block5.Substring(0,3);

        SessionHash theValues = new SessionHash(); theValues.ka = k1 + sep + k2 + sep + k3 + sep + k4 + sep + k5;
        theValues.n1 = Convert.ToInt32(n1); theValues.n2 = Convert.ToInt32(n2); theValues.n3 = Convert.ToInt32(n3);
        theValues.k1 = k1; theValues.k2 = k2; theValues.k3 = k3; theValues.k4 = k4; theValues.k5 = k5;
        return theValues;
    }

    public void doLocalTest(bool which){
        localTest = which;
    }

    private string getEndPoint(){
        if(localTest)
            return "http://localhost:3000/api";
        
        return "https://theplayarcade.com/api";
    }
    

    /////////////////////////////////////////////
    /// Interactive Stuff
    /////////////////////////////////////////////

    void ConnectToWebSocket(string channelID){
        
        Debug.Log("Connecting to WebSocket AS " + channelID);
        var webSocket = new WebSocket(new Uri("wss://44l6i4zvq6.execute-api.us-east-1.amazonaws.com/Prod?gsId=" + channelID));
        
        webSocketList.Add(webSocket);

        //AddChannel(channelID, sChannelName, webSocket);

        int numWebSockets = webSocketList.Count;
        Debug.Log("Num websockets in list after new: " + numWebSockets);

        
        webSocket.OnOpen += OnWebSocketOpen;
        webSocket.OnError += OnWebSocketError; //This will also get hit when ping is not returned
        webSocket.OnClosed += OnWebSocketClosed;
        webSocket.OnBinary += OnBinaryMessageReceived;
        webSocket.OnMessage += OnMessageReceived;
        //webSocket.PingFrequency = 3000; //3 seconds
        //webSocket.StartPingThread = true;
        //webSocket.CloseAfterNoMessage = TimeSpan.FromSeconds(9);
        webSocket.Open();
        DelayedWebSocketReset(channelID);
    }

    private void OnWebSocketOpen(WebSocket webSocket)
    {        
        Debug.Log("WebSocket is now Open!");
        //SetStatus("Connected");
        if(gameSessionIDText) gameSessionIDText.text = "Game: " + GameSessionID;
    }

    private void OnBinaryMessageReceived(WebSocket webSocket, byte[] message)
    {
        Debug.Log("Binary Message received from server. Length: " + message.Length);
    }

    private void OnWebSocketClosed(WebSocket webSocket, UInt16 code, string message)
    {
        Debug.Log("WebSocket is now Closed!");
    }

    void OnWebSocketError(WebSocket ws, string error)
    {
        Debug.LogError("Error: " + error);
        Debug.LogError("Restarting in 3.");
       
        StartCoroutine(resetWebSocket(3.0f,GameSessionID));
    }

    void DelayedWebSocketReset(string channelID){
        //WebSocket will go idle after 10 minutes, and will be reset after 2 hours
        //So periodically reset it
        float timeOut = 90 * 60.0f;
        //float timeOut = 1 * 60.0f;
        StartCoroutine(resetWebSocket(timeOut, channelID));    
    }

    IEnumerator resetWebSocket(float waitTime, string channelID)
    {
        
        yield return new WaitForSeconds(waitTime);
        webSocketResetCounter++;
        Debug.Log("Resetting WebSocket " + webSocketResetCounter);

        int numWebSockets = webSocketList.Count;
        Debug.Log("Num old websockets in list: " + numWebSockets);

        
        if(webSocketList.Count > 0){
            WebSocket currentWebSocket = webSocketList.ElementAt(0);
            
            currentWebSocket.Close();

            webSocketList.Remove(currentWebSocket);
            //Destroy(currentWebSocket);
            currentWebSocket = null;
        }
                
        int numWebSockets2 = webSocketList.Count;
        Debug.Log("Num old websockets in list after delete: " + numWebSockets2);

        ConnectToWebSocket(channelID);
    }

    private void OnMessageReceived(WebSocket webSocket, string message)
    {
        Debug.Log("Message received from server: " + message);


        JSONObject jMessage = new JSONObject(message);
        string jEvent = jMessage.GetField("event").str;

        switch(jEvent){
            case "game_command":
                Debug.Log("Game Command");
                JSONObject jEventData = jMessage.GetField("event_data");
                if (jEventData)
                {
                    string eventData = jEventData.str;
                    Debug.Log("Event Data:" + eventData);
                    if(gameSessionCommandText) gameSessionCommandText.text = eventData;

                    SendToConnectedClients("Go Screw");
                }
            break;
        }
    }

    private void SendToConnectedClients(string message){
         WebSocket currentWebSocket = webSocketList.ElementAt(0);
         JSONObject newCommand = new JSONObject("{\"event\":\"ui_command\",\"event_data\":\"command100\"}");
         string jsonString = "data1";
         string data = "{\"action\":\"sendmessage\", \"data\":\"" + jsonString + "\", \"gsId\":\"" + GameSessionID + "\"}";

         Debug.Log(data);
         currentWebSocket.Send(data);
    }

    public void CheckNetwork() {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            bConnectedToNetwork = false;
        } else {
            bConnectedToNetwork = true;
        }
    }

    private string EncryptString(string keyString, string plainText)  {  
        byte[] cipherData;
        Aes aes = Aes.Create();
        keyString = keyString.Replace("-", string.Empty);
        aes.Key = Encoding.UTF8.GetBytes(keyString);
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        ICryptoTransform cipher = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, cipher, CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
            }

            cipherData = ms.ToArray();
        }

        byte[] combinedData = new byte[aes.IV.Length + cipherData.Length];
        Array.Copy(aes.IV, 0, combinedData, 0, aes.IV.Length);
        Array.Copy(cipherData, 0, combinedData, aes.IV.Length, cipherData.Length);
        return Convert.ToBase64String(combinedData);
    }  

    private string DecryptString(string combinedString, string keyString)  {  
        string plainText;
        keyString = keyString.Replace("-", string.Empty);
        byte[] combinedData = Convert.FromBase64String(combinedString);
        Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(keyString);
        byte[] iv = new byte[aes.BlockSize / 8];
        byte[] cipherText = new byte[combinedData.Length - iv.Length];
        Array.Copy(combinedData, iv, iv.Length);
        Array.Copy(combinedData, iv.Length, cipherText, 0, cipherText.Length);
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        ICryptoTransform decipher = aes.CreateDecryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream(cipherText))
        {
            using (CryptoStream cs = new CryptoStream(ms, decipher, CryptoStreamMode.Read))
            {
                using (StreamReader sr = new StreamReader(cs))
                {
                    plainText = sr.ReadToEnd();
                }
            }

            return plainText;
        }
    }  

    IEnumerator ConnectWhenReady(float delay){
        yield return new WaitForSeconds(delay);
        if(bConnectedToNetwork){
            GameSessionID = Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty).Substring(0, 6).ToUpper();
            Debug.Log("GameSessionID: " + GameSessionID);
            ConnectToWebSocket(GameSessionID);
        } else {
           StartCoroutine(ConnectWhenReady(delay));    
        }   
    }
    public void RefreshUserBalances()
    {
#if (UNITY_WEBGL == true && UNITY_EDITOR == false)
    StartCoroutine(RefreshUserBalances(5.0f));
#endif
    }

    IEnumerator RefreshUserBalances(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        RefreshUserInfo();
    }
}


