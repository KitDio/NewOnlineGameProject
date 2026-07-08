using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    // 静态锁：防止 A传B，B瞬间传回A，造成无限死循环传送！
    private static bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        // 确保是主角，并且当前没有在传送中
        if (other.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportSequence(other.gameObject));
        }
    }

    private IEnumerator TeleportSequence(GameObject player)
    {
        // 上锁，传送过程开始
        isTeleporting = true;

        // ================= 1. 屏幕逐渐变黑 =================
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration); // 透明度从 0 到 1
            fadeImage.color = color;
            yield return null; // 等待下一帧
        }
        color.a = 1f;
        fadeImage.color = color;


        // ================= 2. 核心传送逻辑 =================
        // 【破解 Starter Assets 大坑】：必须先关闭 CharacterController，否则无法传送！
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 瞬间移动玩家坐标和旋转角度
        player.transform.position = destinationNode.position;
        player.transform.rotation = destinationNode.rotation;

        // 重新开启 CharacterController
        if (cc != null) cc.enabled = true;

        // 稍微等 0.2 秒，让物理引擎缓一口气，防止掉进地底虚空
        yield return new WaitForSeconds(0.2f);


        // ================= 3. 屏幕逐渐恢复清晰 =================
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration); // 透明度从 1 到 0
            fadeImage.color = color;
            yield return null;
        }
        color.a = 0f;
        fadeImage.color = color;

        // 解锁，可以进行下一次传送了
        isTeleporting = false;
    }
}