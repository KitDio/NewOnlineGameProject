using UnityEngine;
using Photon.Pun;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class StarterAssetPlayerSetup : MonoBehaviourPun
{
    [Header("--- 摄像机与视觉隔离 ---")]
    [Tooltip("拖入主角底下的 MainCamera 和 PlayerFollowCamera")]
    public GameObject[] cameraObjects;

    [Header("--- 需要被禁用的控制组件 ---")]
    [Tooltip("拖入 ThirdPersonController, StarterAssetsInputs, PlayerInput 等")]
    public Behaviour[] localScripts;

    void Start()
    {
        // ==========================================
        // 留给 UI 同学的接口验证：读取当前这个人物的名字
        // ==========================================

        // photonView.Owner.NickName 可以精准抓取到这个模型主人的名字
        string ownerName = photonView.Owner.NickName;

        if (photonView.IsMine)
        {
            Debug.Log("这是我自己的角色，我的名字是: " + ownerName);
        }
        else
        {
            Debug.Log("发现其他玩家，他的名字是: " + ownerName);
        }

        // 【待办】：你同学以后可以在这里写：
        // nameTextUI.text = ownerName;


        if (!photonView.IsMine)
        {
            // 【别人家的克隆体】：没收它的控制器和输入系统
            foreach (Behaviour script in localScripts)
            {
                if (script != null) script.enabled = false;
            }

            // 【别人家的克隆体】：砸碎它的摄像机，防止你的屏幕被别人接管
            foreach (GameObject cam in cameraObjects)
            {
                if (cam != null) cam.SetActive(false);
            }
        }
        else
        {
            // 【我自己】：火力全开，确保所有组件正常运作
            foreach (Behaviour script in localScripts)
            {
                if (script != null) script.enabled = true;
            }

            foreach (GameObject cam in cameraObjects)
            {
                if (cam != null) cam.SetActive(true);
            }
        }
    }
}