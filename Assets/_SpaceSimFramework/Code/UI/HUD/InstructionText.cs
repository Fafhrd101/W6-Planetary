using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InstructionText : MonoBehaviour {

    public AnimationCurve ScaleCurve;
    public float AnimationTime = 5;

    private TMP_Text flashingText;
    private float elapsedTime = 0;
    private bool started = false;
    
	void Awake () {
        flashingText = GetComponent<TMP_Text>();
        flashingText.enabled = false;
    }

    public void SetText(string text)
    {
        flashingText.enabled = true;
        flashingText.text = text;
        started = true;
    }
	
	void Update () {
        if (!started)
            return;

        elapsedTime += Time.deltaTime;
        flashingText.rectTransform.localScale = Vector3.one * ScaleCurve.Evaluate(elapsedTime / AnimationTime);
        if (elapsedTime > AnimationTime) {
            InstructionText.OnMessageEnd();

            GameObject.Destroy(this.gameObject);
        }
    }

    private static List<GameObject> ActiveInstances;
    private static List<string> ActiveMessages;

    public static void ShowInstructionText(string message)
    {
        if (Ship.PlayerShip.isDestroyed)
            return;
        
        if (ActiveInstances == null) { 
            ActiveInstances = new List<GameObject>();
            ActiveMessages = new List<string>();
        }

        var newInstance = GameObject.Instantiate(
            UIElements.Instance.InstructionText,
            CanvasController.Instance.gameObject.transform);
        // Enqueue message
        ActiveInstances.Add(newInstance);
        ActiveMessages.Add(message);

        if(ActiveInstances.Count == 1)  // Play message if none are active right now
            newInstance.GetComponent<InstructionText>().SetText(message);
    }

    private static void OnMessageEnd()
    {
        // Remove currently ended message 
        ActiveInstances.RemoveAt(0);
        ActiveMessages.RemoveAt(0);

        // If there is a message in the wait queue, show it now
        if(ActiveInstances.Count > 0)
            ActiveInstances[0].GetComponent<TextFlash>().SetText(ActiveMessages[0]);
    }
}
