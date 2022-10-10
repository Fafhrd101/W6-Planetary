using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ChoosePilotMenuController : MonoBehaviour 
{
    public TMP_Text header;
    public Image menuImage;
    public TMP_Text UPP;
    // Camera animation controls
    public AnimationCurve curve;
    public Transform nextPosition, thisPosition, prevPosition;
    private float _timer;
    private const float CameraShiftTime = 3f;
    private int _currentChoice;

    private void Start()
    {
        _currentChoice = Player.Instance.pilotIconNumber;
        Player.Instance.pilotIcon = IconManager.Instance.players[_currentChoice];
        UPP.text = RandomNameGenerator.GetRandomUPP();
    }

    public void OnItemClicked(int selected)
    {
        if (selected == 0)
            StartCoroutine(MoveToNextMenu());
        if (selected == 1)
            StartCoroutine(MoveToPrevMenu());
    }

    private void Update()
    {
        menuImage.sprite = IconManager.Instance.players[Player.Instance.pilotIconNumber];
    }

    public void ButtonNext()
    {
        _currentChoice++;
        if (_currentChoice > IconManager.Instance.players.Length - 1)
            _currentChoice = 0;
        Player.Instance.pilotIcon = IconManager.Instance.players[_currentChoice];
        Player.Instance.pilotIconNumber = _currentChoice;
        menuImage.sprite = Player.Instance.pilotIcon;
        UPP.text = RandomNameGenerator.GetRandomUPP();
    }

    public void ButtonPrev()
    {
        _currentChoice--;
        if (_currentChoice < 0)
            _currentChoice = IconManager.Instance.players.Length - 1;
        Player.Instance.pilotIcon = IconManager.Instance.players[_currentChoice];
        Player.Instance.pilotIconNumber = _currentChoice;
        menuImage.sprite = Player.Instance.pilotIcon;
        UPP.text = RandomNameGenerator.GetRandomUPP();
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
