using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Placing Blocks
    [SerializeField] private float checkIncrement = 0.01f;
    [SerializeField] private float reach = 8;
    [SerializeField] private Transform highlightBlock = null;
    [SerializeField] private Vector3 placeBlockPosition;
    [SerializeField] private Light toolLight;
    // Jumping and falling
    private ElipsoidRigidbody rbody;

    [SerializeField] private float footstepInterval = 0.4f;
    [SerializeField] private float footstepIntervalCrouching = 0.5f;
    [SerializeField] private float footstepIntervalSprinting = 0.25f;

    [SerializeField] private float jumpForce = 5;
    private bool jumpRequest;
    [SerializeField] private AudioManager playerSounds;

    // Moving
    [SerializeField] private bool isSprinting = false;
    [SerializeField] private bool isCrouching = false;

    [SerializeField] private float sprintSpeed = 6;
    [SerializeField] private float walkSpeed = 3;
    [SerializeField] private float crouchSpeed = 2;
    private float horizontal;
    private float vertical;
    private float mouseY;

    [SerializeField] private SceneChanger sceneChanger = default;

    // Misc
    private float heldDownTime = 0;

    private int curBounceCount;
    private float curSpeed;
    private Transform cam;

    private void FixedUpdate()
    {
        if (Client.Instance.IsConnected)
        {
            ClientSend.PlayerMovement();
        }

        if (!IGUIManager.Instance.InUI)
        {
            if (jumpRequest)
            {
                Jump();
            }
            if (isSprinting)
            {
                rbody.CalculateVelocity(horizontal, vertical, sprintSpeed);
            }
            else if (isCrouching)
            {
                rbody.CalculateVelocity(horizontal, vertical, crouchSpeed, true);
            }
            else
            {
                rbody.CalculateVelocity(horizontal, vertical, walkSpeed);
            }
        }
    }

    private void GetPlayerInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Client.Instance.DisconnectFromServer();
            sceneChanger.FadeToScene(0);
        }

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
            isCrouching = false;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }
        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = true;
            isSprinting = false;
        }
        if (Input.GetButtonUp("Crouch"))
        {
            isCrouching = false;
        }

        if (rbody.IsGrounded && Input.GetButton("Jump"))
        {
            jumpRequest = true;
        }

        if (rbody.IsGrounded && (Mathf.Abs(horizontal) >= 0.1f || Mathf.Abs(vertical) >= 0.1f))
        {
            float footstepSpeed;
            if (isSprinting)
            {
                footstepSpeed = footstepIntervalSprinting;
            }
            else if (isCrouching)
            {
                footstepSpeed = footstepIntervalCrouching;
            }
            else
            {
                footstepSpeed = footstepInterval;
            }
            this.GetComponent<FootstepsPlayer>().PlayFootstep(footstepSpeed);
        }

        DestroyBlock();

        if (highlightBlock.gameObject.activeSelf)
        {
            // Build Block
            if (Input.GetMouseButtonDown(1))
            {
                Toolbar toolbar = IGUIManager.Instance.GetInventory.toolbar;
                if (toolbar.inventory.slots[toolbar.slotIndex].HasItem)
                {
                    int3 globalPos = placeBlockPosition.FloorToInt3();
                    if (!rbody.PositionInside(globalPos))
                    {
                        playerSounds.Play("BlockPlace");
                        RimecraftWorld.worldData.SetVoxel(globalPos, toolbar.inventory.slots[toolbar.slotIndex].itemSlot.stack.id);
                        toolbar.inventory.slots[toolbar.slotIndex].itemSlot.Take(1);
                    }
                }
            }
        }
    }

    private void DestroyBlock()
    {
        if (Input.GetMouseButton(0))
        {
            curBounceCount = GetBounceCount(heldDownTime);
            curSpeed = GetSpeed(heldDownTime);
            heldDownTime += Time.deltaTime;
            toolLight.intensity = 1 + curBounceCount * 0.1f;
        }

        if (Input.GetMouseButtonUp(0))
        {
            playerSounds.Play("WeaponUse");
            Vector3 position = Camera.main.transform.position + Camera.main.transform.forward;
            Vector3 direction = Camera.main.transform.forward;
            Projectile.SpawnProjectile(position, direction, curSpeed, curBounceCount, true);
            ClientSend.SpawnProjectile(position, direction, curSpeed, curBounceCount);
            heldDownTime = 0;
            toolLight.intensity = 1;
        }
    }

    private int GetBounceCount(float heldTime)
    {
        return Mathf.FloorToInt(heldTime * 1.5f) + 1;
    }

    private float GetSpeed(float heldTime)
    {
        return Mathf.Sqrt(heldTime * 40) + 8;
    }

    private void Jump()
    {
        playerSounds.Play("Jump");
        rbody.VerticalMomentum = jumpForce;
        rbody.IsGrounded = false;
        jumpRequest = false;
    }

    private void PlaceCursorBlock()
    {
        float step = checkIncrement;
        float3 lastPos = new Vector3();

        while (step < reach)
        {
            float3 position = cam.position + (cam.forward * step);
            int3 roundedPosition = new int3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));

            if (RimecraftWorld.worldData.CheckForVoxel(roundedPosition) != 0)
            {
                highlightBlock.position = new float3(roundedPosition);
                placeBlockPosition = lastPos;

                highlightBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new float3(roundedPosition);

            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
    }

    private void Start()
    {
        rbody = this.gameObject.GetComponent<ElipsoidRigidbody>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * RimecraftWorld.settings.mouseSensitivity * Time.deltaTime * 60;
        mouseY += delta.y;
        mouseY = Mathf.Clamp(mouseY, -89.99f, 89.99f);
        Quaternion mouseRotationHorizontal = Quaternion.AngleAxis(delta.x, Vector3.up);
        Quaternion mouseRotationVertical = Quaternion.AngleAxis(mouseY, -Vector3.right);

        cam.rotation = Quaternion.LookRotation(transform.forward, Vector3.up) * mouseRotationVertical;
        transform.rotation = (Quaternion.LookRotation(transform.forward, Vector3.up) * mouseRotationHorizontal);
        cam.position = new Vector3(transform.position.x, transform.position.y + 1.62f, transform.position.z - 0.15f);

        if (Input.GetKeyDown(KeyCode.R))
        {
            this.transform.position = RimecraftWorld.Instance.spawnPosition;
        }

        if (!IGUIManager.Instance.InUI)
        {
            GetPlayerInputs();
            PlaceCursorBlock();
        }
    }

    private void LateUpdate()
    {
        highlightBlock.rotation = Quaternion.identity;
    }
}