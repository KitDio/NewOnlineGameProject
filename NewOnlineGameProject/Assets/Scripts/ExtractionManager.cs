using UnityEngine;
using TMPro;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ExtractionManager : MonoBehaviourPunCallbacks
{
    [Header("关卡设置")]
    public int targetValue = 2000; // 通关需要达到的总金额
    private int currentSubmittedValue = 0; // 当前房间已经递交的金额

    [Header("UI 引用")]
    public GameObject extractionPanel; // 拖入刚才做的 ExtractionPanel
    public TextMeshProUGUI progressText; // 拖入 ProgressText

    void Start()
    {
        if (extractionPanel != null) extractionPanel.SetActive(false);

        // 游戏刚开始时，由主机负责在房间门上贴一张初始金额为 0 的通告
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", 0);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    // 【核心同步】每当房间门上的通告单（自定义属性）发生改变时，自动触发！
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("RoomTotalValue"))
        {
            // 更新本地的数据，并刷新 UI
            currentSubmittedValue = (int)propertiesThatChanged["RoomTotalValue"];
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = $"Submit Progress:\n<color=#FFD700>${currentSubmittedValue}</color> / ${targetValue}";
        }
    }

    // 给 UI 上的“递交物资”按钮调用的方法
    public void SubmitItems()
    {
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory == null) return;

        int valueToSubmit = inventory.GetTotalValue();
        if (valueToSubmit > 0)
        {
            // 算出新的总额
            int newValue = currentSubmittedValue + valueToSubmit;

            // 把新总额写进通告单，Photon 会自动同步给所有人
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", newValue);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            // 清空本地玩家的背包
            inventory.ClearAllItems();
        }
        else
        {
            Debug.LogWarning("你的包里没东西，或者东西一文不值！");
        }
    }

    // 给 UI 上的“启动撤离”按钮调用的方法
    public void TryExtract()
    {
        if (currentSubmittedValue >= targetValue)
        {
            Debug.Log("<color=green>【任务成功】已达到目标金额，飞船启动，成功撤离！</color>");
            // TODO: 未来可以在这里写切换回大厅场景的代码
        }
        else
        {
            Debug.Log("<color=red>【任务失败】金额不足，强行撤离，雇主非常生气，任务失败！</color>");
            // TODO: 未来可以在这里写死亡/失败界面的代码
        }
    }

    // 给物理触发器控制开关的方法
    public void OpenExtractionUI() { extractionPanel.SetActive(true); UpdateUI(); }
    public void CloseExtractionUI() { extractionPanel.SetActive(false); }
}