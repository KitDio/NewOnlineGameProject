using UnityEngine;
using Photon.Pun;
using System.Collections.Generic; // 必须引入这个才能用 List

// 创建一个新的数据结构，用来在 Inspector 里给每个门板单独设置参数
[System.Serializable]
public class DoorPanel
{
    public Transform movingPart;
    public Vector3 openPositionOffset;
    public Vector3 openRotationOffset;

    [HideInInspector] public Vector3 closedPosition;
    [HideInInspector] public Quaternion closedRotation;
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public Quaternion targetRotation;
}

public class GenericDoor : MonoBehaviourPun
{
    public enum DoorType { Slide, Rotate }

    [Header("--- 门的基础类型设置 ---")]
    public DoorType doorType = DoorType.Slide;

    [Header("--- 把你要动的所有门板都加进这个列表里！ ---")]
    public List<DoorPanel> doorPanels = new List<DoorPanel>();

    [Header("--- 动画与延迟 ---")]
    public float speed = 5f;
    public float closeDelay = 2f;

    private int playersInTriggerCount = 0;

    [Header("--- 门音效设置 ---")]
    [Tooltip("直接把当前物体身上的 AudioSource 组件拖进来")]
    public AudioSource doorAudioSource;
    public AudioClip openDoorSFX;
    public AudioClip closeDoorSFX;

    private bool isDoorOpen = false; // 【新增】用于记录当前门状态，防止音效被重复触发

    void Start()
    {
        // 游戏开始时，记录每一扇门板的初始位置
        foreach (var panel in doorPanels)
        {
            if (panel.movingPart != null)
            {
                panel.closedPosition = panel.movingPart.localPosition;
                panel.closedRotation = panel.movingPart.localRotation;
                panel.targetPosition = panel.closedPosition;
                panel.targetRotation = panel.closedRotation;
            }
        }
    }

    void Update()
    {
        // 每一帧，让列表里的所有门板分别向着自己的目标点平滑移动
        foreach (var panel in doorPanels)
        {
            if (panel.movingPart == null) continue;

            if (doorType == DoorType.Slide)
            {
                panel.movingPart.localPosition = Vector3.Lerp(panel.movingPart.localPosition, panel.targetPosition, Time.deltaTime * speed);
            }
            else if (doorType == DoorType.Rotate)
            {
                panel.movingPart.localRotation = Quaternion.Lerp(panel.movingPart.localRotation, panel.targetRotation, Time.deltaTime * speed);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pView = other.GetComponent<PhotonView>();
            if (pView != null && pView.IsMine)
            {
                photonView.RPC("RpcUpdateDoorCount", RpcTarget.AllBuffered, 1);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pView = other.GetComponent<PhotonView>();
            if (pView != null && pView.IsMine)
            {
                photonView.RPC("RpcUpdateDoorCount", RpcTarget.AllBuffered, -1);
            }
        }
    }

    [PunRPC]
    private void RpcUpdateDoorCount(int change)
    {
        playersInTriggerCount += change;
        if (playersInTriggerCount < 0) playersInTriggerCount = 0;

        if (playersInTriggerCount > 0)
        {
            CancelInvoke("CloseDoor");

            // 【新增控音逻辑】只有当门之前是关着的时候，才发送全网开门音效广播
            if (!isDoorOpen)
            {
                isDoorOpen = true;
                photonView.RPC("RpcPlayDoorSound", RpcTarget.All, true);
            }

            foreach (var panel in doorPanels)
            {
                panel.targetPosition = panel.closedPosition + panel.openPositionOffset;
                panel.targetRotation = Quaternion.Euler(panel.openRotationOffset);
            }
        }
        else if (playersInTriggerCount == 0)
        {
            Invoke("CloseDoor", closeDelay);
        }
    }

    private void CloseDoor()
    {
        // 没人了，让所有门板各自回原位
        foreach (var panel in doorPanels)
        {
            panel.targetPosition = panel.closedPosition;
            panel.targetRotation = panel.closedRotation;
        }

        // 【新增控音逻辑】当执行关门时，发送全网关门音效广播
        if (isDoorOpen)
        {
            isDoorOpen = false;
            photonView.RPC("RpcPlayDoorSound", RpcTarget.All, false);
        }
    }

    [PunRPC]
    private void RpcPlayDoorSound(bool isOpenSound)
    {
        if (doorAudioSource == null) return;

        if (isOpenSound && openDoorSFX != null)
        {
            doorAudioSource.PlayOneShot(openDoorSFX);
        }
        else if (!isOpenSound && closeDoorSFX != null)
        {
            doorAudioSource.PlayOneShot(closeDoorSFX);
        }
    }
}