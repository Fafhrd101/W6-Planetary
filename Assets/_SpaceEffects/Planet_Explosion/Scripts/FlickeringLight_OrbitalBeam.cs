using UnityEngine;
using System.Collections;

public class FlickeringLight_OrbitalBeam : MonoBehaviour {

public Light fuseLight;
private int fuseLightIntensity = 10;

void Start (){

}

void Update (){

    fuseLightIntensity = (Random.Range (4   , 8));
    fuseLight.intensity = fuseLightIntensity;

}
}