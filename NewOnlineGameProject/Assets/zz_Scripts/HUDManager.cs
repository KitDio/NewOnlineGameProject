using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("UI References")]
    public Slider healthSlider;
    public TextMeshProUGUI respawnText;

    // 缓存本地玩家的血量脚本
    private NetworkHealth localPlayerHealth;
    private float countdownTimer = 0f;

    void Awake()
    {
        // 初始化单例
        Instance = this;
    }

    void Update()
    {
        // 1. 如果还没找到本地玩家，就去场景里找
        if (localPlayerHealth == null)
        {
            FindLocalPlayer();
            return; // 还没找到就不执行下面的 UI 更新逻辑
        }

        // 2. 找到了本地玩家，实时同步血量数据到 Slider
        healthSlider.value = localPlayerHealth.currentHealth / localPlayerHealth.maxHealth;

        if (countdownTimer > 0)
        {
            countdownTimer -= Time.deltaTime;
            respawnText.text = $"CRITICAL TRAUMA.\n Evacuating to medbay...{Mathf.CeilToInt(countdownTimer)} ";

            if (countdownTimer <= 0)
            {
                respawnText.gameObject.SetActive(false); // 倒计时结束，隐藏文本
            }
        }
    }

    public void StartRespawnCountdown(float time)
    {
        countdownTimer = time;
        respawnText.gameObject.SetActive(true);
    }

    private void FindLocalPlayer()
    {
        // 查找场景中所有的 NetworkHealth 组件
        NetworkHealth[] allHealthScripts = FindObjectsOfType<NetworkHealth>();

        foreach (var hp in allHealthScripts)
        {
            // 核心判断：这个对象必须属于“我”（IsMine），并且 Tag 是 Player
            if (hp.photonView != null && hp.photonView.IsMine && hp.gameObject.CompareTag("Player"))
            {
                localPlayerHealth = hp;
                Debug.Log("HUDManager: 已成功绑定本地玩家血条！");
                break; // 找到了就跳出循环
            }
        }
    }
}