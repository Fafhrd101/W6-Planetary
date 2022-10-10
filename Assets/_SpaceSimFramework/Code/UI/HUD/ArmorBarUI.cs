using UnityEngine;
using UnityEngine.UI;

public class ArmorBarUI : MonoBehaviour {

    private Text armorText; 

    void Start()
    {
        armorText = GetComponentInChildren<Text>();
    }

    void Update () {
        float value = Ship.PlayerShip.armor / (float)Ship.PlayerShip.maxArmor * 100f;
        armorText.text = "HULL \n" + (int)Ship.PlayerShip.armor + "/" + (int)Ship.PlayerShip.maxArmor;

    }

}
