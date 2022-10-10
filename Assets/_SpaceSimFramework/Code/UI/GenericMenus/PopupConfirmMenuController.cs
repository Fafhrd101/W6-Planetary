using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupConfirmMenuController : MonoBehaviour {
    public TMP_Text HeaderText;
    public Button AcceptButton, CancelButton;
    public Image portrait;
    
    // private void Update()
    // {         
    //     if (Input.GetKeyDown(KeyCode.Return))
    //     {
    //         OnAcceptClicked();
    //     }
    //     if (Input.GetKeyDown(KeyCode.Escape))
    //     {
    //         OnCloseClicked();
    //     }
    // }

    #region button callbacks
    public void OnAcceptClicked()
    {
        AcceptButton.onClick.Invoke();  // Call to invoke and listeners attached to the buttons
        GameObject.Destroy(this.gameObject);
    }

    public void OnCloseClicked()
    {
        CancelButton.onClick.Invoke();  // Call to invoke and listeners attached to the buttons
        GameObject.Destroy(this.gameObject);
        Ship.IsShipInputDisabled = false;
        Cursor.visible = false;
        Ship.PlayerShip.UsingMouseInput = true;
    }
    #endregion button callbacks

}
