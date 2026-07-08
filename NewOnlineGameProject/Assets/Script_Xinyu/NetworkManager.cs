using UnityEngine;
using TMPro; // 如果你用的是 TextMeshPro
using UnityEngine.UI;
using Photon.Pun;      // 引入 Photon 核心
using Photon.Realtime; // 引入 Photon 实时状态
using System.Collections;
using System.Collections.Generic; // 必须有这个才能用 Dictionary

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("--- 注册所有 UI 面板 ---")]
    public GameObject MainMenu_UI_Panel;
    public GameObject Login_UI_Panel;
    public GameObject Loading_UI_Panel;
    public GameObject GameOptions_UI_Panel;
    public GameObject CreateRoomPanel;
    public GameObject ShowRoomListRoomPanel;
    public GameObject ConnectRandomRoomPanel;
    public GameObject RoomGameModeOptionPanel;

    [Header("--- Loading 界面组件 ---")]
    public Slider loadingProgressBar;        // 进度条
    public TMP_Text loadingStatusText;       // 状态文字
    public TMP_InputField playerNameInput;   // 玩家输入名字的输入框

    [Header("--- Create Room 页面组件 ---")]
    public TMP_InputField roomNameInputField;   // 房间名输入框
    public TMP_InputField maxPlayerInputField;  // 最大人数输入框
    public TMP_Text gameModeDescriptionText;    // 第二页中间的那段描述文字

    // 记录玩家最终选择的难度（默认设为 Normal）
    private string selectedGameMode = "Normal";

    [Header("--- Inside Room 界面组件 ---")]
    public GameObject InsideRoomPanel;
    public TMP_Text roomStatusText;      // 对应你的 RoomStatus (房间名+难度)
    public TMP_Text playerCountText;     // 对应你的 PlayerCount (人数)
    public GameObject playerListItemPrefab;// 你刚做好的玩家单行预制体
    public Transform playerListContent;  // 对应层级里的 PlayerListPanel
    public GameObject startGameButton;   // Start 按钮 (只有房主能点)

    [Header("--- Show Room List 界面组件 ---")]
    public GameObject roomListItemPrefab; // 你的单行房间预制体
    public Transform roomListContent;     // 列表的 Content 节点

    //【核心】：用来缓存真实房间数据的本地字典
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    [Header("--- Random Room 界面组件 ---")]
    public GameObject RandomRoomGameModeOptionPanel; // 让你选难度的面板
    public TMP_Text randomGameModeDescriptionText; // 选难度页面的描述文字
    public Slider connectRandomProgressBar;        // 连接时的进度条
    public TMP_Text joinRandomRoomStatusText;      // 连接时的状态文字

    // 记录玩家在随机匹配界面选中的难度
    private string selectedRandomGameMode = "Normal";

    void Start()
    {
        //Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
        PhotonNetwork.AutomaticallySyncScene = true;

        // 游戏一开始，只亮主菜单
        ActivatePanel(MainMenu_UI_Panel.name);

        selectedGameMode = "Normal";
        if (gameModeDescriptionText != null)
        {
            gameModeDescriptionText.text = "Your team needs to take $10000 values from the planet.\nIt's suitable for beginner level.";
        }
    }

    void Update()
    {
        // 如果 Loading 面板开着，实时把 Photon 的底层状态打印到屏幕上
        if (Loading_UI_Panel.activeSelf)
        {
            loadingStatusText.text = "Status: " + PhotonNetwork.NetworkClientState.ToString();
        }
    }

    //核心大总管函数
    public void ActivatePanel(string panelToBeActivated)
    {
        MainMenu_UI_Panel.SetActive(panelToBeActivated.Equals(MainMenu_UI_Panel.name));
        Login_UI_Panel.SetActive(panelToBeActivated.Equals(Login_UI_Panel.name));
        Loading_UI_Panel.SetActive(panelToBeActivated.Equals(Loading_UI_Panel.name));
        GameOptions_UI_Panel.SetActive(panelToBeActivated.Equals(GameOptions_UI_Panel.name));
        CreateRoomPanel.SetActive(panelToBeActivated.Equals(CreateRoomPanel.name));
        ShowRoomListRoomPanel.SetActive(panelToBeActivated.Equals(ShowRoomListRoomPanel.name));
        ConnectRandomRoomPanel.SetActive(panelToBeActivated.Equals(ConnectRandomRoomPanel.name));
        RoomGameModeOptionPanel.SetActive(panelToBeActivated.Equals(RoomGameModeOptionPanel.name));
        InsideRoomPanel.SetActive(panelToBeActivated.Equals(InsideRoomPanel.name));
        RandomRoomGameModeOptionPanel.SetActive(panelToBeActivated.Equals(RandomRoomGameModeOptionPanel.name));
    }


    // ==========================================
    // UI 按钮绑定的跳转函数
    // ==========================================

    public void OnMainMenuStartButtonClicked()
    {
        ActivatePanel(Login_UI_Panel.name);
    }

    // 当在 Login 面板点击 "Log In" 按钮时触发
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;

        // 检查玩家有没有乱填空名字
        if (!string.IsNullOrEmpty(playerName))
        {
            // 1. 设置玩家名字
            PhotonNetwork.LocalPlayer.NickName = playerName;

            // 2. 切到 Loading 界面
            ActivatePanel(Loading_UI_Panel.name);

            // 3. 启动平滑进度条特效
            StartCoroutine(FakeLoadingBar());

            // 4. 正式向 Photon 服务器发起连接
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogWarning("Player name cannot be empty!");
        }
    }

    public void OnQuitGameButtonClicked()
    {
        Application.Quit();
    }

    // ==========================================
    // Game Options 大厅界面的按钮跳转逻辑
    // ==========================================

    // 1. 点击 Create Room 按钮
    public void OnCreateRoomMenuButtonClicked()
    {
        ActivatePanel(CreateRoomPanel.name);
    }

    // 2. 点击 Show Room List 按钮
    public void OnShowRoomListMenuButtonClicked()
    {
        // 【Photon 核心机制】：只有加入 Lobby 才能获取房间列表
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(ShowRoomListRoomPanel.name);
    }

    // 3. 点击 Join Random Room 按钮
    public void OnJoinRandomRoomMenuButtonClicked()
    {
        ActivatePanel(RandomRoomGameModeOptionPanel.name);
    }

    // ==========================================
    // 第一页：Create Room Panel 逻辑
    // ==========================================

    // 1. 点击第一页的 Cancel 按钮 -> 返回大厅
    public void OnCreateRoomCancelButtonClicked()
    {
        ActivatePanel(GameOptions_UI_Panel.name);
    }

    // ==========================================
    // 随机匹配：难度选择面板逻辑
    // ==========================================

    // 1. 点击 Cancel 按钮 -> 退回大厅
    public void OnRandomGameModeCancelButtonClicked()
    {
        ActivatePanel(GameOptions_UI_Panel.name);
    }

    // 2. 玩家点击 Normal Toggle (动态绑定)
    public void OnRandomNormalModeToggleChanged(bool isOn)
    {
        if (isOn)
        {
            selectedRandomGameMode = "Normal";
            randomGameModeDescriptionText.text = "Your team needs to take $10000 values from the planet.\nIt's suitable for beginner level.";
        }
    }

    // 3. 玩家点击 Difficult Toggle (动态绑定)
    public void OnRandomDifficultModeToggleChanged(bool isOn)
    {
        if (isOn)
        {
            selectedRandomGameMode = "Difficult";
            randomGameModeDescriptionText.text = "Extreme danger! Enemies are relentless.\nTake $20000 values to survive.";
        }
    }

    // 4. 终极按钮：点击 Join 按钮 -> 开始执行带条件的匹配！
    public void OnJoinRandomRoomFinalButtonClicked()
    {
        // 1. 切换到等待 Loading 面板
        ActivatePanel(ConnectRandomRoomPanel.name);

        // 2. 启动假进度条动画
        StartCoroutine(FakeConnectRandomBar());

        // 3. 设置匹配条件：只找难度等于 selectedRandomGameMode 的房间！
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "Difficulty", selectedRandomGameMode }
        };

        // 4. 发起请求（参数 0 表示不管房间最大人数限制）
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    // ==========================================
    // 视觉特效：匹配时的进度条
    // ==========================================
    private IEnumerator FakeConnectRandomBar()
    {
        connectRandomProgressBar.value = 0f;
        joinRandomRoomStatusText.text = "Searching for " + selectedRandomGameMode + " rooms...";
        float fakeProgress = 0f;

        // 让进度条在 3 秒内缓慢涨到 95%
        while (fakeProgress < 0.95f)
        {
            fakeProgress += Time.deltaTime * 0.3f;
            connectRandomProgressBar.value = fakeProgress;
            yield return null;
        }
    }

    // ==========================================
    // Photon 随机匹配专用回调
    // ==========================================

    // 当找不到符合条件的房间时，会自动触发这个函数！
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("找不到匹配的 " + selectedRandomGameMode + " 房间，系统将为你自动创建！");

        joinRandomRoomStatusText.text = "No room found. Creating a new one...";

        // 既然找不到，我们就自己当房主，建一个对应难度的房
        string roomName = "RandomRoom_" + Random.Range(1000, 10000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // 假设最大 4 人

        // 必须把刚才选的难度赋予这个新房间，不然别人匹配不到你
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "Difficulty", selectedRandomGameMode }
        };
        roomOptions.CustomRoomProperties = customRoomProperties;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "Difficulty" };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    // 2. 点击第一页的 Next 按钮 -> 去往第二页
    public void OnCreateRoomNextButtonClicked()
    {
        // 可选：你可以在这里加个判断，如果玩家没填名字就不让进下一页
        ActivatePanel(RoomGameModeOptionPanel.name);
    }

    // ==========================================
    // 第二页：Game Mode Option Panel 逻辑
    // ==========================================

    // 3. 点击第二页的 Cancel 按钮 -> 退回第一页
    public void OnGameModeCancelButtonClicked()
    {
        ActivatePanel(CreateRoomPanel.name);
    }

    // 4. 玩家点击 Normal 按钮
    public void OnNormalModeToggleChanged(bool isOn)
    {
        // 只有当这个选项被“点亮”时，才改变文字和后台数据
        if (isOn)
        {
            selectedGameMode = "Normal";
            gameModeDescriptionText.text = "Your team needs to take $10000 values from the planet.\nIt's suitable for beginner level.";
        }
    }

    // 5. 玩家点击 Difficult 按钮
    public void OnDifficultModeToggleChanged(bool isOn)
    {
        if (isOn)
        {
            selectedGameMode = "Difficult";
            gameModeDescriptionText.text = "Extreme danger! Enemies are relentless.\nTake $20000 values to survive.";
        }
    }

    // 6. 终极按钮：点击 Create 按钮 -> 正式向 Photon 申请建房！
    public void OnFinalCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;

        // 如果玩家没填名字，自动随机生成一个
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "VoidRoom_" + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();

        // 处理最大人数转换防报错
        byte maxPlayers = 4;
        if (byte.TryParse(maxPlayerInputField.text, out byte result))
        {
            maxPlayers = result;
        }
        roomOptions.MaxPlayers = maxPlayers;

        // 【Photon 进阶用法】：把游戏难度作为房间属性存进去，这样后面房间列表才能读取到颜色！
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "Difficulty", selectedGameMode }
        };
        roomOptions.CustomRoomProperties = customRoomProperties;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "Difficulty" }; // 声明这个属性要在大厅广播

        // 正式创建！
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }


    // ==========================================
    // Photon 回调函数（网络状态监听）
    // ==========================================

    // 当成功连接到 Photon Master Server 时，这个函数会自动触发！
    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " 成功连接到服务器！");

        // 确保进度条走满
        loadingProgressBar.value = 1f;

        // 成功连上后，大总管自动切到 Game Options 面板
        ActivatePanel(GameOptions_UI_Panel.name);
    }

    // 如果连接失败或断开，这里会触发
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("连接失败或断开: " + cause.ToString());
        // 被踢掉或者没网，自动弹回登录界面
        ActivatePanel(Login_UI_Panel.name);
    }

    // 当本机玩家成功进入房间时触发
    public override void OnJoinedRoom()
    {
        // 关闭 Loading 面板，正式开启 Inside Room 面板！
        ActivatePanel(InsideRoomPanel.name);

        // 只有房主（MasterClient）才有资格看到/点击 Start 按钮
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);

        // 刷新 UI
        UpdateRoomInfoUI();
        UpdatePlayerListUI();
    }

    // 当有其他新玩家进入我们房间时触发
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomInfoUI();
        UpdatePlayerListUI();
    }

    // 当有玩家离开房间时触发
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomInfoUI();
        UpdatePlayerListUI();

        // 防御机制：如果老房主退了，Photon 会自动把房主权限移交给下一个人
        // 重新检查自己是不是变成了新房主，如果是，就把 Start 按钮显示出来
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    // 补漏：退出房间与开始游戏逻辑
    // 给 InsideRoomPanel 里的 Back 按钮挂载这个函数
    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void OnStartGameButtonClicked()
    {
        // 再次确认：只有房主有资格发车
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Explorer_01");
        }
    }

    // 当玩家成功离开房间后，Photon 会自动触发这个回调
    public override void OnLeftRoom()
    {
        // 离开房间后，系统自动把他踢回大厅面板
        ActivatePanel(GameOptions_UI_Panel.name);
    }
    public void OnLeaveRoomListButtonClicked()
    {
        // 1. 如果当前还在大厅里，就正式退出大厅，停止接收房间刷新广播
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        // 2. 呼叫大总管，把 UI 切回到有三个选项的主大厅面板
        ActivatePanel(GameOptions_UI_Panel.name);
    }

    // ==========================================
    // 视觉特效：虚假的平滑进度条
    // ==========================================
    private IEnumerator FakeLoadingBar()
    {
        loadingProgressBar.value = 0f;
        float fakeProgress = 0f;

        // 让进度条在 2 秒内缓慢涨到 90%，剩下的 10% 等真正连上服务器再填满
        while (fakeProgress < 0.9f)
        {
            fakeProgress += Time.deltaTime * 0.5f;
            loadingProgressBar.value = fakeProgress;
            yield return null;
        }
    }

    // 专门用来刷新房间顶部信息的函数
    private void UpdateRoomInfoUI()
    {
        // 从房间属性中读取难度，如果没有就默认 Normal
        string diff = "Normal";
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Difficulty"))
        {
            diff = (string)PhotonNetwork.CurrentRoom.CustomProperties["Difficulty"];
        }

        roomStatusText.text = PhotonNetwork.CurrentRoom.Name + " (" + diff + ")";
        playerCountText.text = "Player Count/Max Player: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    // 专门用来刷新玩家列表的函数（含 YOU 标识逻辑）
    private void UpdatePlayerListUI()
    {
        // 1. 无情清空旧列表，防止数据叠加
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        // 2. 获取当前房间里的所有真实玩家
        Player[] players = PhotonNetwork.PlayerList;

        // 3. 重新按顺序生成列表
        for (int i = 0; i < players.Length; i++)
        {
            // 实例化单行预制体
            GameObject item = Instantiate(playerListItemPrefab, playerListContent);

            // 填入序号和名字
            item.transform.Find("Number/OrderText").GetComponent<TMP_Text>().text = (i + 1).ToString();
            item.transform.Find("NameText").GetComponent<TMP_Text>().text = players[i].NickName;

            //【核心逻辑】：判断是不是本机玩家！
            GameObject youBadge = item.transform.Find("YouBadge").gameObject;
            // ActorNumber 是 Photon 给每个玩家分配的唯一内部数字 ID
            if (players[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                youBadge.SetActive(true);  // 是自己，点亮 YOU
            }
            else
            {
                youBadge.SetActive(false); // 是别人，隐藏 YOU
            }
        }
    }

    // ==========================================
    // 房间列表核心逻辑
    // ==========================================

    // 【Photon 核心回调】：只要大厅里的房间有任何变化，这个函数就会自动触发！
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 1. 更新本地缓存字典
        UpdateCachedRoomList(roomList);
        // 2. 根据最新的字典重新生成 UI
        UpdateRoomListView();
    }

    // 更新字典的逻辑
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // 如果房间被删除了、关门了、或者隐藏了，就把它从字典里踢出去
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
            }
            else
            {
                // 如果是新房间或者数据有更新，就加进字典或者覆盖旧数据
                cachedRoomList[info.Name] = info;
            }
        }
    }

    // 重新生成 UI 列表的逻辑
    private void UpdateRoomListView()
    {
        // 先无情清空 UI 上的所有旧卡片
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        // 遍历我们本地干净的字典，生成新卡片
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject item = Instantiate(roomListItemPrefab, roomListContent);

            // 注意：这里的 "RoomNameText" 等名字必须和你预制体里的子物体名字一模一样！
            item.transform.Find("RoomNameText").GetComponent<TMP_Text>().text = info.Name;

            // 获取房间难度（如果没有难度属性，默认显示 Normal）
            string diff = "Normal";
            if (info.CustomProperties.ContainsKey("Difficulty"))
            {
                diff = (string)info.CustomProperties["Difficulty"];
            }

            TMP_Text diffText = item.transform.Find("DifficultyText").GetComponent<TMP_Text>();
            diffText.text = diff;

            // 根据难度改个颜色，视觉效果拉满
            if (diff == "Easy") diffText.color = Color.green;
            else if (diff == "Normal") diffText.color = Color.white;
            else if (diff == "Difficult") diffText.color = Color.red;

            // 给这张卡片上的 Join 按钮绑定点击事件！
            Button joinBtn = item.transform.Find("JoinButton").GetComponent<Button>();
            joinBtn.onClick.AddListener(() =>
            {
                JoinSpecificRoom(info.Name);
            });
        }
    }

    // 当玩家点击列表里某一个房间的 Join 按钮时触发
    public void JoinSpecificRoom(string roomName)
    {
        // 告诉 Photon 加入这个指定名字的房间
        PhotonNetwork.JoinRoom(roomName);
    }

    // 当玩家离开大厅（比如退回主界面）时，清空字典防止下次进来数据错乱
    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }
    }
}