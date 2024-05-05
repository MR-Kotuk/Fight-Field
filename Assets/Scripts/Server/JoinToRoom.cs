using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

public class JoinToRoom : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField _createInput;
    [SerializeField] private InputField _joinInput;

    public void CreateRoom()
    {
        if (_createInput.text != string.Empty)
        {
            Debug.Log($"Created room: {_createInput.text}");
            PhotonNetwork.CreateRoom(_createInput.text);
        }
        else
            Debug.Log("Room has non corrected name! You can try again!");
    }

    public void JoinRoom()
    {
        try
        {
            PhotonNetwork.JoinRoom(_joinInput.text);
        }
        catch
        {
            Debug.Log("This room is not exist!!!");
        }
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
}
