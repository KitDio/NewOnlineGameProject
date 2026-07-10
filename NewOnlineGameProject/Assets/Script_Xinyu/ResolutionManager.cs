using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Screen.SetResolution(2560, 1440, FullScreenMode.ExclusiveFullScreen);
    }
}