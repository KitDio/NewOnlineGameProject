using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameResultUIManager : MonoBehaviour
{
    public static GameResultUIManager Instance;

    [Header("过渡面板")]
    public GameObject successfulPanel;
    public GameObject failedPanel;

    [Header("--- 动态文字动画引用 ---")]
    [Tooltip("分别把你挂了 ResultTextAnim 脚本的 Title 和 Detail 文字拖进来")]
    public ResultTextAnim successTitleAnim;
    public ResultTextAnim successDetailAnim;
    public ResultTextAnim failTitleAnim;
    public ResultTextAnim failDetailAnim;

    [Header("排行榜面板")]
    public GameObject successfulLeaderboardPanel;
    public GameObject failedLeaderboardPanel;

    [Header("生成设置")]
    public Transform successfulContentArea; // SuccessfulLeaderboardPanel 下的 LeaderboardContent
    public Transform failedContentArea;     // FailedLeaderboardPanel 下的 LeaderboardContent
    public GameObject playerItemPrefab;     // 刚才做好的 PlayerLeaderboardItem 预制体
    public GameObject playerFailedItemPrefab;

    [Header("时间设置")]
    public float panelShowDuration = 4f;    // 胜利/失败界面展示几秒后跳排行榜

    void Awake()
    {
        Instance = this;
        HideAllPanels();
    }

    private void HideAllPanels()
    {
        if (successfulPanel) successfulPanel.SetActive(false);
        if (failedPanel) failedPanel.SetActive(false);
        if (successfulLeaderboardPanel) successfulLeaderboardPanel.SetActive(false);
        if (failedLeaderboardPanel) failedLeaderboardPanel.SetActive(false);
    }

    // 供外界调用的最终接口
    public void TriggerGameEnd(bool isVictory)
    {
        // 游戏结束，把鼠标显示出来，不然玩家没法点“返回房间”按钮
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(ResultFlowRoutine(isVictory));
    }

    private IEnumerator ResultFlowRoutine(bool isVictory)
    {
        HideAllPanels();

        // 1. 弹出 成功/失败 提示大字，并同时触发你的 UI 动效！
        if (isVictory)
        {
            successfulPanel.SetActive(true);

            // 呼叫你的动效脚本
            if (successTitleAnim != null) successTitleAnim.PlayPopUpEffect(0.5f);
            if (successDetailAnim != null) successDetailAnim.PlayTypewriterEffect("Your team has achieved the goal!", 0.05f);
        }
        else
        {
            failedPanel.SetActive(true);

            // 呼叫你的动效脚本，使用极简战术风文本
            if (failTitleAnim != null) failTitleAnim.PlayPopUpEffect(0.5f);
            if (failDetailAnim != null) failDetailAnim.PlayTypewriterEffect("STATUS: M.I.A.\nAll carried equipment and assets forfeit.\nSignal Lost.", 0.05f);
        }

        // 2. 等待面板展示（打字机在同时工作）
        yield return new WaitForSeconds(panelShowDuration);

        // 3. 隐藏提示大字，弹出对应的排行榜
        if (isVictory)
        {
            successfulPanel.SetActive(false);
            successfulLeaderboardPanel.SetActive(true);
            GenerateLeaderboard(successfulContentArea, playerItemPrefab);
        }
        else
        {
            failedPanel.SetActive(false);
            failedLeaderboardPanel.SetActive(true);
            GenerateLeaderboard(failedContentArea, playerFailedItemPrefab);
        }
    }

    private void GenerateLeaderboard(Transform contentArea, GameObject prefabToUse)
    {
        // 先清空原本 Content 里可能残留的东西
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        // 获取房间里的所有玩家，并转换成 List
        List<Player> playerList = PhotonNetwork.PlayerList.ToList();

        // 核心：根据每个人的 "MyScore" 从大到小排序 (降序)
        playerList.Sort((a, b) =>
        {
            int scoreA = a.CustomProperties.ContainsKey("MyScore") ? (int)a.CustomProperties["MyScore"] : 0;
            int scoreB = b.CustomProperties.ContainsKey("MyScore") ? (int)b.CustomProperties["MyScore"] : 0;
            return scoreB.CompareTo(scoreA);
        });

        // 遍历排序好的玩家，生成 UI
        for (int i = 0; i < playerList.Count; i++)
        {
            Player p = playerList[i];
            int score = p.CustomProperties.ContainsKey("MyScore") ? (int)p.CustomProperties["MyScore"] : 0;
            int rank = i + 1;
            bool isMVP = (rank == 1); // 第一名就是 MVP！

            // 实例化预制体，放到 Content 下面
            GameObject itemGo = Instantiate(prefabToUse, contentArea);
            LeaderboardItemUI itemUI = itemGo.GetComponent<LeaderboardItemUI>();

            if (itemUI != null)
            {
                // 假如没起名字，就用 Player 1, Player 2 替代
                string pName = string.IsNullOrEmpty(p.NickName) ? $"Player {p.ActorNumber}" : p.NickName;
                itemUI.Setup(rank, pName, score, isMVP);
            }
        }
    }
    // 1. 点击“回到房间”按钮
    public void OnBackToRoomClicked()
    {
        // 通常来说，“带队返回大厅”是房主的特权。
        // （前提是你在大厅连接服务器时，设置了 PhotonNetwork.automaticallySyncScene = true）
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("房主带队返回房间...");
            PhotonNetwork.LoadLevel("LobbyScene"); // 【注意】这里必须填你大厅场景的准确名字！
        }
        else
        {
            Debug.Log("只有房主可以带领队伍返回房间！请等待房主操作...");
            // （可选）你可以让这个按钮对非房主隐藏，或者弹个 UI 提示他们“等待房主...”
        }
    }

    // 2. 点击“回到主菜单”按钮
    public void OnBackToMenuClicked()
    {
        Debug.Log("正在离开房间...");
        // 核心：调用离开房间指令
        PhotonNetwork.LeaveRoom();
    }

}
