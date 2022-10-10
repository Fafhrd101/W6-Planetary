using UnityEngine;

public class Asteroid : MonoBehaviour {
    [HideInInspector]
    public int Yield = 0;
    public float Health = 250;
    [HideInInspector]
    public string Resource = "";
    public GameObject CargoItemPrefab;
    public GameObject explosionPrefab;
    
    public void TakeDamage(float damage)
    {
        Health -= damage;
        if(Health <= 0)
        {
            //ParticleController.Instance.CreateShipExplosionAtPos(transform.position);
            GameObject obj = GameObject.Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            obj.transform.localScale = transform.localScale/2;
            
            // if (Resource == "")
            // {
            //     GameObject.Destroy(this.gameObject);
            //     return;
            // }

            // Drop random cargo items from the ones available
            Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));

            // Eject item to a random location
            GameObject cargo = GameObject.Instantiate(
                CargoItemPrefab,
                transform.position + randomAddition,
                Quaternion.identity);

            cargo.GetComponent<CargoItem>().InitCargoItem(HoldItem.CargoType.Ware, Yield, "Ore"/*Resource*/);            
            GameObject.Destroy(this.gameObject);
        }
    }

    public void Update()
    {
        if (Ship.PlayerShip != null)
        {
            var distanceToPlayerShip = Vector3.Distance(Ship.PlayerShip.transform.position, this.transform.position);
            if (distanceToPlayerShip < 50)
            {
                Ship.PlayerShip.isSpeedLimited = true;
                Ship.PlayerShip.inSupercruise = false;
            } else Ship.PlayerShip.isSpeedLimited = false;
        }
    }
    public void ApplyMaterial(Material mat)
    {
        foreach (MeshRenderer lod in GetComponentsInChildren<MeshRenderer>())
        {
            lod.material = mat;
        }
    }
}
