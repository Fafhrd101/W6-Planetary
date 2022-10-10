﻿using UnityEngine;

public class TargetScrollMenu : ScrollMenuController
    {
        public GameObject closeButton;

        protected new void Start()
        {
            base.Start();
            closeButton.transform.SetParent(OptionContainer.transform,true);
        }
        public void PopulateMenuOptions(GameObject target)
        {
            HeaderText.text = "Target: " + target.name;

            AddMenuOption("Info").AddListener(() =>
            {
                OpenInfoMenu(target);
            });

            if (target.tag == "Ship" && target.GetComponent<Ship>().faction == Ship.PlayerShip.faction)
            {
                AddMenuOption("Commands").AddListener(() =>
                {
                    SubMenu = GameObject.Instantiate(UIElements.Instance.SimpleMenu, transform.parent);
                    RectTransform rt = SubMenu.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(
                        Screen.width / 2 + GetComponent<RectTransform>().sizeDelta.x / 2,
                        Screen.height / 2 + GetComponent<RectTransform>().sizeDelta.x / 2
                    );

                    SimpleMenuController commsMenu = SubMenu.GetComponent<SimpleMenuController>();
                    commsMenu.IsSubMenu = true;

                    commsMenu.HeaderText.text = "AI Command Console" + target.name;
                    ShipAI shipAI = target.GetComponent<ShipAI>();

                    commsMenu.AddMenuOption("Move To ...").AddListener(() =>
                    {
                        shipAI.MoveTo(null);
                    });

                    commsMenu.AddMenuOption("Follow me").AddListener(() =>
                    {
                        shipAI.FollowMe();
                    });

                    commsMenu.AddMenuOption("Follow ...").AddListener(() =>
                    {
                        shipAI.Follow(null);
                    });

                    commsMenu.AddMenuOption("Idle").AddListener(() =>
                    {
                        shipAI.Idle();
                    });

                    commsMenu.AddMenuOption("Attack Enemies").AddListener(() =>
                    {
                        shipAI.AttackAll();
                    });

                    commsMenu.AddMenuOption("Dock at ...").AddListener(() =>
                    {
                        shipAI.DockAt(null);
                    });

                    // commsMenu.AddMenuOption("Trade in sector").AddListener(() =>
                    // {
                    //     shipAI.AutoTrade();
                    // });

                    if (target == Ship.PlayerShip.gameObject)
                        commsMenu.AddMenuOption("Attack Target").AddListener(() =>
                        {
                            GameObject UItarget = InputHandler.Instance.GetCurrentSelectedTarget();
                            if (UItarget != null)
                                shipAI.Attack(UItarget);
                        });

                });

                // If target is playership and has turrets
                if (target == Ship.PlayerShip.gameObject && target.GetComponent<ShipEquipment>().Turrets.Count > 0)
                    AddMenuOption("Turret Commands").AddListener(() =>
                    {
                        SubMenu = GameObject.Instantiate(UIElements.Instance.SimpleMenu, transform.parent);
                        RectTransform rt = SubMenu.GetComponent<RectTransform>();
                        rt.anchoredPosition = new Vector2(
                            Screen.width / 2 + GetComponent<RectTransform>().sizeDelta.x / 2,
                            Screen.height / 2
                        );

                        SimpleMenuController commsMenu = SubMenu.GetComponent<SimpleMenuController>();
                        commsMenu.IsSubMenu = true;

                        commsMenu.HeaderText.text = "Turret Control" + target.name;
                        ShipEquipment shipWeps = target.GetComponent<ShipEquipment>();

                        commsMenu.AddMenuOption("None").AddListener(() =>
                        {
                            shipWeps.SetTurretCommand(TurretCommands.TurretOrder.None);
                        });

                        commsMenu.AddMenuOption("Attack Enemies").AddListener(() =>
                        {
                            shipWeps.SetTurretCommand(TurretCommands.TurretOrder.AttackEnemies);
                        });

                        commsMenu.AddMenuOption("Attack Target").AddListener(() =>
                        {
                            shipWeps.SetTurretCommand(TurretCommands.TurretOrder.AttackTarget);
                        });

                        commsMenu.AddMenuOption("Manual").AddListener(() =>
                        {
                            shipWeps.SetTurretCommand(TurretCommands.TurretOrder.Manual);
                        });
                    });

                // If is player owned
                if (target.GetComponent<Ship>().faction == Ship.PlayerShip.faction)
                    AddMenuOption("Change Ship").AddListener(() =>
                    {
                        Ship otherShip = target.GetComponent<Ship>();

                        Camera.main.GetComponent<CameraController>().SetTargetShip(otherShip);
                        Ship.PlayerShip.isPlayerControlled = false;
                        otherShip.isPlayerControlled = true;
                        EquipmentIconUI.Instance.SetIconsForShip(otherShip);

                        OnCloseClicked();
                    });
            }

            if (target.CompareTag("Station"))
            {
                AddMenuOption("Comms").AddListener(() =>
                {
                    SubMenu = GameObject.Instantiate(UIElements.Instance.SimpleMenu, transform.parent);
                    RectTransform rt = SubMenu.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(
                        Screen.width / 2 + GetComponent<RectTransform>().sizeDelta.x / 2,
                        Screen.height / 2
                    );

                    SimpleMenuController commsMenu = SubMenu.GetComponent<SimpleMenuController>();
                    commsMenu.IsSubMenu = true;
                    commsMenu.HeaderText.text = "Comms open with " + target.name;

                    commsMenu.AddMenuOption("Request docking").AddListener(() =>
                    {
                        Station station = target.GetComponent<Station>();
                        try
                        {
                            station.RequestDocking(Ship.PlayerShip.gameObject);
                        }
                        catch (DockingException) { }
                    });
                    commsMenu.AddMenuOption("[FORCE DOCK CURRENT SHIP]").AddListener(() =>
                    {
                        Station station = target.GetComponent<Station>();
                        station.ForceDockShip(Ship.PlayerShip);
                    });
                });

                AddMenuOption("Show docked ships").AddListener(() =>
                {
                    Station station = target.GetComponent<Station>();
                    var infoMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollMenu);
                    ScrollMenuController dockedList = infoMenu.GetComponent<ScrollMenuController>();
                    dockedList.HeaderText.text = "Ships docked at " + target.name;

                    for (int ship_i = 0; ship_i < station.dockedShips.Count; ship_i++)
                    {
                        GameObject ship = station.dockedShips[ship_i];
                        dockedList.AddMenuOption(ship.name).AddListener(() =>
                        {
                            var submenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollMenu);
                            ScrollMenuController dockedShipMenu = submenu.GetComponent<ScrollMenuController>();

                            // General options for any ship
                            dockedShipMenu.AddMenuOption("Info").AddListener(() =>
                            {
                                OpenInfoMenu(target);
                            });

                            if (ship.GetComponent<Ship>().faction.name != Ship.PlayerShip.faction.name)
                                return;

                            // Options for player-owned ships
                            dockedShipMenu.AddMenuOption("Undock Ship").AddListener(() =>
                            {
                                station.UndockShip(ship);
                                ship.GetComponent<ShipPlayerInput>().throttle = 0.5f;
                            });
                        });
                    }
                });

                if (target.tag == "Waypoint")
                    AddMenuOption("Autpilot: Move To").AddListener(() =>
                    {
                        Ship.PlayerShip.AIInput.MoveTo(target.transform);
                    });
            }
        }

        public static void OpenInfoMenu(GameObject target)
        {
            if (target.tag == "Station")
            {
                var infoMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText);
                ScrollTextController infoCard = infoMenu.GetComponent<ScrollTextController>();
                
                Station station = target.GetComponent<Station>();

                // Faction text in color according to relation with station
                float relation = Ship.PlayerShip.faction.RelationWith(station.faction);
                infoCard.AddMenuItem("Faction: " + station.faction.name, false,
                    relation == 0 ? Color.white : (relation < 0 ? Color.red : Color.green));

                StationDealer dealer = target.GetComponent<StationDealer>();
                foreach (var ware in dealer.WarePrices)
                {
                    infoCard.AddMenuItem(
                        ware.Key + "(" + ware.Value + ")",
                        Color.white,
                        IconManager.Instance.GetWareIcon(ware.Key)
                    );
                }
            }
            else if (target.tag == "Ship")
            {
                var infoMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ShipInfo);
                ShipInfoController infoCard = infoMenu.GetComponent<ShipInfoController>();
                
                Ship ship = target.GetComponent<Ship>();

                infoCard.HeaderText.text = target.name;
                // Faction text in color according to relation with station
                float relation = Ship.PlayerShip.faction.RelationWith(ship.faction);
                infoCard.AddMenuItem("Faction: " + ship.faction.name, false,
                    relation == 0 ? Color.white : (relation < 0 ? Color.red : Color.green));

                infoCard.AddMenuItem("Model: " + ship.shipModelInfo.modelName, false, Color.white);
                infoCard.AddMenuItem("Armor: " + ship.armor.ToString("0.0") + " / " + ship.maxArmor, false, Color.white);
                infoCard.AddMenuItem("Class: " + ship.shipModelInfo.Class, false, Color.white);
                infoCard.AddMenuItem("Cargobay Size: " + ship.shipModelInfo.CargoSize, false, Color.white);
                infoCard.AddMenuItem("Equipment Slots: " + ship.shipModelInfo.EquipmentSlots, false, Color.white);
                infoCard.AddMenuItem("Generator Power: " + ship.shipModelInfo.GeneratorPower, false, Color.white);
                infoCard.AddMenuItem("Generator Regen rate: " + ship.shipModelInfo.GeneratorRegen, false, Color.white);
                infoCard.AddMenuItem("Uses external docking: " + ship.shipModelInfo.ExternalDocking, false, Color.white);
                infoCard.AddMenuItem("Weapons installed onboard: ", true, Color.white);
                int i = 0;
                foreach (var weapon in ship.Equipment.Guns)
                {
                    if (weapon.mountedWeapon != null)
                        infoCard.AddMenuItem(
                            "Hardpoint " + i + ": " + weapon.mountedWeapon.name,
                            Color.white,
                            IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Gun), 2f
                        );
                    else
                        infoCard.AddMenuItem("Hardpoint " + i + ": [no weapon]",
                            Color.grey,
                            IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Gun), 2f
                        );
                    i++;
                }
                foreach (var weapon in ship.Equipment.Turrets)
                {
                    if (weapon.mountedWeapon != null)
                        infoCard.AddMenuItem(
                            "Turret " + i + ": " + weapon.mountedWeapon.name,
                            Color.white,
                            IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Turret), 2f
                        );
                    else
                        infoCard.AddMenuItem("Hardpoint " + i + ": [no weapon]",
                            Color.grey,
                            IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Turret), 2f
                        );
                    i++;
                }

                infoCard.AddMenuItem("Equipment installed onboard: ", true, Color.white);
                foreach (var item in ship.Equipment.MountedEquipment)
                {
                    infoCard.AddMenuItem(item.name, Color.white, IconManager.Instance.GetEquipmentIcon(item.name), 1f, 80);
                }

                infoCard.AddMenuItem("Cargo carried onboard: ", true, Color.white);
                ShipCargo cargo = target.GetComponent<ShipCargo>();
                foreach (var ware in cargo.cargoContents)
                {
                    infoCard.AddMenuItem(
                        ware.itemName + "(" + ware.amount + ")",
                        Color.white,
                        IconManager.Instance.GetWareIcon(ware.itemName),
                        1f, 80
                    );
                }
            }
            else
            {
                var infoMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText);
                ScrollTextController infoCard = infoMenu.GetComponent<ScrollTextController>();
                
                infoCard.HeaderText.text = target.name;
                infoCard.AddMenuItem("No information available", true, Color.yellow);
            }
        }
    }
