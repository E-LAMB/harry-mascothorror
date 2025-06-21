using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public float gridSize = 1f;
    public Vector3 gridOffset = Vector3.zero;

    public PlacementManager manager;
    public LayerMask placementLayer;
    public LayerMask placedObjectsLayer;
    public Vector3 checkBoxSize = new Vector3(0.45f, 0.45f, 0.45f);

    private Quaternion currentRotation = Quaternion.identity;
    public float rotationStep = 90f;

    public Material previewMaterialValid;
    public Material previewMaterialInvalid;

    private GameObject previewInstance;
    private Renderer[] previewRenderers;

    void Update()
    {
        if (manager.currentObjectToPlace != null)
        {
            if (previewRenderers != null)
            {
                ApplyPreviewMaterial(previewMaterialValid);
            }

            if (previewInstance != null)
            { 
                UpdateRays(); 
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceObject();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                currentRotation *= Quaternion.Euler(0f, -rotationStep, 0f);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                currentRotation *= Quaternion.Euler(0f, rotationStep, 0f);
            }
        }
    }

    public void UpdatePreview()
    {
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
        }

        previewInstance = Instantiate(manager.currentObjectToPlace);
        DestroyImmediate(previewInstance.GetComponent<Collider>());
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();
    }

    void UpdateRays()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, placementLayer))
        {
            Vector3 alignedPosition = GetSnappedPosition(hit.point);
            previewInstance.transform.position = alignedPosition;
            previewInstance.transform.rotation = currentRotation;

            bool blocked = IsPositionOccupied(alignedPosition);
            ApplyPreviewMaterial(blocked ? previewMaterialInvalid : previewMaterialValid);
        }
    }

    void TryPlaceObject()
    {
        Vector3 placePos = previewInstance.transform.position;

        if (!IsPositionOccupied(placePos))
        {
            GameObject placed = Instantiate(manager.currentObjectToPlace, placePos, currentRotation);
            SetLayerRecursively(placed, LayerMaskToLayer(placedObjectsLayer));
        }
        else
        {
            UnityEngine.Debug.Log("Blocked: object already exists here.");
        }
    }

    Vector3 GetSnappedPosition(Vector3 worldPosition)
    {
        float x = Mathf.Round((worldPosition.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
        float y = Mathf.Round((worldPosition.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;
        float z = Mathf.Round((worldPosition.z - gridOffset.z) / gridSize) * gridSize + gridOffset.z;

        return new Vector3(
            Mathf.Round(x * 1000f) / 1000f,
            Mathf.Round(y * 1000f) / 1000f,
            Mathf.Round(z * 1000f) / 1000f
        );
    }

    bool IsPositionOccupied(Vector3 position)
    {
        Collider[] hits = Physics.OverlapBox(position, checkBoxSize, Quaternion.identity, placedObjectsLayer);
        return hits.Length > 0;
    }

    void ApplyPreviewMaterial(Material mat)
    {
        foreach (Renderer rend in previewRenderers)
        {
            rend.material = mat;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (previewInstance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(previewInstance.transform.position, checkBoxSize * 2);
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    int LayerMaskToLayer(LayerMask mask)
    {
        int layer = 0;
        int layerValue = mask.value;
        while (layerValue > 1)
        {
            layerValue = layerValue >> 1;
            layer++;
        }
        return layer;
    }
}
