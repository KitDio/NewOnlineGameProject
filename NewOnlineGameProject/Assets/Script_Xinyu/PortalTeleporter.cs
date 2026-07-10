using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PortalTeleporter : MonoBehaviour
{
    [Header("--- 目标传送点 ---")]
    [Tooltip("把另一个传送门前面的空物体拖到这里")]
    public Transform destinationNode;

    [Header("--- 视觉效果 ---")]
    [Tooltip("把你刚才做的黑屏 UI Image 拖到这里")]
    public Image fadeImage;
    [Tooltip("渐黑/渐亮的时间（秒）")]
    public float fadeDuration = 1f;

    [Header("--- 传送音效设置 ---")]
    public AudioSource portalAudioSource;
    public AudioClip teleportSFX;

    private static bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pView = other.GetComponent<PhotonView>();

            // 【联网核心】：只有我自己，并且当前没在传送中，才触发传送协程！
            if (pView != null && pView.IsMine && !isTeleporting)
            {
                // 启动你写好的黑屏传送动画！
                StartCoroutine(TeleportSequence(other.gameObject));
            }
        }
    }

    private IEnumerator TeleportSequence(GameObject player)
    {
        isTeleporting = true;

        if (portalAudioSource != null && teleportSFX != null)
        {
            // 使用 PlayOneShot 可以确保音效在玩家传走后，依然能在原位置完整播完
            portalAudioSource.PlayOneShot(teleportSFX);
        }

        // ================= 1. 屏幕逐渐变黑 =================
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1f;
        fadeImage.color = color;

        // ================= 2. 核心传送逻辑 =================
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = destinationNode.position;
        player.transform.rotation = destinationNode.rotation;

        if (cc != null) cc.enabled = true;

        yield return new WaitForSeconds(0.2f);

        // ================= 3. 屏幕逐渐恢复清晰 =================
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 0f;
        fadeImage.color = color;

        isTeleporting = false;
    }
}