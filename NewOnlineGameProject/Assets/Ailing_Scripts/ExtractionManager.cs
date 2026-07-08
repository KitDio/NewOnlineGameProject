using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public enum GameDifficulty
{
    Normal,
    Difficult
}

public class ExtractionManager : MonoBehaviourPunCallbacks
{
    [Header("关卡难度设置")]
    [Tooltip("在 Normal 下单人 10000，Difficult 下单人 20000。金额会自动乘以房间人数。")]
    public GameDifficulty currentDifficulty = GameDifficulty.Normal;

    // 动态计算出来的当前目标（实际判定通关看这个数值）
    private int currentTargetValue = 10000;
    private int currentSubmittedValue = 0;

    [Header("UI 引用")]
    public GameObject extractionPanel;
    public TextMeshProUGUI progressText;

    void Start()
    {
        if (extractionPanel != null) extractionPanel.SetActive(false);
        currentSubmittedValue = 0;

        // 游戏一开始，先算一次单人的基础钱数显示出来
        UpdateDynamicTarget();
    }

    public override void OnJoinedRoom()
    {
        UpdateDynamicTarget();

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", 0);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RoomTotalValue"))
        {
            currentSubmittedValue = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomTotalValue"];
            UpdateUI();
        }
    }

    // ================== 【难度动态核心机制】 ==================

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateDynamicTarget();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateDynamicTarget();
    }

    private void UpdateDynamicTarget()
    {
        // 1. 根据你选的难度，确定“单人基础金额”
        int baseAmount = (currentDifficulty == GameDifficulty.Normal) ? 10000 : 20000;

        // 2. 如果进了房间，就乘以人数；如果还没进，就默认算 1 个人的
        int playerCount = (PhotonNetwork.CurrentRoom != null) ? PhotonNetwork.CurrentRoom.PlayerCount : 1;

        // 3. 计算最终目标
        currentTargetValue = baseAmount * playerCount;

        Debug.Log($"<color=yellow>【难度动态调整】当前难度: {currentDifficulty}，房间人数: {playerCount}，目标金额更新为: ${currentTargetValue}</color>");

        UpdateUI();
    }

    // ==========================================================

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
            progressText.text = $"Submit Progress:\n<color=#FFD700>${currentSubmittedValue}</color> / ${currentTargetValue}";
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
        if (currentSubmittedValue >= currentTargetValue)
        {
            Debug.Log("<color=green>【任务成功】已达到动态目标金额，飞船启动，成功撤离！</color>");
        }
        else
        {
            Debug.Log("<color=red>【任务失败】金额不足，强行撤离，雇主非常生气，任务失败！</color>");
        }
    }

    public void AddFundsDirectly(int amount)
    {
        if (!PhotonNetwork.InRoom) return;
        int newValue = currentSubmittedValue + amount;
        Hashtable hash = new Hashtable();
        hash.Add("RoomTotalValue", newValue);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        Debug.Log($"<color=cyan>【ATM 提示】刷卡成功！增加了 ${amount}！</color>");
    }

    public bool TrySpendFunds(int amount)
    {
        if (!PhotonNetwork.InRoom) return false;
        if (currentSubmittedValue >= amount)
        {
            int newValue = currentSubmittedValue - amount;
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", newValue);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            Debug.Log($"<color=green>【售货机提示】购买成功！扣除 ${amount}</color>");
            return true;
        }
        else
        {
            Debug.LogWarning("余额不足！快去搬砖！");
            return false;
        }
    }

    public void OpenExtractionUI() { extractionPanel.SetActive(true); }
    public void CloseExtractionUI() { extractionPanel.SetActive(false); }

    void Update()
    {
        if (extractionPanel != null && extractionPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Q)) SubmitItems();
            if (Input.GetKeyDown(KeyCode.X)) TryExtract();
        }
    }
}