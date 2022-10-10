using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleEffect : MonoBehaviour
{
    void OnEnable()
    {
        Invoke(nameof(DisableEffect), .25f);
    }

    private void DisableEffect()
    {
        this.gameObject.SetActive(false);
    }
}
