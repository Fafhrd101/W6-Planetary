using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet_Explosion_Fast_Laser : MonoBehaviour
{

    public GameObject planetExplodeFX;
    public GameObject planetExplodeAnim;
    public GameObject planetAtmos;
    public GameObject laserFX;
    public GameObject cameraShake;

    // Use this for initialization
    void Start()
    {

        planetExplodeFX.SetActive(false);
        laserFX.SetActive(false);
        planetAtmos.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Fire1"))
        {

            StartCoroutine("ExplodePlanet");

        }

    }


    IEnumerator ExplodePlanet()
    {

        laserFX.SetActive(true);

        yield return new WaitForSeconds(0.9f);

        planetExplodeFX.SetActive(true);
        cameraShake.GetComponent<Animation>().Play();
        planetExplodeAnim.GetComponent<Animation>().Play();
        planetAtmos.SetActive(false);

        yield return new WaitForSeconds(6.0f);

        planetExplodeFX.SetActive(false);
        laserFX.SetActive(false);

    }


}
