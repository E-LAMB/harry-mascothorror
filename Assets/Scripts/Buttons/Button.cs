using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalButton : MonoBehaviour
{
    public bool isPressed = false;
    private ButtonManager manager;
    public float interactRange = 5f;
    public LayerMask blockingLayer;
    public Camera playerCamera;

    void Start()
    {
        manager = FindObjectOfType<ButtonManager>();
    }

    void Update()
    {
        if (isPressed) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryInteractWithThisButton();
        }
    }

    void TryInteractWithThisButton()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, blockingLayer))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Press();
            }
        }
    }

    public void Press()
    {
        if (isPressed) return;

        isPressed = true;
        transform.localPosition += new Vector3(0, -0.1f, 0);
        manager.ButtonPressed();
    }
}
