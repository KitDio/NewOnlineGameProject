using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class Y_EnemyNetwork : MonoBehaviourPun, IPunObservable
{
    private PhotonView pv;

    private NavMeshAgent agent;
    private Animator anim;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    void Start()
{
    if (!PhotonNetwork.IsMasterClient)
    {
        agent.enabled = false;
    }
}

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                networkPosition,
                Time.deltaTime * 10f);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                networkRotation,
                Time.deltaTime * 10f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
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
}