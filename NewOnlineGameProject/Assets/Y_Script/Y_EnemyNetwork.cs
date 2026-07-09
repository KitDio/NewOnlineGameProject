using Photon.Pun;
using UnityEngine;

public class Y_EnemyNetwork : MonoBehaviourPun, IPunObservable
{
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        Debug.Log("Serialize: " + stream.IsWriting);
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }

    void Start()
    {
        Debug.Log("PhotonView IsMine = " + photonView.IsMine);
        Debug.Log("PhotonView IsSceneView = " + photonView.IsSceneView);
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        transform.position = Vector3.Lerp(
            transform.position,
            networkPosition,
            Time.deltaTime * 10f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            networkRotation,
            Time.deltaTime * 10f);
    }
}