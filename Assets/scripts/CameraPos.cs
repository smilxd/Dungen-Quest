using UnityEngine;

public class CameraPos : MonoBehaviour
{
    public Transform CameraPosi;

    private void Update()
    {
        transform.position = CameraPosi.position;
    }

}
