using UnityEngine;
using System.Collections;

public class Starfield : MonoBehaviour
{
    private ParticleSystem.Particle[] points;
    private Vector3[] velocities;
    private ParticleSystem ps;

    public int starsMax = 100;
    public float starSize = 1;
    public float starDistance = 10;
    public float starClipDistance = 1;
    private float starDistanceSqr;
    private float starClipDistanceSqr;


    // Use this for initialization
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        starDistanceSqr = starDistance * starDistance;
        starClipDistanceSqr = starClipDistance * starClipDistance;
    }


    private void CreateStars()
    {
        points = new ParticleSystem.Particle[starsMax];
        velocities = new Vector3[starsMax];

        for (int i = 0; i < starsMax; i++)
        {
            points[i].position = Random.insideUnitSphere * starDistance + transform.position;
            points[i].startColor = new Color(1, 1, 1, 1);
            points[i].startSize = starSize;
            velocities[i] = new Vector3(Random.value - .5f, Random.value - .5f, Random.value - .5f) * 0.2f;
        }

        ps.SetParticles(points, points.Length);
    }


    // Update is called once per frame
    void LateUpdate()
    {
        if (points == null) CreateStars();

        for (int i = 0; i < starsMax; i++)
        {

            if ((points[i].position - transform.position).sqrMagnitude > starDistanceSqr)
            {
                points[i].position = Random.insideUnitSphere.normalized * starDistance + transform.position;
            }

            if ((points[i].position - transform.position).sqrMagnitude <= starClipDistanceSqr)
            {
                float percent = (points[i].position - transform.position).sqrMagnitude / starClipDistanceSqr;
                points[i].startColor = new Color(1, 1, 1, percent);
                points[i].startSize = percent * starSize;
            }

            points[i].position += velocities[i];

        }

        ps.SetParticles(points, points.Length);
        
    }
}