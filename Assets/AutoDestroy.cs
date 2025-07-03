using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifeTime = 10f;   // some após 10 s

    void Start() => Destroy(gameObject, lifeTime);

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            Destroy(gameObject);
    }
}
