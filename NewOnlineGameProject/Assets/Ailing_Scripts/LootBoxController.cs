using UnityEngine;
using Photon.Pun;
using System.Collections; // 用于协程（倒计时延迟）

public class LootBoxController : MonoBehaviourPun
{
    [Header("盲盒组件")]
    public Rigidbody lidRigidbody;
    public Transform spawnPoint; 

    [Header("物理效果")]
    public float popForce = 5f;

    [Header("奖池 (填入 Resources 文件夹里的预制体名字)")]
    public string[] possibleItems;

    [Header("音效设置 (SFX)")]
    public AudioSource audioSource; 
    public AudioClip openSound; 

    private bool isOpened = false; 

    public void RequestOpen()
    {
        if (isOpened) return;
        photonView.RPC("RPC_TryOpen", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_TryOpen()
    {
        if (isOpened) return;
        isOpened = true;

        photonView.RPC("RPC_ConfirmOpen", RpcTarget.All);
    }

    [PunRPC]
    void RPC_ConfirmOpen()
    {
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        if (lidRigidbody != null)
        {
            lidRigidbody.isKinematic = false; 
            lidRigidbody.AddForce((Vector3.up + transform.forward) * popForce, ForceMode.Impulse);
            lidRigidbody.AddTorque(new Vector3(10f, 0, 10f), ForceMode.Impulse);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnItemAndDestroyBox());
        }
    }

    IEnumerator SpawnItemAndDestroyBox()
    {
        if (possibleItems.Length > 0 && spawnPoint != null)
        {
            int randomIndex = Random.Range(0, possibleItems.Length);
            string itemToSpawn = possibleItems[randomIndex];

            PhotonNetwork.Instantiate(itemToSpawn, spawnPoint.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(3f);

        PhotonNetwork.Destroy(gameObject);
    }
}