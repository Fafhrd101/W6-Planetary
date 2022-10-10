using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "DataHolders/ParticleController")]
public class ParticleController : SingletonScriptableObject<ParticleController>
{
    public GameObject StoneHitEffectPrefab;
    public GameObject MetalHitEffectPrefab;
    public GameObject StationExplosionPrefab;
    public GameObject ShipExplosionPrefab;
    public GameObject JumpFlashPrefab;
    public GameObject ShipDamageEffect;
    public GameObject AsteroidExplosionPrefab;

    public void CreateParticleEffectAtPos(Vector3 position)
    {
        GameObject.Instantiate(StoneHitEffectPrefab, position, Quaternion.identity);
    }
    public void CreateStoneHitEffectAtPos(Vector3 position)
    {
        GameObject.Instantiate(StoneHitEffectPrefab, position, Quaternion.identity);
    }
    public void CreateMetalHitEffectAtPos(Vector3 position)
    {
        GameObject.Instantiate(MetalHitEffectPrefab, position, Quaternion.identity);
    }
    public void CreateShipExplosionAtPos(Vector3 position)
    {
        GameObject.Instantiate(ShipExplosionPrefab, position, Quaternion.identity);
    }
    public void CreateStationExplosionAtPos(Vector3 position)
    {
        GameObject.Instantiate(StationExplosionPrefab, position, Quaternion.identity);
    }
    public void CreateAsteroidExplosionAtPos(Vector3 position)
    {
        GameObject.Instantiate(AsteroidExplosionPrefab, position, Quaternion.identity);
    }
    public void CreateJumpFlashAtPos(Vector3 position)
    {
        GameObject.Instantiate(JumpFlashPrefab, position, Quaternion.identity);
    }
}
