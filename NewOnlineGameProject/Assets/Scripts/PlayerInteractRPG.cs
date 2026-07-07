using UnityEngine;
using Photon.Pun; // 记得加上这句，为了能读取网络状态

public class PlayerInteractRPG : MonoBehaviour
{
    private SciFiItemPickup currentItem;

    private void OnTriggerEnter(Collider other)
    {
        SciFiItemPickup item = other.GetComponent<SciFiItemPickup>();
        if (item != null)
        {
            currentItem = item;
            Debug.Log("可以按 E 拾取了！");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<SciFiItemPickup>() == currentItem)
        {
            currentItem = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentItem != null)
        {
            // 【新增的安全锁】判断现在到底进房间了没有？
            if (!PhotonNetwork.InRoom)
            {
                Debug.LogWarning("别急，还没连上服务器/没进房间，现在不能捡东西！");
                return; // 直接打断，不再往下执行，这样就不会报错了
            }

            Debug.Log("检测到按键，并且网络正常，发送拾取请求！");
            currentItem.RequestPickup();
            currentItem = null;
        }
    }
}