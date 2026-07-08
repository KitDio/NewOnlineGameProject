using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractUIManager : MonoBehaviour
{
    // 【核心魔法】静态单例，让全世界都能直接访问这个脚本
    public static InteractUIManager Instance;

    [Header("UI 引用 (拖入场景里的 UI)")]
    public GameObject interactPanel;
    public TextMeshProUGUI interactPromptText;
    public Image interactIcon;
    public Sprite defaultLootBoxIcon;

    void Awake()
    {
        // 游戏一开始，就把自己挂上牌子：“我是唯一的 UI 管家”
        Instance = this;
    }

    void Start()
    {
        // 确保游戏开始时，面板是隐藏的
        if (interactPanel != null)
        {
            interactPanel.SetActive(false);
        }
    }
}