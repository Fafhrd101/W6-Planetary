using UnityEngine;
using System.Collections;
using DunGen.DungeonCrawler;

public class 
    UsableDevice : MonoBehaviour, IClickableObject
{

    [Header("Settings")]
    public bool IsEnabled = true;
	public GameObject AffectedTarget;
	public string MessageToSend = "Action";
    public float UseDelay = 1.0f;
    
    [Tooltip("The cursor to use when the player hovers over it with the mouse")]
    public Texture2D hoverCursor = null;
    [Tooltip("How close the player has to be to interact. If further away, the player will run to it.")]
    public float interactDistance = 2f;
    [Tooltip("An image that's rendered to the minimap, if desired")]
    public GameObject minimapIcon = null;
    public GameObject interactPosition;
    
	public bool _inUse = false;
	private float _inUseTimer = 0.0f;
    //[HideInInspector]
	public GameObject user;

    public enum Key
    {
        None,
        Blue,
        Yellow,
        Red
    }

    [Header("Key Settings")]
    public Key KeyType;
    //private bool UnlockedKey = false;

    [Header("HUD")]
    public GameObject MeshSelector;
    public GameObject UseBar;
    public Color UseBaseColor = Color.white;
    private GameObject UseBarParent;
    private UnityEngine.UI.Text UseText;

    [Header("Debug")]
    public Mesh InteractIcon;

    // Use this for initialization
    private void Start () 
    {
        if (MeshSelector)
        {
            MeshSelector.SetActive(false);
            UseText = MeshSelector.GetComponentInChildren<UnityEngine.UI.Text>() as UnityEngine.UI.Text;
            if (UseBar)
            {
                UseBar.GetComponent<UnityEngine.UI.Image>().color = UseBaseColor;
                UseBarParent = UseBar.transform.parent.gameObject;
                UseBarParent.SetActive(false);
            }
        }
    }

    public void Update () {
		if (IsEnabled && AffectedTarget)
		{
			if (_inUse)
			{
                
                if (_inUseTimer < UseDelay)
				{
                    _inUseTimer += Time.deltaTime;
                    if (UseBar)
                        UseBar.GetComponent<UnityEngine.UI.Image>().transform.localScale = new Vector3((1 / UseDelay) * _inUseTimer, 0.6f, 1.0f);
                }
				else if (_inUseTimer >= UseDelay)
                {
					ResetUse();
                }
			}
		}
    }

    private void Use()
	{
        if (IsEnabled && !_inUse)
        {
            if (KeyType == Key.None)
            {
                _inUse = true;
            }
            else
            {
                TextFlash.ShowText("You need a key", Color.blue);
                CancelUse();
            }
        }
        
        if (_inUse && UseBarParent)
            UseBarParent.SetActive(true);
        _inUseTimer = 0.0f;
        if (_inUse)
            user.GetComponent<PrTopDownCharInventory>().StartUsingGeneric("Use");
    }

    private void CancelUse()
    {
        if (UseBarParent)
            UseBarParent.SetActive(false);
        _inUse = false;
        _inUseTimer = 0.0f;
        user.SendMessage("StopUse", SendMessageOptions.DontRequireReceiver);
        user = null;
    }

    private void ResetUse()
	{
        if (UseBarParent)
            UseBarParent.SetActive(false);
        _inUse = false;
		_inUseTimer = 0.0f;
        if (AffectedTarget)
        {
            //print(AffectedTarget+" has been used. Sending "+MessageToSend);
            AffectedTarget.SendMessage(MessageToSend, SendMessageOptions.DontRequireReceiver);
        }
		user.SendMessage("StopUse", SendMessageOptions.DontRequireReceiver);
		user = null;
        if (UseBar)
            UseBar.GetComponent<UnityEngine.UI.Image>().transform.localScale = new Vector3(0f, 1.0f, 1.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && MeshSelector && IsEnabled)
        {
            user = other.gameObject;
            MeshSelector.SetActive(true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && MeshSelector)
        {
            user = null;
            MeshSelector.SetActive(false);
        }
    }
    
    public bool CanInteract()
    {
        // if already used = false;
        return true;
    }

    public Texture2D GetHoverCursor()
    {
        return hoverCursor;
    }

    public void Interact()
    {
        var player = FindObjectOfType<ClickToMove>();
        float distanceToPlayer = (transform.position - player.transform.position).magnitude;

        // If we're in range to use, just do it...
        if (distanceToPlayer <= interactDistance)
        {
            // print("interact here");
            // TextFlash.ShowText("Using", Color.blue);
            Use();
        }
        // ...otherwise, have the player run to object instead
        else
            PathTo(player);
    }
    /// <summary>
    /// Have the player move to the object's location
    /// </summary>
    /// <param name="player">The player's ClickToMove component</param>
    private void PathTo(ClickToMove player)
    {
        //print("Moving closer");
        // Tell the player to move to a point in front of the obelisk
        Vector3 destination = interactPosition.transform.position + interactPosition.transform.forward * 0.5f;

        player.StopManualMovement();
        player.MoveTo(destination, true);
    }
}
