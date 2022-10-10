using System;
using FORGE3D;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetInfocard : MonoBehaviour
{

    public GameObject infoCardPanel;
    public TMP_Text modelName;
    public Image shipIcon;
    public RawImage shipCameraView;
    public TMP_Text speedValue;
    public TMP_Text maneuverability;
    public TMP_Text dockingType;
    public TMP_Text cargo;
    public TMP_Text hullArmorValue;
    public Slider armorBar;
    public TMP_Text damageValue;
    public TMP_Text equipmentMounts;
    public TMP_Text gunMounts;
    public TMP_Text turretMounts;
    public TMP_Text totalPoints;
    public TMP_Text orders;
    
    private string _orders;
    private float _hullPercentage;

    private Ship _targetShip;

    public void InitializeInfocard(Ship targetShip)
    {
        if(targetShip == null)
        {
            print("Missing targetship!");
            infoCardPanel.SetActive(false);
            return;
        }
        _targetShip = targetShip;
        
        if (shipIcon != null)
            shipIcon.sprite = IconManager.Instance.GetShipIcon(targetShip.shipModelInfo.modelName);
        
        modelName.text = targetShip.name;
        
       // ModelData.text = "Class: " + targetShip.ShipModelInfo.Class + "\nArmor: " + targetShip.ShipModelInfo.MaxArmor +
        //    "\nGenerator: " + targetShip.ShipModelInfo.GeneratorPower + "\nRegen: " + targetShip.ShipModelInfo.GeneratorRegen;
        //UnitName.text = targetShip.ShipModelInfo.name;//targetShip.name;
        
        // int maxSpeed = targetShip.ShipModelInfo.maxSpeed * 3;
        // speedValue.text = maxSpeed.ToString();
        //
        // float af = targetShip.ShipModelInfo.AngularForce.x + targetShip.ShipModelInfo.AngularForce.y +
        //            targetShip.ShipModelInfo.AngularForce.z;
        // float maneuver = (targetShip.ShipModelInfo.totalMass / af * 10);
        // maneuverability.text = maneuver.ToString("F1");
        
        int armor = targetShip.shipModelInfo.MaxArmor;
        hullArmorValue.text = armor.ToString();
        
        // int cargoSlots = targetShip.ShipModelInfo.CargoSize;
        // int equip = targetShip.ShipModelInfo.EquipmentSlots;
        // cargo.text = cargoSlots.ToString()+"/"+equip.ToString();
        // equipmentMounts.text = equip.ToString();
        
        // int count = 0;
        // int count2 = 0;
        // int count3 = 0;
        // float dam = 0;
        // foreach (var mg in targetShip.Equipment.mountedGuns)
        // {
        //     count++;
        //     dam += mg.GetComponent<GunHardpoint>().mountedWeapon.GetProjectileDamage();
        // }
        // foreach (var mg in targetShip.Equipment.mountedFXguns)
        // {
        //     count++;
        //     dam += mg.GetComponent<GunHardpoint>().mountedWeapon.GetProjectileDamage();
        // }
        // foreach (var mt in targetShip.Equipment.mountedTurrets)
        // {
        //     count2++;
        //     dam += mt.GetComponent<TurretHardpoint>().mountedWeapon.GetProjectileDamage();
        // }
        // foreach (var mt in targetShip.Equipment.mountedMissiles)
        // {
        //     count3++;
        // }
        
        //gunMounts.text = count.ToString()+"/"+count2.ToString()+"/"+count3.ToString();
        //turretMounts.text = count2.ToString();
        
        // damageValue.text = dam.ToString("F1");
        //
        // dockingType.text = targetShip.ShipModelInfo.mooring.ToString();
        
        // int pointValue;
        // pointValue = equip + (cargoSlots / 10) + (int) (armor / 250) + equip + (4/*(count+count2)*/ * 5);
        // totalPoints.text = pointValue.ToString();

        infoCardPanel.SetActive(true);
    }

    private void Update()
    {
        if (_targetShip == null)
            return;

        _hullPercentage = _targetShip.armor / _targetShip.maxArmor;
        if (armorBar != null)
            armorBar.value = _hullPercentage;
        orders.text = _targetShip.AIInput.CurrentOrder != null ? _targetShip.AIInput.CurrentOrder.Name : "";
    }
}

