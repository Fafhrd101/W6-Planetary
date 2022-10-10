using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticlePool : Singleton<ParticlePool>
{
    public GameObject[] RandomExplosionPrefabs;
    public int particlePoolSize = 20;
    public List<GameObject> particlePool;
    public void Awake()
    {
        particlePool = new List<GameObject>(particlePoolSize*RandomExplosionPrefabs.Length);
        for (int i = 0; i < RandomExplosionPrefabs.Length; i++)
        {
            for (int x = 0; x < particlePoolSize; x++)
            {
                GameObject proj = GameObject.Instantiate(RandomExplosionPrefabs[i]);
                particlePool.Add(proj);
                proj.transform.SetParent(this.transform);
                proj.SetActive(false);
            }
        }
        particlePool = particlePool.OrderBy( x => Random.value ).ToList( );
    }

    public void CreateParticleEffectAtPos(Vector3 position)
    {
        //var numb = Random.Range(0, RandomExplosionPrefabs.Length);
        //GameObject.Instantiate(RandomExplosionPrefabs[numb], position, Quaternion.identity);

        foreach (var t in particlePool)
        {
            if (t != null && !t.activeInHierarchy)
            {
                t.transform.position = position;
                t.SetActive(true);
                return;
            }
        }
    }
}
