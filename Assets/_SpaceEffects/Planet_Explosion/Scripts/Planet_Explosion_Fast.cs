using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet_Explosion_Fast : MonoBehaviour
{

    public GameObject planetExplodeFX;
    public GameObject planetExplodeAnim;
    public GameObject planetAtmos;


    // Use this for initialization
    void Start()
    {

        planetExplodeFX.SetActive(false);
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

        planetExplodeFX.SetActive(true);
        planetExplodeAnim.GetComponent<Animation>().Play();
        planetAtmos.SetActive(false);

        yield return new WaitForSeconds(6.0f);

        planetExplodeFX.SetActive(false);

    }


}
