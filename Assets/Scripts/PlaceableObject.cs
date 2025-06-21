using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceableObject : MonoBehaviour
{
    public GameObject prefab;
    public PlacementManager manager;
    public Button placeButton;

    void Start()
    {
        placeButton.onClick.AddListener(SelectThisObject);
    }

    public void SelectThisObject()
    {
        manager.SetCurrentPlaceable(prefab);
    }
}