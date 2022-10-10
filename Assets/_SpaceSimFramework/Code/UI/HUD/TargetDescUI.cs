using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class TargetDescUI : MonoBehaviour {

    private TMP_Text text;
    private GameObject target;
    private GameObject prevTarget;

    void Awake () {
        text = GetComponent<TMP_Text>();
	}
	
	void Update ()
	{
		if (Ship.PlayerShip == null)
			return;
        target = InputHandler.Instance.GetCurrentSelectedTarget();

        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        if (target == null)
        {
	        text.text = "";//"Target:\nNone";
        }
        else
            text.text = "Target: " + target.name + "\nDistance: " + (int)Vector3.Distance(Ship.PlayerShip.transform.position, target.transform.position);
	}
}
