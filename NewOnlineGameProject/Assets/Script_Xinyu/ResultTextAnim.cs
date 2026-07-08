using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class ResultTextAnim : MonoBehaviour
{
    private TMP_Text uiText;
    private string originalText; // 用来存你原本在面板里写的字

    [Header("--- 测试设置 ---")]
    public bool hideOnStart = true; // 勾选后，游戏一开始文字会先隐藏

    void Awake()
    {
        uiText = GetComponent<TMP_Text>();
        originalText = uiText.text; // 记住你原本写的比如 "EXTRACTION SUCCESSFUL"

        if (hideOnStart)
        {
            uiText.text = ""; // 清空文字
            transform.localScale = Vector3.zero; // 把大小缩成0
        }
    }

    // ==========================================
    // 黑魔法：在 Unity 面板里右键直接运行！
    // ==========================================
    [ContextMenu("测试：打字机效果 (Typewriter)")]
    public void TestTypewriter()
    {
        // 强行恢复大小，防止之前被缩放成0了
        transform.localScale = Vector3.one; 
        PlayTypewriterEffect(originalText, 0.05f);
    }

    [ContextMenu("测试：盖章弹出效果 (Pop Up)")]
    public void TestPopUp()
    {
        // 强行恢复文字，防止之前被清空了
        uiText.text = originalText; 
        PlayPopUpEffect(0.5f);
    }


    // ==========================================
    // 下面是你刚才的实际功能代码（保持不变）
    // ==========================================
    public void PlayTypewriterEffect(string content, float delayBetweenChars = 0.05f)
    {
        StopAllCoroutines(); // 打断正在播放的动画
        StartCoroutine(TypewriterCoroutine(content, delayBetweenChars));
    }

    private IEnumerator TypewriterCoroutine(string content, float delay)
    {
        uiText.text = "";
        foreach (char c in content)
        {
            uiText.text += c;
            yield return new WaitForSeconds(delay);
        }
    }

    public void PlayPopUpEffect(float duration = 0.5f)
    {
        StopAllCoroutines();
        StartCoroutine(PopUpCoroutine(duration));
    }

    private IEnumerator PopUpCoroutine(float duration)
    {
        transform.localScale = Vector3.zero;
        float timer = 0f;
        
        float overShootTime = duration * 0.7f;
        while (timer < overShootTime)
        {
            timer += Time.deltaTime;
            float progress = timer / overShootTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, progress);
            yield return null;
        }

        timer = 0f;
        float settleTime = duration - overShootTime;
        while (timer < settleTime)
        {
            timer += Time.deltaTime;
            float progress = timer / settleTime;
            transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, progress);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}