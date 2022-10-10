using UnityEngine;

public class ParticleAutoDestroy : MonoBehaviour
{
    private ParticleSystem psys;

    public void Awake()
    {
        psys = GetComponent<ParticleSystem>();
    }

    public void Update()
    {
        if (!psys) return;
        if (!psys.IsAlive()) Destroy(gameObject);
    }
}