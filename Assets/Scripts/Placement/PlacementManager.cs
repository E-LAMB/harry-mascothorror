using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlacementManager : MonoBehaviour
{
    public GameObject currentObjectToPlace;
    public GameObject menuParent;
    public PlayerMovement player;
    public ObjectPlacer placer;
    public Button backButton;
    private bool inMenu = false;

    void Start()
    {
        CloseMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (inMenu)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        backButton.onClick.AddListener(CloseMenu);
    }

    public void SetCurrentPlaceable(GameObject newPrefab)
    {
        currentObjectToPlace = newPrefab;
        CloseMenu();
        placer.UpdatePreview();
    }

    public void CloseMenu()
    {
        menuParent.SetActive(false);
        inMenu = false;
        player.canMove = true;
    }

    public void OpenMenu()
    {
        currentObjectToPlace = null;
        Destroy(placer.previewInstance);
        menuParent.SetActive(true);
        inMenu = true;
        player.canMove = false;
    }
}