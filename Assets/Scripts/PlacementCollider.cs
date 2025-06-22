using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementCollider : MonoBehaviour
{
    public bool isBlocked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Preview"))
        {
            isBlocked = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Preview"))
        {
            isBlocked = false;
        }
    }
}
