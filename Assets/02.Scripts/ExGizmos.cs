using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExGizmos : MonoBehaviour
{
    public Color color;
    public float size;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, size);
    }
}
