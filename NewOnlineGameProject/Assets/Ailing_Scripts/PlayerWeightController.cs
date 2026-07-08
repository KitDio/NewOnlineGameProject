using UnityEngine;
using Photon.Pun;
using System.Collections;
using Synty.AnimationBaseLocomotion.Samples;

public class PlayerWeightController : MonoBehaviourPun
{
    [Header("负重参数")]
    public float maxWeightCapacity = 20f; // 极限重量（超过这个值，速度降到最低）
    public float minimumSpeedMultiplier = 0.2f; // 最低速度倍率（即使超重，也能像蜗牛一样蠕动，保留0.2倍速）

    private SamplePlayerAnimationController movementScript;


    private float originalWalkSpeed;
    private float originalRunSpeed;
    private float originalSprintSpeed;

    // 【新增】用来记录当前身上的临时加速 Buff
    private float currentBuffMultiplier = 1f;
    private Coroutine buffCoroutine;

    void Start()
    {
        movementScript = GetComponent<SamplePlayerAnimationController>();

        // 记录初始的三种速度，作为计算基准
        if (movementScript != null)
        {
            originalWalkSpeed = movementScript._walkSpeed;
            originalRunSpeed = movementScript._runSpeed;
            originalSprintSpeed = movementScript._sprintSpeed;
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

            // 2. 把基础减速和药物Buff乘在一起
            float finalMultiplier = baseSpeedMultiplier * currentBuffMultiplier;

            // 3. 【核心修改】把算好的最终倍率，乘以初始速度，还给 Synty 的移动脚本
            if (movementScript != null)
            {
                movementScript._walkSpeed = originalWalkSpeed * finalMultiplier;
                movementScript._runSpeed = originalRunSpeed * finalMultiplier;
                movementScript._sprintSpeed = originalSprintSpeed * finalMultiplier;
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