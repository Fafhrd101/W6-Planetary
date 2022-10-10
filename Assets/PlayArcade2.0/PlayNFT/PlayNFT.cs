using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayNFT : Singleton<PlayNFT>
{
    public int score;
    public int health;
    public int luck;
    public int speed;
    public int xp;
    public int power;

    public string collectionName;
    public string imageURL;
    
    public void ApplyValues()
    {
        Ship.PlayerShip.maxArmor *= 1+(PlayNFT.Instance.health * .1f);
        Ship.PlayerShip.armor = Ship.PlayerShip.maxArmor;
        Ship.PlayerShip.Equipment.energyCapacity *= 1+(PlayNFT.Instance.power * .1f);
        Ship.PlayerShip.Equipment.energyAvailable = Ship.PlayerShip.Equipment.energyCapacity;
        var adjust = 1 + (PlayNFT.Instance.speed * .1f);
        Ship.PlayerShip.Physics.forceMultiplier *= 1+(PlayNFT.Instance.speed * .1f);
    }
}
