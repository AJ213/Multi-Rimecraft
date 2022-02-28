using Unity.Mathematics;
using UnityEngine;

public class ElipsoidRigidbody : MonoBehaviour
{
    [SerializeField] private float objectWidth = 0.25f;
    public float objectHeight = 1.6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private bool usesGravity = false;
    [SerializeField] private Vector3 velocity;

    public float VerticalMomentum { get; set; }
    public bool IsGrounded { get; set; }

    public bool PositionInside(int3 position)
    {
        bool3 result3 = (transform.position.FloorToInt3() == position);
        bool result = result3.x && result3.y && result3.z;
        for (int i = 1; i <= (int)objectHeight; i++)
        {
            result3 = (transform.position.FloorToInt3() + new int3(0, i, 0) == position);
            result |= result3.x && result3.y && result3.z;
        }
        return result;
    }

    private bool Colliding(int3 position)
    {
        bool result = RimecraftWorld.worldData.CheckForVoxel(position) == 0;
        for (int i = 1; i <= (int)objectHeight; i++)
        {
            result &= RimecraftWorld.worldData.CheckForVoxel(position + new int3(0, i, 0)) == 0;
        }
        return !result;
    }

    public bool BackCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z - (objectWidth - velocity.z)));
        return Colliding(position);
    }

    public bool FrontCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z + (objectWidth + velocity.z)));
        return Colliding(position);
    }

    public bool LeftCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x - (objectWidth - velocity.x)), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
        return Colliding(position);
    }

    public bool RightCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x + (objectWidth + velocity.x)), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
        return Colliding(position);
    }

    public void CalculateVelocity(float horizontal, float vertical, float speed, bool crouching = false)
    {
        // Affect verical momentum with gravity.
        if (usesGravity)
        {
            VerticalMomentum += Time.fixedDeltaTime * gravity;
        }

        // if we're sprinting, use the sprint multiplier.
        if (crouching)
        {
            if (!ObjectObstructedVerticallyAt(-0.8f, this.transform.position + transform.forward * vertical * Time.fixedDeltaTime * speed))
            {
                vertical = 0;
            }
            if (!ObjectObstructedVerticallyAt(-0.8f, this.transform.position + transform.right * horizontal * Time.fixedDeltaTime * speed))
            {
                horizontal = 0;
            }
        }
        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * speed;

        // Apply vertical momentum (falling/jumping).
        if (usesGravity)
        {
            velocity += Vector3.up * VerticalMomentum * Time.fixedDeltaTime;
        }

        if ((velocity.z > 0 && FrontCollision()) || (velocity.z < 0 && BackCollision()))
        {
            velocity.z = 0;
        }

        if ((velocity.x > 0 && RightCollision()) || (velocity.x < 0 && LeftCollision()))
        {
            velocity.x = 0;
        }

        if (velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }
    }

    private int3 ObjectWidthBlockLocations(int index, Vector3 position, float verticalOffset)
    {
        // Grabs the top right position block relative to object
        float widthAdjustment = objectWidth;

        switch (index)
        {
            case 0:
                return new int3(Mathf.FloorToInt(position.x - widthAdjustment), Mathf.FloorToInt(position.y + verticalOffset), Mathf.FloorToInt(position.z - widthAdjustment));

            case 1:
                return new int3(Mathf.FloorToInt(position.x + widthAdjustment), Mathf.FloorToInt(position.y + verticalOffset), Mathf.FloorToInt(position.z - widthAdjustment));

            case 2:
                return new int3(Mathf.FloorToInt(position.x + widthAdjustment), Mathf.FloorToInt(position.y + verticalOffset), Mathf.FloorToInt(position.z + widthAdjustment));

            default:
                return new int3(Mathf.FloorToInt(position.x - widthAdjustment), Mathf.FloorToInt(position.y + verticalOffset), Mathf.FloorToInt(position.z + widthAdjustment));
        }
    }

    public bool ObjectObstructedVerticallyAt(float height, Vector3 position)
    {
        bool obstructed = true;
        for (int i = 0; i < 4; i++)
        {
            obstructed &= RimecraftWorld.worldData.CheckForVoxel(ObjectWidthBlockLocations(i, position, height)) == 0;
        }
        return !obstructed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, objectWidth);
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (ObjectObstructedVerticallyAt(downSpeed, transform.position))
        {
            IsGrounded = true;
            VerticalMomentum = 0;
            return 0;
        }
        else
        {
            IsGrounded = false;
            return downSpeed;
        }
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if (ObjectObstructedVerticallyAt(upSpeed + objectHeight, transform.position))
        {
            VerticalMomentum = 0;
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (RimecraftWorld.worldData.GetChunk(this.transform.position.FloorToInt3()) != null)
        {
            if (!IGUIManager.Instance.InUI)
            {
                transform.Translate(velocity, Space.World);
            }
        }
    }
}