using System.Collections;
using UnityEngine;
using Unity.Mathematics;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Vector3 directionVector;
    private SphericalRigidbody rbody;

    public int3 projectileChunkCoord;
    private int3 projectileLastChunkCoord;
    [SerializeField] private GameObject[] sounds = default;

    [SerializeField] private float speed = 1;
    [SerializeField] private float bounceCoolDown = 0.3f;
    [SerializeField] private bool bouncing = false;
    [SerializeField] private int bounces = 3;
    private bool canBreakBlocks = true;

    private void Awake()
    {
        rbody = GetComponent<SphericalRigidbody>();
        projectileLastChunkCoord = WorldHelper.GetChunkCoordFromPosition(this.transform.position);
    }

    public void SetBreakBlocks(bool state)
    {
        canBreakBlocks = state;
    }

    public static void SpawnProjectile(Vector3 position, Vector3 direction, bool breakState = false)
    {
        GameObject projectile = Instantiate(Resources.Load("Mining Projectile"), position, Quaternion.identity) as GameObject;
        Projectile proj = projectile.GetComponent<Projectile>();
        proj.Fire(direction, 10, 3);
        proj.SetBreakBlocks(breakState);
    }

    public void Fire(Vector3 directionVector, float speed, int bounceCount)
    {
        this.directionVector = directionVector.normalized;
        this.speed = speed;
        this.bounces = bounceCount;
    }

    private void Update()
    {
        projectileChunkCoord = WorldHelper.GetChunkCoordFromPosition(this.transform.position);

        if (!projectileChunkCoord.Equals(projectileLastChunkCoord))
        {
            bool3 tooFar = math.abs(RimecraftWorld.Instance.playerChunkCoord - projectileChunkCoord) >= RimecraftWorld.settings.viewDistance;
            if (tooFar.x || tooFar.y || tooFar.z)
            {
                Destroy(this.gameObject);
            }
            else
            {
                projectileLastChunkCoord = projectileChunkCoord;
            }
        }

        if (rbody.colliding && bounces > 0)
        {
            if (!bouncing)
            {
                if (canBreakBlocks)
                {
                    BreakBlock();
                }

                CalculateReflection();
                StartCoroutine(Bouncing());
                bounces--;
            }
        }

        rbody.CalculateVelocity(directionVector, speed);

        if (bounces == 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void BreakBlock()
    {
        int3 breakBlock = rbody.lastCollidedWithBlockLocation;
        ushort blockBreakingID = WorldData.GetVoxelFromPosition(breakBlock);

        ChunkData chunk = WorldData.RequestChunkViaGlobalPosition(breakBlock, false);
        if (chunk != null)
        {
            RimecraftWorld.worldData.SetVoxel(breakBlock, 0);
        }

        if (blockBreakingID == 2)
        {
            Instantiate(sounds[0], this.transform.position, Quaternion.identity);
        }
        else if (blockBreakingID == 1)
        {
            Instantiate(sounds[1], this.transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(sounds[2], this.transform.position, Quaternion.identity);
        }

        if (blockBreakingID != 0)
        {
            GameObject droppedBlock = (GameObject)Instantiate(Resources.Load("DroppedItem"), this.transform.position, Quaternion.identity);
            droppedBlock.GetComponent<DropItem>().SetItemStack(blockBreakingID, 1);
        }
    }

    private void CalculateReflection()
    {
        rbody.UpCollision();
        rbody.DownCollision();
        Vector3 normal = rbody.collisionNormal;

        directionVector -= (2 * Vector3.Dot(directionVector, normal) * normal);
    }

    private IEnumerator Bouncing()
    {
        bouncing = true;
        yield return new WaitForSeconds(bounceCoolDown);
        bouncing = false;
    }
}