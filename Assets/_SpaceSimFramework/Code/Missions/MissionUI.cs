using UnityEngine;
using TMPro;

public class MissionUI : Singleton<MissionUI> {

    public GameObject panel;
    public TMP_Text MissionType, Employer, Duration, Description, Payout, Sector;
    private float timer;

    private void Update()
    {
        if (panel.activeInHierarchy)
        {
            timer -= Time.deltaTime;
            Duration.text = "Duration: " + (int)(timer / 60f) + " m " + (int)(timer % 60) + " s";
            if (timer < 0)
                panel.SetActive(false);
        }
    }

    public void SetDescriptionText(string text)
    {
        Description.text = text;
    }

    public void Populate(Mission m_i)
    {
        MissionType.text = m_i.Type.ToString();
        Employer.text = "Employer: " + m_i.Employer.name;
        Duration.text = "Duration: " + (m_i.Duration / 60 + " m");
        Payout.text = m_i.Payout + " Cr";

        timer = m_i.Duration;
    }
}
