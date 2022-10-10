using UnityEngine;
using UnityEngine.UI;

public class PlayShopButton : MonoBehaviour
{
    public Text itemName;
    public Text itemCost;
    public Image icon;
    public string itemID;
    public string itemDesc;
    public int itemNumber; // button number, matches shop list
    public string application_credit_item_id;
    public PlayShop shop;
    
    public void buttonAction()
    {
        shop.BuyItem(this.itemNumber);
    }

    public void OnHover()
    {
        shop.descBox.gameObject.SetActive(true);
        shop.descText.text = shop.shopItems[itemNumber].itemDesc;
    }
    
    public void OnUnHover()
    {
        shop.descBox.gameObject.SetActive(false);
    }
}
