using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ExitedOfRoom : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && Input.GetKey(KeyCode.E))
        {
            Cursor.lockState = CursorLockMode.None;

            PhotonNetwork.LeaveRoom();

            SceneManager.LoadScene("Menu");
        }

    }
}
