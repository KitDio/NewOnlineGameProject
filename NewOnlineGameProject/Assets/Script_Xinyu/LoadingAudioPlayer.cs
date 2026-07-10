using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingAudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip loadingSFX;

    void Awake()
    {
        // 【核心魔法】：告诉 Unity 切换场景的时候不要杀掉我
        DontDestroyOnLoad(gameObject);
    }

    public void PlayLoadingSound()
    {
        if (audioSource != null && loadingSFX != null)
        {
            audioSource.clip = loadingSFX;
            audioSource.loop = false; // 加载音效通常放一遍即可
            audioSource.Play();
        }

        // 监听场景加载完成的事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 只要新场景（游戏关卡）加载成功，就会自动触发这个函数
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 移除监听，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 【优雅退场】：新场景加载好了，把我自己连同声音一起销毁清理掉
        Destroy(gameObject);
    }
}