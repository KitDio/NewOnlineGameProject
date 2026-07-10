using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldAudioHelper : MonoBehaviour
{
    private TMP_InputField inputField;

    [Header("--- 打字音效 ---")]
    public AudioSource audioSource;
    public AudioClip typeSound;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        // 用代码动态绑定监听，省去在编辑器里连线的麻烦
        inputField.onValueChanged.AddListener(OnType);
    }

    private void OnType(string currentText)
    {
        // 防御：没有文字、或者刚被清空时不响
        if (string.IsNullOrEmpty(currentText)) return;

        // 细节优化：获取玩家刚刚输入的最后一个字符
        char lastChar = currentText[currentText.Length - 1];

        // 如果最后一个字不是空格，并且组件齐全，就播放清脆的敲击声
        if (lastChar != ' ' && audioSource != null && typeSound != null)
        {
            audioSource.PlayOneShot(typeSound);
        }
    }
}