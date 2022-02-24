using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// In Game UI Manager
public class IGUIManager : MonoBehaviourSingleton<IGUIManager>
{
    [SerializeField] private GameObject debugScreen;
    [SerializeField] private Inventory inventory;
    public Inventory GetInventory => inventory;

    private bool inUI = false;
    public bool InUI
    {
        get => inUI;
        set
        {
            inUI = value;
            if (inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            InUI = !InUI;
            inventory.storage.SetActive(InUI);
        }
    }
}