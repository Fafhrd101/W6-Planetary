using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class WarningUI : Singleton<WarningUI>
{
    
    public GameObject corrosion, sensorObscuring;
    public TMP_Text corrosionDps;
    public bool corrosionOn = false;
    public bool sensorsObscured = false;
    private float _t;
    private float _targetAlpha;
    public float blinkSpeed = 1f;
    
    private void Update()
    {
        switch (sensorsObscured)
        {
            case true when !sensorObscuring.activeSelf:
                sensorObscuring.SetActive(true);
                break;
            case false when sensorObscuring.activeSelf:
                sensorObscuring.SetActive(false);
                break;
        }
        switch (corrosionOn)
        {
            case true when !corrosion.activeSelf:
                corrosion.SetActive(true);
                break;
            case false when corrosion.activeSelf:
                corrosion.SetActive(false);
                break;
        }
        _t += Time.deltaTime * blinkSpeed;
        _targetAlpha = Mathf.PingPong(_t, 1f);
        
        var i = corrosion.GetComponent<TMP_Text>();
        i.color = new Color(i.color.r, i.color.g, i.color.b, _targetAlpha);
        i = corrosionDps.GetComponent<TMP_Text>();
        i.color = new Color(i.color.r, i.color.g, i.color.b, _targetAlpha);
        i = sensorObscuring.GetComponent<TMP_Text>();
        i.color = new Color(i.color.r, i.color.g, i.color.b, _targetAlpha);
    }
}
