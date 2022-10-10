using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidExplosion : MonoBehaviour
{

    public GameObject planetExplodeFX;
    public Animation planetExplodeAnim;


    void Start()
    {
        StartCoroutine(nameof(ExplodePlanet));
    }


    IEnumerator ExplodePlanet()
    {
        planetExplodeFX.SetActive(true);
        planetExplodeAnim.Play();
        yield return new WaitForSeconds(4.0f);
        planetExplodeFX.SetActive(false);
    }


}
