using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotsUI : MonoBehaviour
{
    public Sprite unusableSlot;
    public Sprite usableSlot;
    private GameObject[] slot;
    public ShipEquipment equipment;
    public Ship ship;
    void Start()
    {
        equipment = Ship.PlayerShip.gameObject.GetComponent<ShipEquipment>();
    }

    void OnEnable()
    {
        //int x = 0;
        if (Ship.PlayerShip == null)
        {
            //print("no ship, trying again "+x++);
            Invoke("OnEnable",1f);
        }
        else
        {
            ship = Ship.PlayerShip;
            //print(Ship.PlayerShip.gameObject.name);
            equipment = Ship.PlayerShip.gameObject.GetComponent<ShipEquipment>();
            // if (equipment == null)
            //     print("No ship");
            // else
            //     print("pondering players premounted weapons");

            // if (equipment.Turrets.Count > 0)
            //     print("we got turrets");
            // if (equipment.mountedGuns.Count > 0)
            //     print("we got guns");
            slot = new GameObject[transform.childCount];
            foreach (Transform child in transform)
            {
                Image image = child.GetComponent<Image>();
                image.sprite = unusableSlot;
            }

            // first add the guns
            
            // next equipment capabilities
            var usable = Ship.PlayerShip.shipModelInfo.EquipmentSlots;
            for (int i = 0; i < usable; i++)
                transform.GetChild(i).GetComponent<Image>().sprite = usableSlot;
            //print(usable + " slots available on this ship");
        }
    }
}
