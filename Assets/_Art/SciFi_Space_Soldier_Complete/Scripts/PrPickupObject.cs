using UnityEngine;

public class PrPickupObject : MonoBehaviour {
    
    [HideInInspector]
    public GameObject player;
    public Renderer MeshSelector;

    [HideInInspector]
    public bool showSelectorAlways = false;

    [Header("HUD")]
    public bool showText = false;
    private UnityEngine.UI.Text _useText;

    [HideInInspector]
    public string itemName = "item";
    [HideInInspector]
    public string[] weaponNames;

    public void Start () {
        gameObject.tag ="Pickup";
        if (MeshSelector)
            MeshSelector.enabled = showSelectorAlways;
        _useText = GetComponentInChildren<UnityEngine.UI.Text>() as UnityEngine.UI.Text;
    }

    protected virtual void SetName()
    {
        //set Name
    }

    public virtual void Initialize()
    {
        SetName();

        if (MeshSelector)
            MeshSelector.enabled = showSelectorAlways;
    }

    // BasicInteractable uses this entrance
    public void PickupObject()
    {
        player.transform.LookAt(gameObject.transform);
        PickupObjectNow(1);
        // SendMessageUpwards("TargetPickedUp", SendMessageOptions.DontRequireReceiver);
        // SendMessageUpwards("CollectablePickup", SendMessageOptions.DontRequireReceiver);
        // Destroy(gameObject);
    }
    
    protected virtual void PickupObjectNow(int activeWeapon)
    {
        player.SendMessage("TargetPickedUp", SendMessageOptions.RequireReceiver);
        Destroy(gameObject, 1);
    }

    public void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (MeshSelector) MeshSelector.enabled = true;
        player = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (MeshSelector) MeshSelector.enabled = false;
        player = null;
    }
}
