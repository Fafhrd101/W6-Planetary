using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This simply brings the random sky stuff created in MainMenu into our first sector (0,0).
// That sector will not be saved.

public class SectorStartScenario : Singleton<SectorStartScenario>
{
    public Flare flare;
    public Material skybox;
    public Color skyboxTint;

    void Awake ()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    public void SetScenario()
    {
        GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare = flare;
        RenderSettings.skybox = skybox;
        if (RenderSettings.skybox.HasProperty("_Tint"))
            RenderSettings.skybox.SetColor("_Tint", skyboxTint);
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            RenderSettings.skybox.SetColor("_SkyTint", skyboxTint);
        
        DynamicGI.UpdateEnvironment(); 
        Destroy(this.gameObject); // we're done with it. New games will supply another.
    }
}
