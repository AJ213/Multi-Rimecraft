using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject world;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
    {
        GameObject _player;
        if (id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, position, rotation);
            Instantiate(world);
        }
        else
        {
            _player = Instantiate(playerPrefab, position, rotation);
        }

        _player.GetComponent<PlayerManager>().id = id;
        _player.GetComponent<PlayerManager>().username = username;
        players.Add(id, _player.GetComponent<PlayerManager>());
    }
}