using UnityEngine;

public class GraphicsAPIInfo : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Graphics Device Type: " + SystemInfo.graphicsDeviceType);
        Debug.Log("Graphics Device Name: " + SystemInfo.graphicsDeviceName);
        Debug.Log("Graphics Device Version: " + SystemInfo.graphicsDeviceVersion);
    }
}
