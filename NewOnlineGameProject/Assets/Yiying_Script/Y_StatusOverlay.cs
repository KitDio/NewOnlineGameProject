using UnityEngine;
using UnityEngine.UI;

public class Y_StatusOverlay : MonoBehaviour
{
    public Image burnOverlay;
    public Image freezeOverlay;

    void Start()
    {
        burnOverlay.enabled = false;
        freezeOverlay.enabled = false;
    }

    public void ShowBurn(bool show)
    {
        burnOverlay.enabled = show;
    }

    public void ShowFreeze(bool show)
    {
        freezeOverlay.enabled = show;
    }
}