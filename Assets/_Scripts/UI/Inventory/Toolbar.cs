using UnityEngine;

[System.Serializable]
public class Toolbar : MonoBehaviour
{
    public InventoryRow inventory;
    [SerializeField] private RectTransform highlight;
    [SerializeField] private AudioManager playerSounds;
    public int slotIndex = 0;

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool updateHighlight = false;
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                slotIndex--;
            }
            else
            {
                slotIndex++;
            }

            if (slotIndex > inventory.slots.Length - 1)
            {
                slotIndex = 0;
            }

            if (slotIndex < 0)
            {
                slotIndex = inventory.slots.Length - 1;
            }
            updateHighlight = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            slotIndex = 0;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            slotIndex = 1;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            slotIndex = 2;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            slotIndex = 3;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            slotIndex = 4;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            slotIndex = 5;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            slotIndex = 6;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            slotIndex = 7;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            slotIndex = 8;
            updateHighlight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            slotIndex = 9;
            updateHighlight = true;
        }
        if (updateHighlight)
        {
            playerSounds.Play("ToolbarSwap");
            highlight.position = inventory.slots[slotIndex].slotIcon.transform.position;
            updateHighlight = false;
        }
    }
}