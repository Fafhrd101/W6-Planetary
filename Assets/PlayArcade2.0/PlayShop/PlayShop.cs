using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
	/// This script handles a shop, in which there are items that can be bought and unlocked with coins.
	/// </summary>
	public class PlayShop : MonoBehaviour
	{
		//How many coins we have left in the shop
		public int coinsLeft = 0;
		
		//The text that displays the coins we have
		public Transform coinsText;

		// An array of shop items that can be bought and unlocked
		public PlayShopItem[] shopItems;

		//The number of the currently selected item
		public int currentItem = 0;
		
		//The color of the item when we have at least one of it
		public Color unselectedColor = new Color(0.6f,0.6f,0.6f,1);
		
		//The color of the item when it is selected
		public Color selectedColor = new Color(1,1,1,1);
		
		//The color of the item when we can't afford it
		public Color errorColor = new Color(1,0,0,1);
		public GameObject buttonPrefab;
		public Transform buttonHolder;
		public Transform descBox;
		public Text descText;

		public GameObject confirmationPopup;
		public Text confirmationText;
		public Button confirmButton;
		public Button cancelButton;
		
		public Image coinImage;

		void OnEnable()
		{
			// Remove any existing buttons
			foreach (Transform child in buttonHolder.transform) {
				GameObject.Destroy(child.gameObject);
			}
			coinImage.sprite = PlayArcadeIntegration.Instance.coinSprite;
			
			descBox.gameObject.SetActive(false);
			confirmationPopup.SetActive(false);
			// Remove any basic store items, such as credit to play.
			var itemToRemove = PlayArcadeIntegration.Instance.storeItems.SingleOrDefault(r => r.friendly_name == "Game Start");
			if (itemToRemove != null)
				PlayArcadeIntegration.Instance.storeItems.Remove(itemToRemove);
			List<PlayArcadeStoreItem> SortedList = PlayArcadeIntegration.Instance.storeItems.OrderBy(o=>o.item_cost).ToList();
			//print("We have "+SortedList.Count+" sorted items");
			
			// Should be able to resize here, so we don't need to preset array
			//shopItems = new PlayShopItem[SortedList.Count];
			
			for (int index = 0; index < SortedList.Count; index++)
			{
				shopItems[index].itemCost = SortedList[index].item_cost;
				shopItems[index].itemID = SortedList[index].item_id;
				shopItems[index].itemName = SortedList[index].friendly_name;
				shopItems[index].itemImage = SortedList[index].item_image;
				shopItems[index].itemDesc = SortedList[index].description;
				var button = Instantiate(buttonPrefab, buttonHolder);
				PlayShopButton psb = button.GetComponent<PlayShopButton>();
				psb.itemCost.text = shopItems[index].itemCost.ToString();
				psb.itemName.text = shopItems[index].itemName;
				psb.itemID = shopItems[index].itemID;
				psb.itemNumber = index;
				psb.itemDesc = shopItems[index].itemDesc;
				//print("requesting icon "+ SortedList[index].item_image);
				//StartCoroutine(PlayArcadeIntegration.Instance.GetDirectTexture(shopItems[index].itemImage, psb.icon, null));
				psb.shop = this;
				shopItems[index].itemButton = psb.transform;
			}
		}
		void Start()
		{
			//Get the number of coins we have
			coinsLeft = PlayArcadeIntegration.Instance.currentBalance;
			
			//Update the text of the coins we have
			coinsText.GetComponent<Text>().text = coinsLeft.ToString();

			//Update all the items
			UpdateItems();
		}
		
		// Update so we can sell an item multiple times, or prevent it from being selected again, etc
		void UpdateItems()
		{
			for ( int index = 0 ; index < shopItems.Length ; index++ )
			{
				//Get the lock state of this item from a not-yet-saved Arcade call
				shopItems[index].lockState = 0;
				
				//Deselect the item
				// if (shopItems[index].itemButton)
				// shopItems[index].itemButton.GetComponent<Image>().color = unselectedColor;
				
				//If we already unlocked this item, don't display its price
				if ( shopItems[index].lockState > 0 )
				{
					//Deactivate the price and coin icon
					shopItems[index].itemButton.Find("TextPrice").gameObject.SetActive(false);
					
					// //Highlight the currently selected item
					// if ( index == currentItem )    shopItems[index].itemButton.GetComponent<Image>().color = selectedColor;
				}
				else
				{
					//Update the text of the cost
					if (shopItems[index].itemButton)
					shopItems[index].itemButton.Find("TextPrice").GetComponent<Text>().text = shopItems[index].itemCost.ToString();
				}
			}
		}
		
		public void BuyItem( int itemNumber )
		{
			if (confirmationPopup.activeSelf)
				return;
			if ( shopItems[itemNumber].itemCost <= coinsLeft ) //If we have enough coins, buy this item
			{
				//Select the item
				SelectItem(itemNumber);
				print("Asking to buy: "+shopItems[itemNumber].itemName );
				ConfirmBuyItem(itemNumber);
			}
			
			//Update all the items
			UpdateItems();
		}

		public void ConfirmBuyItem(int itemNumber)
		{
			confirmationPopup.SetActive(true);
			confirmButton.interactable = true;
			confirmationText.text = "Buy " +
			                   shopItems[itemNumber].itemName + " for " +
			                   shopItems[itemNumber].itemCost + "?";
		}

		public void Confirmed()
		{
			PlayArcadeIntegration.Instance.BuyAThing(shopItems[currentItem].itemID);
			// lock the button until we've received confirmation
			confirmButton.interactable = false;
		}

		public void Canceled()
		{
			confirmationPopup.SetActive(false);
		}
		
		public IEnumerator CloseConfirmation()
		{
			yield return new WaitForSecondsRealtime(1);
			//Increase the item count
			shopItems[currentItem].lockState = 1;
			//Deduct the price from the coins we have
			coinsLeft -= shopItems[currentItem].itemCost;
			//Update the text of the coins we have
			coinsText.GetComponent<Text>().text = coinsLeft.ToString();
			confirmationPopup.SetActive(false);
			shopItems[currentItem].itemButton.GetComponent<Button>().interactable = true;
		}
		
		void SelectItem( int itemNumber )
		{
			currentItem = itemNumber;
		}
	}



