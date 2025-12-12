using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutUpObject : MonoBehaviour, IHandsUp
{
    private Rigidbody rb;
    public float handDownForward;
    [Header("Put Up vector3")]
    public Vector3 putupPos;
    public Vector3 putUpRot;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void InHandTransform()
    {
        transform.localPosition = putupPos;
        transform.localEulerAngles = putUpRot;
        rb.isKinematic = true;
    }

    public void HandDownTransform()
    {
        GameObject player = transform.root.gameObject;
        transform.parent = null;
        rb.isKinematic = false;

        Vector3 vector = transform.eulerAngles;
        vector.x = 0;
        vector.z = 0;
        transform.eulerAngles = vector;

        transform.position = player.transform.position + player.transform.forward * handDownForward;

        // 플레이어 앞으로 약간 이동시킴 (겹침 방지)

        if ((rb.constraints & RigidbodyConstraints.FreezePositionY) != 0)
            rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
    }

    public void Throw(GameObject player, float power)
    {
        HandDownTransform();
        rb.AddForce(player.transform.forward * power);
    }
}
