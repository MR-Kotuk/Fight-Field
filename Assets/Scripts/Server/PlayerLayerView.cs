using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLayerView : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private string _anyPlayerSkin, _anyPlayerHands;
    [SerializeField] private string _playerSkin, _playerHands;
    [Space]

    [Header("Game Objects")]
    [SerializeField] private GameObject _player, _hands;
    [Space]

    [Header("Photon Pun 2")]
    [SerializeField] private PhotonView _playerView;

    private void Start()
    {
        AddCorrectLayer();
    }

    private void SetLayerAllChildren(Transform parent, int layer)
    {
        
    }

    private void AddCorrectLayer()
    {
        int currentLayer;
        int skinLayer;
        int handsLayer;

        if (_playerView.IsMine)
        {
            skinLayer = LayerMask.NameToLayer(_playerSkin);
            handsLayer = LayerMask.NameToLayer(_playerHands);
        }
        else
        {
            skinLayer = LayerMask.NameToLayer(_anyPlayerSkin);
            handsLayer = LayerMask.NameToLayer(_anyPlayerHands);
        }

        currentLayer = skinLayer;

        Transform[] children = _player.GetComponentsInChildren<Transform>(includeInactive: true);

        foreach (Transform child in children)
        {
            if (child == _hands.transform || child.IsChildOf(_hands.transform))
                currentLayer = handsLayer;
            else
                currentLayer = skinLayer;

            child.gameObject.layer = currentLayer;
        }
    }
}
