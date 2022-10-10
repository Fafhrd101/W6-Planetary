using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationBroadcastAnnouncement : Singleton<StationBroadcastAnnouncement>
{
    public Image portraitSlot;
    public TMP_Text headerText;
    public TMP_Text announcementText;
    private CanvasGroup canvas;

    void Start()
    {
        canvas = GetComponent<CanvasGroup>();
    }
    public void StationAnnouncement(Station station, string text)
    {
        if (Ship.PlayerShip.isDestroyed)
            return;
        portraitSlot.sprite = station.stationChief;
        canvas.alpha = 1;
        StartCoroutine(FadeMenuToZeroAlpha(3f, GetComponent<CanvasGroup>()));
        this.announcementText.text = text;
        this.headerText.text = station.stationName + " Sector Broadcast";
    }
    public IEnumerator FadeMenuToZeroAlpha(float t, CanvasGroup i)
    {
        i.alpha = 1;
        while (i.alpha > 0.0f)
        {
            i.alpha -= (Time.deltaTime / t);
            yield return null;
        }
    }
}
