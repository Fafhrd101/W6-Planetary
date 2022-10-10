using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ChooseNFTMenuController : MonoBehaviour {
    
    public TMP_Text header;
    public Material matHolder;
    public Image menuImage;
    public TMP_Text counterDisplay;
    
    // Camera animation controls
    public AnimationCurve curve;
    public Transform nextPosition, thisPosition, prevPosition;
    private float _timer;
    private const float CameraShiftTime = 3f;
    
    // void Start()
    // {
    //     header.text = "Collection Loaded\n"+PlayNFT.Instance.collectionName;
    //     if (PlayArcadeIntegration.Instance.nftInCollection > 0)
    //         counterDisplay.text = "1/" + PlayArcadeIntegration.Instance.nftInCollection;
    //     else
    //         counterDisplay.text = "None";
    // }
    
    void Update()
    {
        menuImage.sprite = PlayArcadeIntegration.Instance.nftSprite;
        if (PlayArcadeIntegration.Instance.nftInCollection > 0)
            counterDisplay.text = (PlayArcadeIntegration.Instance.nftSelected + 1).ToString()+"/" + PlayArcadeIntegration.Instance.nftInCollection;
        else
            counterDisplay.text = "None";
    }
    
    public void OnItemClicked(int selected)
    {
        if (selected == 0)
            StartCoroutine(MoveToNextMenu());
        if (selected == 1)
            StartCoroutine(MoveToPrevMenu());
    }

    public void ButtonNext()
    {
        PlayArcadeIntegration.Instance.nftSelected++;
        if (PlayArcadeIntegration.Instance.nftSelected > PlayArcadeIntegration.Instance.nftInCollection-1)
            PlayArcadeIntegration.Instance.nftSelected = 0;
        counterDisplay.text = (PlayArcadeIntegration.Instance.nftSelected + 1).ToString()+"/"+PlayArcadeIntegration.Instance.nftInCollection;
        PlayArcadeIntegration.Instance.processScribeData(PlayArcadeIntegration.Instance.nftSelected);
        StartCoroutine(PlayArcadeIntegration.Instance.GetGameTexture(PlayNFT.Instance.imageURL, false, PlayArcadeIntegration.Instance.nftMaterial));
        
        matHolder.mainTexture = menuImage.sprite.texture;
        //header.text = "Collection Loaded\n"+PlayNFT.Instance.collectionName;
    }

    public void ButtonPrev()
    {
        PlayArcadeIntegration.Instance.nftSelected--;
        if (PlayArcadeIntegration.Instance.nftSelected < 0)
            PlayArcadeIntegration.Instance.nftSelected = PlayArcadeIntegration.Instance.nftInCollection - 1;
        counterDisplay.text = (PlayArcadeIntegration.Instance.nftSelected + 1).ToString()+"/"+PlayArcadeIntegration.Instance.nftInCollection;
        
        PlayArcadeIntegration.Instance.processScribeData(PlayArcadeIntegration.Instance.nftSelected);
        StartCoroutine(PlayArcadeIntegration.Instance.GetGameTexture(PlayNFT.Instance.imageURL, false, PlayArcadeIntegration.Instance.nftMaterial));
        
        matHolder.mainTexture = menuImage.sprite.texture;
        //header.text = "Collection Loaded\n"+PlayNFT.Instance.collectionName;
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
