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

    [Header("音效与音乐设置 (Audio)")]
    public AudioSource bgmAudioSource;      // 场景里正在播放全局BGM的喇叭
    public AudioSource jingleAudioSource;   // 专门用来播放胜利/失败音乐的喇叭
    public AudioClip victoryJingle;         // 胜利结算音乐
    public AudioClip defeatJingle;          // 失败结算音乐

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

    public void TriggerGameEnd(bool isVictory)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(ResultFlowRoutine(isVictory));
    }

    private IEnumerator ResultFlowRoutine(bool isVictory)
    {
        HideAllPanels();

        // --- 【核心修改：音效控制】 ---
        // 1. 暂停全局 BGM (使用 Pause 而不是 Stop，这样能接上进度)
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
        }

        // 2. 播放对应的结算音乐，并获取它的时长
        float jingleDuration = 0f;
        if (jingleAudioSource != null)
        {
            AudioClip clipToPlay = isVictory ? victoryJingle : defeatJingle;
            if (clipToPlay != null)
            {
                jingleAudioSource.PlayOneShot(clipToPlay);
                jingleDuration = clipToPlay.length; // 获取这个音效总共有多少秒
            }
        }

        // 3. 启动一个独立的协程，等结算音乐播完后恢复 BGM
        if (bgmAudioSource != null && jingleDuration > 0f)
        {
            StartCoroutine(ResumeBGMRoutine(jingleDuration));
        }
        // ------------------------------

        // 1. 弹出 成功/失败 提示大字，并同时触发你的 UI 动效！
        if (isVictory)
        {
            successfulPanel.SetActive(true);
            if (successTitleAnim != null) successTitleAnim.PlayPopUpEffect(0.5f);
            if (successDetailAnim != null) successDetailAnim.PlayTypewriterEffect("Your team has achieved the goal!", 0.05f);
        }
        else
        {
            failedPanel.SetActive(true);
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

    // 【新增】专门用来等待并恢复 BGM 的协程
    private IEnumerator ResumeBGMRoutine(float delayTime)
    {
        // 等待结算音乐播放完毕
        yield return new WaitForSeconds(delayTime);

        // 恢复播放 BGM
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Play(); // Play 会接着之前 Pause 的地方继续播
        }
    }

    private void GenerateLeaderboard(Transform contentArea, GameObject prefabToUse)
    {
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        List<Player> playerList = PhotonNetwork.PlayerList.ToList();

        playerList.Sort((a, b) =>
        {
            int scoreA = a.CustomProperties.ContainsKey("MyScore") ? (int)a.CustomProperties["MyScore"] : 0;
            int scoreB = b.CustomProperties.ContainsKey("MyScore") ? (int)b.CustomProperties["MyScore"] : 0;
            return scoreB.CompareTo(scoreA);
        });

        for (int i = 0; i < playerList.Count; i++)
        {
            Player p = playerList[i];
            int score = p.CustomProperties.ContainsKey("MyScore") ? (int)p.CustomProperties["MyScore"] : 0;
            int rank = i + 1;
            bool isMVP = (rank == 1);

            GameObject itemGo = Instantiate(prefabToUse, contentArea);
            LeaderboardItemUI itemUI = itemGo.GetComponent<LeaderboardItemUI>();

            if (itemUI != null)
            {
                string pName = string.IsNullOrEmpty(p.NickName) ? $"Player {p.ActorNumber}" : p.NickName;
                itemUI.Setup(rank, pName, score, isMVP);
            }
        }
    }

    public void OnBackToRoomClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("房主带队返回房间...");
            PhotonNetwork.LoadLevel("LobbyScene");
        }
        else
        {
            Debug.Log("只有房主可以带领队伍返回房间！请等待房主操作...");
        }
    }

    public void OnBackToMenuClicked()
    {
        Debug.Log("正在离开房间...");
        PhotonNetwork.LeaveRoom();
    }
}