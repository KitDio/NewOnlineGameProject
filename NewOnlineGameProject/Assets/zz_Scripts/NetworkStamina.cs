using UnityEngine;
using Photon.Pun;
using Synty.AnimationBaseLocomotion.Samples.InputSystem; // 引入 InputReader 的命名空间

public class NetworkStamina : MonoBehaviourPun
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Tooltip("每秒消耗的体力值")]
    public float sprintDrainRate = 20f;

    [Tooltip("每秒回复的体力值")]
    public float regenRate = 15f;

    [Tooltip("停止奔跑后，等待多久开始回复体力")]
    public float regenDelay = 1f;

    // 状态标记
    private bool isSprinting = false;
    private float lastSprintTime = 0f;

    // 获取 InputReader 引用
    private InputReader inputReader;

    void Start()
    {
        currentStamina = maxStamina;
        inputReader = GetComponent<InputReader>();

        // 如果是本地玩家，监听 InputReader 里已经写好的疾跑事件
        if (photonView.IsMine && inputReader != null)
        {
            // 当按下 Shift 触发 onSprintActivated 时，执行 StartSprint 方法
            inputReader.onSprintActivated += StartSprint;
            // 当松开 Shift 触发 onSprintDeactivated 时，执行 StopSprint 方法
            inputReader.onSprintDeactivated += StopSprint;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // 1. 消耗体力逻辑
        if (isSprinting)
        {
            currentStamina -= sprintDrainRate * Time.deltaTime;
            lastSprintTime = Time.time; // 记录最后一次奔跑的时间

            // 如果体力耗尽
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                ForceStopSprint();
            }
        }
        // 2. 回复体力逻辑
        else
        {
            // 只有当经过了延迟时间，且体力不满时，才开始回复
            if (Time.time - lastSprintTime > regenDelay && currentStamina < maxStamina)
            {
                currentStamina += regenRate * Time.deltaTime;
                if (currentStamina > maxStamina) currentStamina = maxStamina;
            }
        }
    }

    private void StartSprint()
    {
        // 只有体力大于 0 才能起跑
        if (currentStamina > 0)
        {
            isSprinting = true;
        }
    }

    private void StopSprint()
    {
        isSprinting = false;
    }

    private void ForceStopSprint()
    {
        isSprinting = false;

        // 【核心交互】
        // 当体力耗尽时，我们需要强制通知系统的 InputReader 或 Locomotion 脚本停止奔跑。
        // 由于 InputReader 里预留了委托，我们可以主动调用 Deactivate 事件来打断动作。
        if (inputReader != null)
        {
            inputReader.onSprintDeactivated?.Invoke();
        }
    }

    void OnDestroy()
    {
        // 养成好习惯，销毁时注销事件，防止内存泄漏
        if (inputReader != null)
        {
            inputReader.onSprintActivated -= StartSprint;
            inputReader.onSprintDeactivated -= StopSprint;
        }
    }
}