using UnityEngine;

public class PlayerInteractRPG : MonoBehaviour
{
    private SciFiItemPickup currentItem; // 记录脚下踩到了啥

    // 玩家走进物品的 Trigger 感应圈
    private void OnTriggerEnter(Collider other)
    {
        SciFiItemPickup item = other.GetComponent<SciFiItemPickup>();
        if (item != null)
        {
            currentItem = item;
            Debug.Log("可以按 E 拾取了！");
        }
    }

    // 玩家离开感应圈
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<SciFiItemPickup>() == currentItem)
        {
            currentItem = null;
        }
    }

    void Update()
    {
        // 按下 E 键，且脚下确实有东西
        if (Input.GetKeyDown(KeyCode.E) && currentItem != null)
        {
            currentItem.RequestPickup();
            currentItem = null; // 捡完清空
        }
    }
}