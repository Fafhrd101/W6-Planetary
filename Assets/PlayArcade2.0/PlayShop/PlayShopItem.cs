using UnityEngine;
using System;


	[Serializable]
	public class PlayShopItem
	{
		public Transform itemButton;
		public int lockState = 0;
		public string itemID;
		public string itemName;
		public string itemDesc;
		public string itemImage;
		public int itemCost = 100;
	}
