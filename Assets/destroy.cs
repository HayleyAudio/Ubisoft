using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class DestroyOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
    }
}