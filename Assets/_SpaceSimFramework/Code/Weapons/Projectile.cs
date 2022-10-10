using UnityEngine;

public class Projectile : MonoBehaviour {

    public TrailRenderer trail;
    public ParticleSystem hitEffect;
    private float _range;
    private Vector3 _initialPos;
    [HideInInspector]
    public int damage;

    // To prevent ships from shooting themselves...
    private float minRange = 15f;
    private SphereCollider _projCollider;

    public bool playerShot;
    public Ship owner;
    private void Awake()
    {
        _projCollider = GetComponent<SphereCollider>();
        _projCollider.enabled = false;
    }

    void Update () {
        float distanceTravelled = Vector3.Distance(_initialPos, transform.position);

        if(distanceTravelled > minRange)
        {
            _projCollider.enabled = true;
        }
		if(distanceTravelled > _range)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.SetActive(false);
        }
	}

    public void FireProjectile(Vector3 direction, float force, float range, int dmg, Ship attacker)
    {
        // Players do double damage, make up for being lousy shots?
        if (attacker == Ship.PlayerShip)
            dmg *= 2;
        GetComponent<Rigidbody>().AddForce(direction * force, ForceMode.Impulse);        
        // To prevent ships from shooting themselves...
        this._range = range;
        this._initialPos = transform.position;
        this.damage = dmg;
        this.owner = attacker;

   }

    private void OnCollisionEnter(Collision collision)
    {
        if (owner == Ship.PlayerShip)
            playerShot = true;
        
        if (!_projCollider.enabled)
            return;

        ParticlePool.Instance.CreateParticleEffectAtPos(collision.contacts[0].point);
        // if (hitEffect != null)
        // {
        //     // Test reasons
        //     hitEffect.gameObject.name = "bullet hit and exploded";
        //     hitEffect.Play();
        // }
        
        if(collision.gameObject.CompareTag("ForceField"))
        {
            //print("Shield hit! "+collision.gameObject.GetComponent<ShipEquipment>().shieldActive);
            if (collision.gameObject.GetComponent<ShipEquipment>() && !collision.gameObject.GetComponent<ShipEquipment>().shieldActive)
                collision.gameObject.GetComponent<Ship>().TakeDamage(damage, playerShot);
        }
        else if(collision.gameObject.CompareTag("Ship"))
        {
            // if (owner == Ship.PlayerShip)
            //     print("We hit a ship with "+damage+":"+playerShot);
            
            if (!collision.gameObject.GetComponent<ShipEquipment>().shieldActive)
                collision.gameObject.GetComponent<Ship>().TakeDamage(damage, playerShot);
        }
        else if (collision.gameObject.CompareTag("Asteroid"))
        {
            collision.gameObject.GetComponent<Asteroid>().TakeDamage(damage);
        }
        else if (collision.gameObject.CompareTag("Station")||collision.gameObject.CompareTag("StationParts"))
        {
            if (collision.gameObject.GetComponentInParent<Station>() != null)
                collision.gameObject.GetComponentInParent<Station>().TakeDamage(damage, owner, playerShot);
        }
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if(trail)
            trail.Clear();
    }
}
