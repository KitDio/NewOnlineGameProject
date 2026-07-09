using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardItemUI : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI valueText;
    public GameObject mvpImage; // MVP 的皇冠/图标

    // 留给结算管家调用的排版接口
    public void Setup(int rank, string playerName, int value, bool isMVP)
    {
        if (numberText != null) numberText.text = rank.ToString();
        if (playerNameText != null) playerNameText.text = playerName;
        if (valueText != null) valueText.text = "$" + value.ToString();

        if (mvpImage != null) mvpImage.SetActive(isMVP);
    }
}