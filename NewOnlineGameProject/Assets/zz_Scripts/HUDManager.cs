using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("UI References")]
    public Slider healthSlider;
    public Slider staminaSlider;
    public Slider energySlider;

    public GameObject energyContent;

    // 【新增】头像框下方的名字文本
    public TextMeshProUGUI avatarNameText;

    public TextMeshProUGUI respawnText;

    [Header("Vignette Settings")]
    public Image vignetteImage;          // 拖入你的 SPR_Vignette
    public Color normalColor;            // 正常状态下的颜色（比如现在的深蓝色）
    public Color dangerColor = Color.red;// 危险状态下的颜色（红色）
    public float dangerThreshold = 30f;

    // 缓存本地玩家的血量脚本
    private NetworkHealth localPlayerHealth;
    private NetworkStamina localPlayerStamina; // 【新增】缓存体力脚本

    private PlayerWeightController localPlayerWeight;

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

        staminaSlider.value = localPlayerStamina.currentStamina / localPlayerStamina.maxStamina;

        if (energySlider != null && localPlayerWeight != null)
        {
            // 如果还有 Buff 时间，就显示能量条并更新百分比
            if (localPlayerWeight.currentBuffDuration > 0)
            {
                energyContent.SetActive(true);
                energySlider.value = localPlayerWeight.currentBuffDuration / localPlayerWeight.maxBuffDuration;
            }
            else // 如果没喝饮料，或者时间到了，直接隐藏整个能量条
            {
                energyContent.SetActive(false);
            }
        }

        if (vignetteImage != null)
        {
            if (localPlayerHealth.currentHealth <= dangerThreshold && localPlayerHealth.currentHealth >= 0)
            {
                // 血量低于设定值且没死，变成红色警告
                vignetteImage.color = dangerColor;
            }
            else
            {
                // 血量健康，或者已经死了（复活倒计时中），恢复正常颜色
                vignetteImage.color = normalColor;
            }
        }

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
                localPlayerStamina = hp.GetComponent<NetworkStamina>();


                //获取玩家名字
                if (avatarNameText != null)
                {
                    // 直接尝试获取 Photon 引擎底层的玩家昵称
                    string myName = PhotonNetwork.NickName;

                    // 防御性设计：如果大厅同学还没传名字过来，给个默认编号兜底
                    if (string.IsNullOrEmpty(myName))
                    {
                        myName = "Pilot_" + PhotonNetwork.LocalPlayer.ActorNumber;
                    }

                    avatarNameText.text = myName;
                }
                Debug.Log("HUDManager: 已成功绑定本地玩家血条！");
                break; // 找到了就跳出循环
            }
        }
    }
}