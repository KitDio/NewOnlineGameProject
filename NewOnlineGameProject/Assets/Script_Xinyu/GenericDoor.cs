using UnityEngine;

public class GenericDoor : MonoBehaviour
{
    public enum DoorType { Slide, Rotate }

    [Header("--- 把要动的门模型拖到这里！ ---")]
    public Transform movingPart;

    [Header("--- 门的基础类型设置 ---")]
    public DoorType doorType = DoorType.Slide;

    [Header("--- 门开启时的目标状态 ---")]
    public Vector3 openPositionOffset;
    public Vector3 openRotationOffset;

    [Header("--- 动画与延迟 ---")]
    public float speed = 5f;
    public float closeDelay = 2f;

    private Vector3 closedPosition;
    private Quaternion closedRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // 【核心新增】记录当前在 Trigger 内部的玩家数量
    private int playersInTriggerCount = 0;

    void Start()
    {
        if (movingPart == null) return;
        closedPosition = movingPart.localPosition;
        closedRotation = movingPart.localRotation;
        targetPosition = closedPosition;
        targetRotation = closedRotation;
    }

    void Update()
    {
        if (movingPart == null) return;

        if (doorType == DoorType.Slide)
        {
            movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, targetPosition, Time.deltaTime * speed);
        }
        else if (doorType == DoorType.Rotate)
        {
            movingPart.localRotation = Quaternion.Lerp(movingPart.localRotation, targetRotation, Time.deltaTime * speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTriggerCount++; // 进来一个人，计数器加 1

            // 只有当第一个人进来时，才真正执行开门和打断关门逻辑
            if (playersInTriggerCount == 1)
            {
                CancelInvoke("CloseDoor");
                targetPosition = closedPosition + openPositionOffset;
                targetRotation = Quaternion.Euler(openRotationOffset);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTriggerCount--; // 离开一个人，计数器减 1

            // 防御性代码：防止计数器变成负数
            if (playersInTriggerCount < 0) playersInTriggerCount = 0;

            // 只有当最后一个人也离开了（计数器归零），门才启动关门倒计时！
            if (playersInTriggerCount == 0)
            {
                Invoke("CloseDoor", closeDelay);
            }
        }
    }

    private void CloseDoor()
    {
        targetPosition = closedPosition;
        targetRotation = closedRotation;
    }
}