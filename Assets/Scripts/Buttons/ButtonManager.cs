using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public List<PhysicalButton> buttons = new List<PhysicalButton>();

    private int pressedCount = 0;
    public GameObject winScreen;

    void Start()
    {
        winScreen.SetActive(false);
    }

    public void ButtonPressed()
    {
        pressedCount++;

        Debug.Log($"Button pressed! {pressedCount}/{buttons.Count}");

        if (pressedCount == buttons.Count)
        {
            AllButtonsPressed();
        }
    }

    void AllButtonsPressed()
    {
        Debug.Log("All buttons have been pressed!");
        winScreen.SetActive(true);
    }
}

