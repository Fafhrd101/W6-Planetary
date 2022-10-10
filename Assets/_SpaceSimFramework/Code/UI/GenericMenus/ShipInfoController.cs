
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipInfoController : MonoBehaviour
{
    // Reference to scroll content used for adding objects
    public GameObject ScrollContentContainer;
    public Scrollbar ScrollBar;
    public TMP_Text HeaderText;

    public RectTransform textContainer;
    private float keyPressTimer;
    private bool fastScrollDown, fastScrollUp;

    public void Awake()
    {
        textContainer = ScrollContentContainer.GetComponent<RectTransform>();
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ScrollBar.value -= 0.1f * ScrollBar.size;

            keyPressTimer = 0.5f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            keyPressTimer -= Time.deltaTime;
            if (keyPressTimer < 0 && !fastScrollDown)
            {
                fastScrollDown = true;
                keyPressTimer = 0.05f;
            }

            if (keyPressTimer < 0 && fastScrollDown)
            {
                ScrollBar.value -= 0.1f * ScrollBar.size;
                keyPressTimer = 0.05f;
            }
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            fastScrollDown = false;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ScrollBar.value += 0.1f * ScrollBar.size;

            keyPressTimer = 0.5f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            keyPressTimer -= Time.deltaTime;
            if (keyPressTimer < 0 && !fastScrollUp)
            {
                fastScrollUp = true;
                keyPressTimer = 0.05f;
            }

            if (keyPressTimer < 0 && fastScrollUp)
            {

                ScrollBar.value += 0.1f * ScrollBar.size;

                keyPressTimer = 0.05f;
            }
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            fastScrollUp = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventManager.OnCloseClicked(System.EventArgs.Empty, gameObject);
        }
    }

    /// <summary>
    /// This method is invoked by the element that populates this menu. The method 
    /// allows a text item to be added with optional bolding and color choice.
    /// </summary>
    /// <param name="text">Title/text of the menu option</param>
    /// <returns>OnClick event handler to be used by the invoker</returns>
    public void AddMenuItem(string text, bool isBold, Color color)
    {
        GameObject listItem = Instantiate(UIElements.Instance.TextPanel);
        listItem.name = text;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        //print("rt:"+rt+" "+"text:"+textContainer);
        rt.SetParent(textContainer.transform);
        rt.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, rt.rect.height);

        TMP_Text t = listItem.GetComponentInChildren<TMP_Text>();
        t.text = text;

        if (isBold)
            t.fontStyle = FontStyles.Bold;

        t.color = color;
    }

    /// <summary>
    /// This method is invoked by the element that populates this menu. The method 
    /// allows a text item to be added with optional bolding and color choice. This text item
    /// used two text fields, one on the left and one on the right of the panel.
    /// </summary>
    /// <param name="text">Title/text of the menu option</param>
    /// <returns>OnClick event handler to be used by the invoker</returns>
    public void AddMenuItem(string text1, string text2, bool isBold, Color color)
    {
        GameObject listItem = Instantiate(UIElements.Instance.TwoTextPanel);
        listItem.name = text1;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.SetParent(textContainer.transform);
        rt.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, rt.rect.height);

        TMP_Text[] t = listItem.GetComponentsInChildren<TMP_Text>();
        t[0].text = text1;
        t[1].text = text2;

        if (isBold)
        {
            t[0].fontStyle = FontStyles.Bold;
            t[1].fontStyle = FontStyles.Bold;
        }

        t[0].color = color;
        t[1].color = color;
    }

    /// <summary>
    /// This method is invoked by the element that populates this menu. The method 
    /// returns an onclick handler which can be used by the invoker to process the 
    /// option click/selection events.
    /// </summary>
    /// <param name="text">Title/text of the menu option</param>
    /// <param name="color">Color of the item in the list</param>
    /// <param name="icon">Icon of the item</param>
    /// <param name="iconAspect">Aspect ratio of the icon image</param>
    /// <returns>OnClick event handler to be used by the invoker</returns>
    public void AddMenuItem(string text, Color color, Sprite icon, float iconAspect = 1, int itemHeight = 40)
    {
        GameObject listItem = Instantiate(UIElements.Instance.ClickableImageText);
        listItem.name = text;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.SetParent(textContainer.transform);

        var le = listItem.GetComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = itemHeight;

        NavigationListItem nli = listItem.GetComponent<NavigationListItem>();
        nli.SetText(text);
        nli.Icon.color = color;
        nli.Icon.sprite = icon;
        nli.Icon.GetComponent<AspectRatioFitter>().aspectRatio = iconAspect;
        nli.OwnerMenu = this.gameObject;
    }

    public void ClearItems()
    {
        if (textContainer == null)
            textContainer = ScrollContentContainer.GetComponent<RectTransform>();

        for (int i = 0; i < textContainer.childCount; i++)
            GameObject.Destroy(textContainer.GetChild(i).gameObject);
    }

    public void OnCloseClicked()
    {
        EventManager.OnCloseClicked(System.EventArgs.Empty, gameObject);
    }

    public void OpenInfoMenu(GameObject target)
    {
        var infoMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ShipInfo);
        Ship ship = target.GetComponent<Ship>();

        HeaderText.text = target.name;
        // Faction text in color according to relation with station
        float relation = Ship.PlayerShip.faction.RelationWith(ship.faction);
        AddMenuItem("Faction: " + ship.faction.name, false,
            relation == 0 ? Color.white : (relation < 0 ? Color.red : Color.green));

        AddMenuItem("Model: " + ship.shipModelInfo.modelName, false, Color.white);
        AddMenuItem("Armor: " + ship.armor.ToString("0.0") + " / " + ship.maxArmor, false, Color.white);
        AddMenuItem("Class: " + ship.shipModelInfo.Class, false, Color.white);
        AddMenuItem("Cargobay Size: " + ship.shipModelInfo.CargoSize, false, Color.white);
        AddMenuItem("Equipment Slots: " + ship.shipModelInfo.EquipmentSlots, false, Color.white);
        AddMenuItem("Generator Power: " + ship.shipModelInfo.GeneratorPower, false, Color.white);
        AddMenuItem("Generator Regen rate: " + ship.shipModelInfo.GeneratorRegen, false, Color.white);
        AddMenuItem("Uses external docking: " + ship.shipModelInfo.ExternalDocking, false, Color.white);
        AddMenuItem("Weapons installed onboard: ", true, Color.white);
        int i = 0;
        foreach (var weapon in ship.Equipment.Guns)
        {
            if (weapon.mountedWeapon != null)
                AddMenuItem(
                    "Hardpoint " + i + ": " + weapon.mountedWeapon.name,
                    Color.white,
                    IconManager.Instance.GetWeaponIcon((int) IconManager.EquipmentIcons.Gun), 2f
                );
            else
                AddMenuItem("Hardpoint " + i + ": [no weapon]",
                    Color.grey,
                    IconManager.Instance.GetWeaponIcon((int) IconManager.EquipmentIcons.Gun), 2f
                );
            i++;
        }

        foreach (var weapon in ship.Equipment.Turrets)
        {
            if (weapon.mountedWeapon != null)
                AddMenuItem(
                    "Turret " + i + ": " + weapon.mountedWeapon.name,
                    Color.white,
                    IconManager.Instance.GetWeaponIcon((int) IconManager.EquipmentIcons.Turret), 2f
                );
            else
                AddMenuItem("Hardpoint " + i + ": [no weapon]",
                    Color.grey,
                    IconManager.Instance.GetWeaponIcon((int) IconManager.EquipmentIcons.Turret), 2f
                );
            i++;
        }

        AddMenuItem("Equipment installed onboard: ", true, Color.white);
        foreach (var item in ship.Equipment.MountedEquipment)
        {
            AddMenuItem(item.name, Color.white, IconManager.Instance.GetEquipmentIcon(item.name), 1f, 80);
        }

        AddMenuItem("Cargo carried onboard: ", true, Color.white);
        ShipCargo cargo = target.GetComponent<ShipCargo>();
        foreach (var ware in cargo.cargoContents)
        {
            AddMenuItem(
                ware.itemName + "(" + ware.amount + ")",
                Color.white,
                IconManager.Instance.GetWareIcon(ware.itemName),
                1f, 80
            );
        }
    }
}


