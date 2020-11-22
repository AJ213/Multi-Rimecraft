﻿using UnityEngine;

public class Toolbar : MonoBehaviour
{
    public UIItemSlot[] slots;
    public RectTransform highlight;
    public Player player;
    public int slotIndex = 0;

    private void Start()
    {
        ushort index = 1;
        foreach (UIItemSlot s in slots)
        {
            ItemStack stack = new ItemStack(6, Random.Range(2, 65));
            ItemSlot slot = new ItemSlot(s, stack);
            index++;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            player.playerSounds.Play("ToolbarSwap");
            if (scroll > 0)
            {
                slotIndex--;
            }
            else
            {
                slotIndex++;
            }

            if (slotIndex > slots.Length - 1)
            {
                slotIndex = 0;
            }

            if (slotIndex < 0)
            {
                slotIndex = slots.Length - 1;
            }

            highlight.position = slots[slotIndex].slotIcon.transform.position;
        }
    }
}