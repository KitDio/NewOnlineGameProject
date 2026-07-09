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
    public GameDifficulty currentDifficulty = GameDifficulty.Normal;

    private int currentTargetValue = 10000;
    private int currentSubmittedValue = 0;

    [Header("倒计时设置 (秒)")]
    public float roundDuration = 300f;
    private double startTime = -1;
    private bool isTimerRunning = false;
    private bool isGameOver = false;

    [Header("UI 引用")]
    public GameObject extractionPanel;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI timerText;

    void Start()
    {
        if (extractionPanel != null) extractionPanel.SetActive(false);
        currentSubmittedValue = 0;

        // 【核心大修复】主动判断！如果刚加载出来就已经在房间里了（从大厅过来的），直接初始化！
        if (PhotonNetwork.InRoom)
        {
            InitializeRoomSetup();
        }
        else
        {
            // 单机测试模式（没联网直接点 Play）
            startTime = Time.time;
            isTimerRunning = true;
            UpdateDynamicTarget();
            Debug.Log("<color=yellow>【单机测试模式】未连接 Photon 房间，已启用本地离线倒计时！</color>");
        }
    }

    public override void OnJoinedRoom()
    {
        // 如果是联网状态下，直接在当前场景点 Play，依然会走这里
        if (!isTimerRunning)
        {
            InitializeRoomSetup();
        }
    }

    // 【新增】把所有进房间要干的活儿，全塞进这个独立方法里
    private void InitializeRoomSetup()
    {
        UpdateDynamicTarget();

        Hashtable myHash = new Hashtable();
        myHash.Add("InZone", false);
        myHash.Add("MyScore", 0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(myHash);

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", 0);

            // 房主负责定下开局时间
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("StartTime"))
            {
                double currentNetTime = PhotonNetwork.Time;
                hash.Add("StartTime", currentNetTime);

                startTime = currentNetTime;
                isTimerRunning = true;
            }
            else
            {
                // 如果房主掉线重连了，接着以前的时间算
                startTime = (double)PhotonNetwork.CurrentRoom.CustomProperties["StartTime"];
                isTimerRunning = true;
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
        else
        {
            // 队友加入时，读取房间里的属性
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RoomTotalValue"))
                currentSubmittedValue = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomTotalValue"];

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("StartTime"))
            {
                startTime = (double)PhotonNetwork.CurrentRoom.CustomProperties["StartTime"];
                isTimerRunning = true;
            }

            UpdateUI();
        }
    }

    void Update()
    {
        if (extractionPanel != null && extractionPanel.activeSelf && !isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Q)) SubmitItems();
            if (Input.GetKeyDown(KeyCode.X)) TryExtract();
        }

        if (isTimerRunning && !isGameOver)
        {
            double currentTime = PhotonNetwork.InRoom ? PhotonNetwork.Time : (double)Time.time;

            double timePassed = currentTime - startTime;
            float timeLeft = roundDuration - (float)timePassed;

            if (timeLeft <= 0)
            {
                timeLeft = 0;
                TimeUpFailed();
            }

            UpdateTimerUI(timeLeft);
        }
    }

    private void UpdateTimerUI(float timeLeft)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);

            string colorHex = timeLeft <= 60f ? "#FF0000" : "#FFFFFF";
            timerText.text = $"<color={colorHex}>{minutes:00}:{seconds:00}</color>";
        }
    }

    private void TimeUpFailed()
    {
        isGameOver = true;
        isTimerRunning = false;

        Debug.Log("<color=red>【任务失败】时间耗尽！</color>");
        if (timerText != null) timerText.text = "<color=red>00:00</color>";

        // 【新增】呼叫结算界面：失败
        if (GameResultUIManager.Instance != null)
        {
            GameResultUIManager.Instance.TriggerGameEnd(false);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) { UpdateDynamicTarget(); }
    public override void OnPlayerLeftRoom(Player otherPlayer) { UpdateDynamicTarget(); }

    private void UpdateDynamicTarget()
    {
        int baseAmount = (currentDifficulty == GameDifficulty.Normal) ? 10000 : 20000;
        int playerCount = (PhotonNetwork.CurrentRoom != null) ? PhotonNetwork.CurrentRoom.PlayerCount : 1;
        currentTargetValue = baseAmount * playerCount;

        UpdateUI();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("RoomTotalValue"))
        {
            currentSubmittedValue = (int)propertiesThatChanged["RoomTotalValue"];
            UpdateUI();
        }

        if (propertiesThatChanged.ContainsKey("StartTime"))
        {
            startTime = (double)propertiesThatChanged["StartTime"];
            isTimerRunning = true;
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
        if (isGameOver) return;

        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory == null) return;

        int valueToSubmit = inventory.GetTotalValue();
        if (valueToSubmit > 0)
        {
            // 1. 给房间总进度加钱
            int newValue = currentSubmittedValue + valueToSubmit;
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", newValue);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            // 2. 【新增】给自己的个人贡献分加钱！
            int myCurrentScore = 0;
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("MyScore"))
            {
                myCurrentScore = (int)PhotonNetwork.LocalPlayer.CustomProperties["MyScore"];
            }
            Hashtable myHash = new Hashtable();
            myHash.Add("MyScore", myCurrentScore + valueToSubmit);
            PhotonNetwork.LocalPlayer.SetCustomProperties(myHash);

            inventory.ClearAllItems();
        }
        else
        {
            Debug.LogWarning("你的包里没东西，或者东西一文不值！");
        }
    }

    public void TryExtract()
    {
        if (isGameOver) return;

        if (currentSubmittedValue >= currentTargetValue)
        {
            if (AreAllPlayersInZone())
            {
                photonView.RPC(nameof(RpcExtractionSuccess), RpcTarget.All);
            }
            else
            {
                Debug.Log("<color=orange>【撤离等待】资金已达标，但还有队员未进入撤离区！等全员到齐后才能撤离！</color>");
            }
        }
        else
        {
            Debug.Log("<color=red>【任务失败】金额不足，强行撤离，雇主非常生气，任务失败！</color>");
        }
    }

    [PunRPC]
    public void RpcExtractionSuccess()
    {
        isGameOver = true;
        isTimerRunning = false;

        Debug.Log("<color=green>【全网广播】任务成功！飞船启动，全员成功撤离！</color>");

        // 【新增】呼叫结算界面：胜利
        if (GameResultUIManager.Instance != null)
        {
            GameResultUIManager.Instance.TriggerGameEnd(true);
        }
    }

    private bool AreAllPlayersInZone()
    {
        if (!PhotonNetwork.InRoom) return true;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.TryGetValue("InZone", out object inZoneObj))
            {
                if (!(bool)inZoneObj) return false;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public void AddFundsDirectly(int amount)
    {
        if (!PhotonNetwork.InRoom || isGameOver) return;
        int newValue = currentSubmittedValue + amount;
        Hashtable hash = new Hashtable();
        hash.Add("RoomTotalValue", newValue);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    public bool TrySpendFunds(int amount)
    {
        if (!PhotonNetwork.InRoom || isGameOver) return false;
        if (currentSubmittedValue >= amount)
        {
            int newValue = currentSubmittedValue - amount;
            Hashtable hash = new Hashtable();
            hash.Add("RoomTotalValue", newValue);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            return true;
        }
        return false;
    }

    public void OpenExtractionUI() { if (!isGameOver) extractionPanel.SetActive(true); }
    public void CloseExtractionUI() { extractionPanel.SetActive(false); }
}