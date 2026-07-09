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

    private void ShowOverlay(Image overlay, bool show)
    {
        if (overlay != null)
        {
            overlay.enabled = show;
        }
    }

    public void ShowBurn(bool show)
    {
        ShowOverlay(burnOverlay, show);
    }

    public void ShowFreeze(bool show)
    {
        ShowOverlay(freezeOverlay, show);
    }
}