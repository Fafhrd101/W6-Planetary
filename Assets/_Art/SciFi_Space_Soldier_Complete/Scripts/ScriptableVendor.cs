using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableVendor")]
public class ScriptableVendor : ScriptableObject
{
    public AudioClip[] audioClips;
    
    public void PlayAudioClip(int i)
    {
        AudioSource.PlayClipAtPoint(audioClips[i], Vector3.zero);
    }

    public void OpenPanel()
    {
        Debug.Log("Opening vendor... somewhere.");
    }
}
