using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        Vector3 vector = target.position;
        vector.y = transform.position.y;

        transform.position = vector;
    }
}
