using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    private void Start()
    {
        Vector3 createPos = new Vector3(0f, 3f, 0f);
        PhotonNetwork.Instantiate(_player.name, createPos, Quaternion.identity);
    }
}
