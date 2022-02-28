using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField] private GraphicRaycaster raycaster = null;
    private PointerEventData pointerEventData;
    [SerializeField] private EventSystem eventSystem = null;

    public void DropCursorItemSlot()
    {
        DropItem.SpawnDropItem(cursorItemSlot.stack.id, RimecraftWorld.Instance.player.transform.position + RimecraftWorld.Instance.player.transform.forward * 2 + Vector3.up, cursorItemSlot.stack.amount);
        cursorItemSlot.EmptySlot();
    }

    private void Start()
    {
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!IGUIManager.Instance.InUI)
        {
            if (cursorItemSlot.stack != null && cursorItemSlot.stack.amount > 0)
            {
                DropCursorItemSlot();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (cursorItemSlot.stack != null && cursorItemSlot.stack.amount > 0)
            {
                DropCursorItemSlot();
            }
        }

        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UIItemSlot clickedSlot)
    {
        if (clickedSlot == null)
        {
            return;
        }

        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            return;
        }

        if (!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            return;
        }
        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            return;
        }
        if (cursorSlot.HasItem && clickedSlot.HasItem)
        {
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id)
            {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();

                clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                cursorSlot.itemSlot.InsertStack(oldSlot);
            }
            else
            {
                if (cursorSlot.itemSlot.stack.amount + clickedSlot.itemSlot.stack.amount < ItemStack.maxSize)
                {
                    ItemStack resultingStack = new ItemStack(cursorSlot.itemSlot.stack.id, cursorSlot.itemSlot.stack.amount + clickedSlot.itemSlot.stack.amount);
                    clickedSlot.itemSlot.InsertStack(resultingStack);
                    cursorSlot.itemSlot.EmptySlot();
                }
                else
                {
                    if (clickedSlot.itemSlot.stack.amount < ItemStack.maxSize)
                    {
                        int takeAmount = ItemStack.maxSize - clickedSlot.itemSlot.stack.amount;
                        ItemStack resultingStack = new ItemStack(cursorSlot.itemSlot.stack.id, ItemStack.maxSize);
                        clickedSlot.itemSlot.InsertStack(resultingStack);
                        cursorSlot.itemSlot.Take(takeAmount);
                    }
                }
            }
        }
    }

    private UIItemSlot CheckForSlot()
    {
        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("UIItemSlot"))
            {
                return result.gameObject.GetComponent<UIItemSlot>();
            }
        }
        return null;
    }
}