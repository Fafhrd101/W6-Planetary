using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet_Explosion_Slow : MonoBehaviour
{

    public GameObject planetSeparateFX;
    public GameObject planetExplodeFX;
    public GameObject planetExplodeAnim;
    public GameObject glowSphere;
    public GameObject planetAtmos;


    // Use this for initialization
    void Start()
    {

        planetExplodeFX.SetActive(false);
        planetSeparateFX.SetActive(false);
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

        planetSeparateFX.SetActive(true);
        planetExplodeAnim.GetComponent<Animation>().Play();

        yield return new WaitForSeconds(4.5f);

        planetAtmos.SetActive(false);
        glowSphere.SetActive(false);
        planetExplodeFX.SetActive(true);

        yield return new WaitForSeconds(6.0f);

        planetExplodeFX.SetActive(false);
        planetSeparateFX.SetActive(false);

    }


}
