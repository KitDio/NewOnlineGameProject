using UnityEngine;
using Photon.Pun;
using System.Collections;
using Synty.AnimationBaseLocomotion.Samples;

public class PlayerWeightController : MonoBehaviourPun
{
    [Header("负重参数")]
    public float maxWeightCapacity = 20f;
    public float minimumSpeedMultiplier = 0.2f;

    [Header("Buff 状态")]
    public float maxBuffDuration = 0f;
    public float currentBuffDuration = 0f;

    private SamplePlayerAnimationController movementScript;
    private float originalWalkSpeed;
    private float originalRunSpeed;
    private float originalSprintSpeed;

    private float currentBuffMultiplier = 1f;   // 喝饮料加速
    private float currentDebuffMultiplier = 1f; // 【新增】被子弹打中减速

    private Coroutine buffCoroutine;
    private Coroutine debuffCoroutine; // 【新增】管理减速的协程

    void Start()
    {
        movementScript = GetComponent<SamplePlayerAnimationController>();

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

            // 1. 基础负重减速
            float baseSpeedMultiplier = 1f - (currentWeight / maxWeightCapacity);
            baseSpeedMultiplier = Mathf.Clamp(baseSpeedMultiplier, minimumSpeedMultiplier, 1f);

            // 2. 【核心修改】最终倍率 = 负重倍率 * 饮料Buff倍率 * 子弹减速倍率
            float finalMultiplier = baseSpeedMultiplier * currentBuffMultiplier * currentDebuffMultiplier;

            if (movementScript != null)
            {
                movementScript._walkSpeed = originalWalkSpeed * finalMultiplier;
                movementScript._runSpeed = originalRunSpeed * finalMultiplier;
                movementScript._sprintSpeed = originalSprintSpeed * finalMultiplier;
            }
        }
    }

    // 喝饮料加速调用的方法
    public void ApplySpeedBuff(float buffMultiplier, float duration)
    {
        if (buffCoroutine != null) StopCoroutine(buffCoroutine);
        buffCoroutine = StartCoroutine(SpeedBuffRoutine(buffMultiplier, duration));
    }

    private IEnumerator SpeedBuffRoutine(float buffMultiplier, float duration)
    {
        currentBuffMultiplier = buffMultiplier;
        maxBuffDuration = duration;
        currentBuffDuration = duration;

        // 只要时间大于 0，就每一帧扣除时间
        while (currentBuffDuration > 0)
        {
            currentBuffDuration -= Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 倒计时结束，重置参数
        currentBuffDuration = 0f;
        currentBuffMultiplier = 1f;
        Debug.Log("【系统提示】药效已过，移速恢复正常。"); //[cite: 15]
    }

    // 【新增】被冰冻子弹打中调用的方法
    public void ApplySpeedDebuff(float debuffMultiplier, float duration)
    {
        if (debuffCoroutine != null) StopCoroutine(debuffCoroutine);
        debuffCoroutine = StartCoroutine(SpeedDebuffRoutine(debuffMultiplier, duration));
    }

    private IEnumerator SpeedDebuffRoutine(float debuffMultiplier, float duration)
    {
        currentDebuffMultiplier = debuffMultiplier;
        yield return new WaitForSeconds(duration);
        currentDebuffMultiplier = 1f; // 持续时间到，恢复正常乘数
        Debug.Log("【系统提示】冰冻效果已过，移速恢复。");
    }
}