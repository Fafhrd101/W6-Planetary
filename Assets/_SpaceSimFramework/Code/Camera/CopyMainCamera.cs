using UnityEngine;

public class CopyMainCamera : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
        transform.position = Camera.main.transform.position;
    }
}
