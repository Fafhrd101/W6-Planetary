using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideContactList : MonoBehaviour
{

    public GameObject NavContactList;
    public RectTransform image;
    
    public void OnClick()
    {
        Vector3 rot = image.transform.eulerAngles;
        rot.z = !NavContactList.activeInHierarchy ? 45 : 0;
        image.transform.eulerAngles = rot;
        NavContactList.SetActive(!NavContactList.activeInHierarchy);
    }

    public void CloseNavMap()
    {
        CanvasViewController.Instance.ToggleMap();
    }
}
