using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows throttle and speed of the player ship.
/// </summary>
public class ShipUI : MonoBehaviour
{
    public TMP_Text textThrottle, textVelocity, textHull;
    public Image VelocityBar;
    public Image SpeedBar;
    public Image HullBar;
    private float value;
    
    void Update()
    {
        if(Ship.PlayerShip != null)
        {
            value = Ship.PlayerShip.Throttle * 100.0f;
            textThrottle.text = "Throttle " + Mathf.Round(value) + "%";
            value = Ship.PlayerShip.Velocity.magnitude;
            textVelocity.text = "Velocity " + value.ToString("f2") + " UPS";
            textHull.text = "Hull Armor " + (int) Ship.PlayerShip.armor;

            if (HullBar != null)
            {
                float value = Ship.PlayerShip.armor / (float) Ship.PlayerShip.maxArmor * 100f;
                HullBar.fillAmount = value;
            }
        }
        //else print("NULL ship!");
    }
}
