using UnityEngine;
using TMPro; // 如果你用的是 TextMeshPro
using UnityEngine.UI;
using Photon.Pun;      // 引入 Photon 核心
using Photon.Realtime; // 引入 Photon 实时状态
using System.Collections;

// 注意这里：从 MonoBehaviour 改成了 MonoBehaviourPunCallbacks！
public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("--- 注册所有 UI 面板 ---")]
    public GameObject MainMenu_UI_Panel;
    public GameObject Login_UI_Panel;
    public GameObject Loading_UI_Panel;      // 新增：Loading 界面
    public GameObject GameOptions_UI_Panel;  // 新增：大厅/房间选项界面

    [Header("--- Loading 界面组件 ---")]
    public Slider loadingProgressBar;        // 进度条
    public TMP_Text loadingStatusText;       // 状态文字
    public TMP_InputField playerNameInput;   // 玩家输入名字的输入框

    void Start()
    {
        // 游戏一开始，只亮主菜单
        ActivatePanel(MainMenu_UI_Panel.name);
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
}