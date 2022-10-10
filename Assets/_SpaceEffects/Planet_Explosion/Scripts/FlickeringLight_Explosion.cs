using UnityEngine;
using System.Collections;

public class FlickeringLight_Explosion : MonoBehaviour {

public Light light;
private int lightIntensity = 32;

void Start (){

}

void Update (){

    lightIntensity = (Random.Range (27, 32));
    light.intensity = lightIntensity;

}
}