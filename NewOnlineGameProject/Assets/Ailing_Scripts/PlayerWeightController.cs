using UnityEngine;
using Photon.Pun;
using StarterAssets;
using System.Collections;

public class PlayerWeightController : MonoBehaviourPun
{
    [Header("负重参数")]
    public float maxWeightCapacity = 20f; // 极限重量（超过这个值，速度降到最低）
    public float minimumSpeedMultiplier = 0.2f; // 最低速度倍率（即使超重，也能像蜗牛一样蠕动，保留0.2倍速）

    // 存放 Starter Asset 控制器的引用
    private ThirdPersonController thirdPersonController;


    // 记录玩家初始的健康速度
    private float originalMoveSpeed;
    private float originalSprintSpeed;

    // 【新增】用来记录当前身上的临时加速 Buff
    private float currentBuffMultiplier = 1f;
    private Coroutine buffCoroutine;

    void Start()
    {
        // 尝试获取玩家身上的控制器（兼容第一人称和第三人称）
        thirdPersonController = GetComponent<ThirdPersonController>();

        // 记录初始速度，作为计算基准
        if (thirdPersonController != null)
        {
            originalMoveSpeed = thirdPersonController.MoveSpeed;
            originalSprintSpeed = thirdPersonController.SprintSpeed;
        }
    }

    void Update()
    {
        if (photonView != null && !photonView.IsMine) return;

        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null)
        {
            float currentWeight = inventory.GetTotalWeight();

            // 1. 算出负重导致的基础减速
            float baseSpeedMultiplier = 1f - (currentWeight / maxWeightCapacity);
            baseSpeedMultiplier = Mathf.Clamp(baseSpeedMultiplier, minimumSpeedMultiplier, 1f);

            // 2. 【核心修改】把基础减速 和 药物Buff 乘在一起！
            float finalMultiplier = baseSpeedMultiplier * currentBuffMultiplier;

            // 3. 应用最终速度
            if (thirdPersonController != null)
            {
                thirdPersonController.MoveSpeed = originalMoveSpeed * finalMultiplier;
                thirdPersonController.SprintSpeed = originalSprintSpeed * finalMultiplier;
            }
        }
    }

    public void ApplySpeedBuff(float buffMultiplier, float duration)
    {
        // 如果连续打针，先停掉上一个倒计时
        if (buffCoroutine != null) StopCoroutine(buffCoroutine);
        // 开启新的打针倒计时
        buffCoroutine = StartCoroutine(SpeedBuffRoutine(buffMultiplier, duration));
    }

    private IEnumerator SpeedBuffRoutine(float buffMultiplier, float duration)
    {
        currentBuffMultiplier = buffMultiplier; // 获得加速
        yield return new WaitForSeconds(duration); // 等待药效过去
        currentBuffMultiplier = 1f; // 药效结束，打回原形
        Debug.Log("【系统提示】肾上腺素药效已过，移速恢复正常。");
    }
}