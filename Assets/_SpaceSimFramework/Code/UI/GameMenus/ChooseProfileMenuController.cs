using UnityEngine;
using System.Collections;
using System.IO;

public class ChooseProfileMenuController : MonoBehaviour {
    
    public TargetInfocard infoCard;

    // Camera animation controls
    public AnimationCurve curve;
    public Transform nextPosition, thisPosition/*, prevPosition*/;
    private float _timer;
    private const float CameraShiftTime = 3f;

    void Start()
    {
        infoCard.InitializeInfocard(Ship.PlayerShip); //Player.Instance.Ships[0].GetComponent<Ship>()); 
        // Material is always screwed. Fix it immediately.
        PlayArcadeIntegration.Instance.coinMaterial.mainTexture = PlayArcadeIntegration.Instance.coinTexture;
    }

    public void OnItemClicked(int selected)
    {
        if (selected == 0)
        {
            File.Delete(Application.persistentDataPath + "Knowledge");
            File.Delete(Application.persistentDataPath + "AutoSave");
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            Player.Instance.pilotIconNumber = 0;
            Player.Instance.pilotIcon = null;
            PlayArcadeIntegration.Instance.loadAutosave = false;
        }
        else if (selected == 1)
        {
            LoadGame.LoadAutosave();
            PlayArcadeIntegration.Instance.loadAutosave = true;
        }
        
        StartCoroutine(MoveToNextMenu());
    }

    private IEnumerator MoveToNextMenu()
    {
        _timer = 0;
        while (_timer < CameraShiftTime) {
            if (Camera.main is not null)
                Camera.main.transform.position = Vector3.Lerp(thisPosition.position, nextPosition.position,
                    curve.Evaluate(_timer / CameraShiftTime));
            _timer += Time.deltaTime;
            yield return null;
        }
    }

}
