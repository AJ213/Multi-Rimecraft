using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject world;
    public GameObject playerPrefab;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
    {
        GameObject _player;
        if (id == Client.Instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, position, rotation);
            Instantiate(world);
        }
        else
        {
            _player = Instantiate(playerPrefab, position, rotation);
        }
        PlayerManager manager = _player.GetComponent<PlayerManager>();
        manager.id = id;
        manager.username = username;
        if (players.ContainsKey(id))
        {
            players.Remove(id);
        }
        players.Add(id, manager);
    }
}