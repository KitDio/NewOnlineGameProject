using UnityEngine;
using Photon.Pun;
using StarterAssets;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerSetup : MonoBehaviourPun
{
    void Start()
    {
        if (photonView.IsMine)
        {
            Debug.Log("【系统】这是我的玩家！正在满世界寻找摄像机...");

            // 1. 尝试寻找场景里的虚拟摄像机
            CinemachineVirtualCamera vCam = FindObjectOfType<CinemachineVirtualCamera>();

            if (vCam != null)
            {
                // 2. Starter Assets 的标配：寻找那个专门用来挂相机的骨骼 (PlayerCameraRoot)
                // 之前的 transform.Find 只能找第一层子物体，现在我们用递归把它翻出来
                Transform cameraRoot = null;
                Transform[] allChildren = GetComponentsInChildren<Transform>();
                foreach (Transform child in allChildren)
                {
                    if (child.name == "PlayerCameraRoot")
                    {
                        cameraRoot = child;
                        break;
                    }
                }

                // 3. 强行绑架！
                Transform target = cameraRoot != null ? cameraRoot : transform;
                vCam.Follow = target;

                Debug.Log($"<color=green>【摄像机绑定成功！】已成功将镜头强行绑定给: {target.name}</color>");
            }
            else
            {
                Debug.LogError("【严重错误】完蛋了！场景里根本找不到 CinemachineVirtualCamera！你是不是把它不小心删了？");
            }
        }
        else
        {
            Debug.Log("【系统】检测到克隆体，已剥夺其身体控制权。");
            ThirdPersonController tpc = GetComponent<ThirdPersonController>();
            if (tpc != null) tpc.enabled = false;

            PlayerInput pi = GetComponent<PlayerInput>();
            if (pi != null) pi.enabled = false;
        }
    }
}