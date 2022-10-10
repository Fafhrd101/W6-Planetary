using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionUI : MonoBehaviour
{
    public Slider[] sliders;
    public TMP_Text[] names;

    private void Update()
    {
        DisplayFactionRelations();
    }
    
    private void DisplayFactionRelations()
    {
        var counter = 0;
        foreach (var otherFaction in ObjectFactory.Instance.Factions)
        {
            if (Player.Instance.playerFaction == otherFaction) continue;
            names[counter].text = otherFaction.name;
            sliders[counter].value = Player.Instance.playerFaction.RelationWith(otherFaction);
            counter++;
        }
    }
}
