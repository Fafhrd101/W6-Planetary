using UnityEngine;

public class AsteroidItem : CargoItem
{
    public GameObject ParticleAttractorEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Ship>() == Ship.PlayerShip)
        {
            if (other.gameObject.GetComponent<ShipCargo>() != null)
                other.gameObject.GetComponent<ShipCargo>().AddCargoItem(this);
            var transform1 = transform;
            var attractor = GameObject.Instantiate(ParticleAttractorEffect, transform1.position, transform1.rotation);
            attractor.GetComponent<ParticleAttractorLinear>().Target = other.gameObject.transform;
            GameObject.Destroy(this.gameObject);
            TextFlash.ShowYellowText(item.amount + " " + item.itemName + " recovered!");
            Progression.AddExperience(100);
            Player.Instance.salvageCollected++;
        }
    }
}
