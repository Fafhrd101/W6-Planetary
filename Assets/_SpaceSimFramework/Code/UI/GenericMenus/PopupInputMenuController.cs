using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupInputMenuController : MonoBehaviour {

    public TMP_Text HeaderText;
    public TMP_InputField TextInput;
    public Button AcceptButton, CancelButton;

    private void Start()
    {
        TextInput.ActivateInputField();    
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnAcceptClicked();
        }
    }

    #region button callbacks
    public void OnAcceptClicked()
    {
        // Placeholder
        GameObject.Destroy(this.gameObject);
    }

    public void OnCloseClicked()
    {
        GameObject.Destroy(this.gameObject);
    }
    #endregion button callbacks

    #region text setters
    public void SetHeaderText(string newText)
    {
        HeaderText.text = newText;
    }

    public void SetPlaceholderText(string newText)
    {
        TextInput.placeholder.GetComponent<Text>().text = newText;
    }

    public void SetTextFields(string header, string text1)
    {
        HeaderText.text = header;
        TextInput.placeholder.GetComponent<Text>().text = text1;
    }
    #endregion text setters

}
