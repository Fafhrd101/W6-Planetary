using UnityEngine;

public class Beam : MonoBehaviour
{
    public LineRenderer BeamLine;

    // To prevent ships from shooting themselves...
    private const float MINRange = 15f;

    public bool playerShot;
    public Ship owner;
    
    public void FireProjectile(Vector3 direction, float range, int dps, Ship attacker)
    {
        if (BeamLine == null)
            BeamLine = GetComponent<LineRenderer>();

        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        if (Physics.Raycast(transform.position+direction.normalized* MINRange, direction, out var hit, range, layerMask)){
            if (hit.transform.CompareTag("Ship"))
            {
                if (!hit.transform.GetComponent<ShipEquipment>().shieldActive)
                    hit.transform.GetComponent<Ship>().TakeDamage(dps * Time.deltaTime, playerShot);
            }
            else if (hit.transform.CompareTag("Asteroid"))
                hit.transform.GetComponent<Asteroid>().TakeDamage(dps * Time.deltaTime);
            else if (hit.transform.CompareTag("StationParts"))
                hit.transform.GetComponentInParent<Station>().TakeDamage(dps * Time.deltaTime, attacker, playerShot);
            
            BeamLine.SetPosition(0, transform.position);
            BeamLine.SetPosition(1, hit.transform.position);
        }
        else
        {
            var position = transform.position;
            BeamLine.SetPosition(0, position);
            BeamLine.SetPosition(1, position+ direction * range);
        }
    }
}
