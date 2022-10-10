﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupSliderMenuController : MonoBehaviour {

    public TMP_Text HeaderText;
    public TMP_Text InfoText;
    public TMP_Text AmountText;
    public Slider Slider;
    public Button AcceptButton, CancelButton;

    private float keyPressTimer;
    private bool fastScrollLeft, fastScrollRight;

    private void Start()
    {
        AmountText.text = "0";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Slider.value --;

            keyPressTimer = 0.5f;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            keyPressTimer -= Time.deltaTime;
            if (keyPressTimer < 0 && !fastScrollLeft)
            {
                fastScrollLeft = true;
                keyPressTimer = 0.05f;
            }
            if (keyPressTimer < 0 && fastScrollLeft)
            {
                Slider.value--;
                keyPressTimer = 0.05f;
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            fastScrollLeft = false;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Slider.value ++;
       
            keyPressTimer = 0.5f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            keyPressTimer -= Time.deltaTime;
            if (keyPressTimer< 0 && !fastScrollRight)
            {
                fastScrollRight = true;
                keyPressTimer = 0.05f;
            }
            if (keyPressTimer< 0 && fastScrollRight)
            {
                Slider.value++;
                keyPressTimer = 0.05f;
            }
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            fastScrollRight = false;
        }


        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnAcceptClicked();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCloseClicked();
        }
    }

    public float GetSliderValue()
    {
        return Slider.value;
    }

    #region button callbacks
    public void OnAcceptClicked()
    {
        AcceptButton.onClick.Invoke();
        GameObject.Destroy(this.gameObject);
    }

    public void OnCloseClicked()
    {
        CancelButton.onClick.Invoke();
        GameObject.Destroy(this.gameObject);
    }
    #endregion button callbacks

    #region text setters
    public void SetHeaderText(string newText)
    {
        HeaderText.text = newText;
    }

    public void SetInfoText(string newText)
    {
        InfoText.text = newText;
    }

    public void SetTextFields(string header, string text1)
    {
        HeaderText.text = header;
        InfoText.text = text1;
    }
    #endregion text setters

}
