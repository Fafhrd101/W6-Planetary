using UnityEngine;
using System.IO;
using System.Collections;
using Unity.Mathematics;

public class MainMenuController : MonoBehaviour
{
    public GDTFadeEffect screenFader;

    // Camera animation parameters
    public AnimationCurve curve;
    public Transform thisPosition, prevPosition;
    private float _timer;
    private const float CameraShiftTime = 3f;

    private void Awake()
    {
        GenerateRandomSector.GenerateMainMenuSectorAtPosition(-1 * Vector2.one, Vector2.zero);
    }

    public void OnStartNewClicked()
    {
        var loadType = PlayArcadeIntegration.Instance.loadAutosave ? "SavedGame" : "NewGame";
        print("Loading "+loadType);

        if (PlayArcadeIntegration.Instance.loadAutosave && (int)Player.Instance.currentSector.x != -1 && (int)Player.Instance.currentSector.y != -1)
            PlayArcadeIntegration.Instance.startScene = "EmptyFlight";
        else
            PlayArcadeIntegration.Instance.startScene = "StartScenario";
        screenFader.gameObject.SetActive(true);
        Invoke(nameof(StartGame), 1.5f);
    }

    void StartGame()
    {
        screenFader.gameObject.SetActive(true);
        PlayArcadeIntegration.Instance.ButtonStartGameRequest();
    }
    
    public void OnItemClicked(int selected)
    {
        if (selected == 0)
            OnStartNewClicked();
        if (selected == 1)
            StartCoroutine(MoveToPrevMenu());
    } 
    private IEnumerator MoveToPrevMenu()
    {
        _timer = 0;
        while (_timer < CameraShiftTime) {
            if (Camera.main is not null)
                Camera.main.transform.position = Vector3.Lerp(thisPosition.position, prevPosition.position,
                    curve.Evaluate(_timer / CameraShiftTime));
            _timer += Time.deltaTime;
            yield return null;
        }
    }
}
