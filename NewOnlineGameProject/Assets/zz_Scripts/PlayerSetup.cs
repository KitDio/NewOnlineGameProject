using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    [Tooltip("把这个角色预制体里的摄像机或者摄像机控制脚本拖到这里")]
    public GameObject localCamera;

    void Start()
    {
        // 如果这不是我自己的角色，就关掉它的摄像机
        if (!photonView.IsMine)
        {
            if (localCamera != null)
            {
                localCamera.SetActive(false);
            }
        }
    }
}