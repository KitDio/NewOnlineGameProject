using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerNameDisplay : MonoBehaviourPun
{
    [Header("UI Reference")]
    public TextMeshProUGUI nameText; // 拖入你刚才做的 NameText
    public GameObject nameCanvas;    // 拖入你的 Name_Canvas，用来控制隐藏

    void Start()
    {
        // 1. 如果是我自己，直接隐藏头顶的 UI，不显示名字
        if (photonView.IsMine)
        {
            if (nameCanvas != null) nameCanvas.SetActive(false);
            return;
        }

        // 2. 如果是其他玩家，读取大厅传过来的名字
        if (nameText != null)
        {
            // 如果大厅同学还没做完，photonView.Owner.NickName 可能是空的
            // 我们做一个后备方案，如果没有名字，就显示 Player + 他的网络 ID
            string playerName = photonView.Owner.NickName;

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Player " + photonView.OwnerActorNr;
            }

            nameText.text = playerName;
        }
    }
}