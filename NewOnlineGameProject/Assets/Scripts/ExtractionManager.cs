using UnityEngine;
using TMPro;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ExtractionManager : MonoBehaviourPunCallbacks
{
    [Header("关卡设置")]
    public int targetValue = 2000;
    private int currentSubmittedValue = 0;

    [Header("UI 引用")]
    public GameObject extractionPanel;
    public TextMeshProUGUI progressText;

    void Start()
    {
        if (extractionPanel != null) extractionPanel.SetActive(false);

        // 【关键修复】游戏一开始（不管有没有连上网），直接强行刷新一次默认的 0 元文本！
        currentSubmittedValue = 0;
        UpdateUI();
    }

    // 【关键修复】把查房间属性的逻辑，挪到“真正进入房间后”再执行
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 房主负责贴初始的 0 元通告
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", 0);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RoomTotalValue"))
        {
            // 如果是后进房间的玩家，开局先同步一下当前的真实进度
            currentSubmittedValue = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomTotalValue"];
            UpdateUI(); // 拿到真实数据后，再刷新一次盖掉默认的 0
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("RoomTotalValue"))
        {
            currentSubmittedValue = (int)propertiesThatChanged["RoomTotalValue"];
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            // 默认显示文本，金额带高亮颜色
            progressText.text = $"Submit Progress:\n<color=#FFD700>${currentSubmittedValue}</color> / ${targetValue}";
        }
    }

    public void SubmitItems()
    {
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory == null) return;

        int valueToSubmit = inventory.GetTotalValue();
        if (valueToSubmit > 0)
        {
            int newValue = currentSubmittedValue + valueToSubmit;
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", newValue);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            inventory.ClearAllItems();
        }
        else
        {
            Debug.LogWarning("你的包里没东西，或者东西一文不值！");
        }
    }

    public void TryExtract()
    {
        if (currentSubmittedValue >= targetValue)
        {
            Debug.Log("<color=green>【任务成功】已达到目标金额，飞船启动，成功撤离！</color>");
        }
        else
        {
            Debug.Log("<color=red>【任务失败】金额不足，强行撤离，雇主非常生气，任务失败！</color>");
        }
    }

    public void OpenExtractionUI()
    {
        extractionPanel.SetActive(true);
    }

    public void CloseExtractionUI()
    {
        extractionPanel.SetActive(false);
    }
    void Update()
    {
        // 只有当玩家站在撤离点旁边，且撤离面板成功弹出时，快捷键才允许触发
        if (extractionPanel != null && extractionPanel.activeSelf)
        {
            // 按 Q 键递交物资
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SubmitItems();
                // 可选：加个小提示，证明是按键触发的
                Debug.Log("通过快捷键 [Q] 触发了递交！");
            }

            // 按 X 键尝试撤离
            if (Input.GetKeyDown(KeyCode.X))
            {
                TryExtract();
                Debug.Log("通过快捷键 [X] 触发了撤离！");
            }
        }
    }
    public void AddFundsDirectly(int amount)
    {
        if (!PhotonNetwork.InRoom) return;

        // 算出新的总额
        int newValue = currentSubmittedValue + amount;

        // 把新总额写进通告单，Photon 会自动同步给所有人，并触发 UpdateUI
        Hashtable hash = new Hashtable();
        hash.Add("RoomTotalValue", newValue);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        Debug.Log($"<color=cyan>【ATM 提示】刷卡成功！直接为全队进度增加了 ${amount}！</color>");
    }
}