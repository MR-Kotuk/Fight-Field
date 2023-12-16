using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsUI : MonoBehaviour
{
    [SerializeField] private Button _crouchButton;

    [SerializeField] private SpriteManager _spriteManager;
    [SerializeField] private PlayerMove _playerMove;

    private void Update()
    {
        if (_playerMove.isCrouch)
            _crouchButton.GetComponent<Image>().sprite = _spriteManager.WakeUpUI;
        else
            _crouchButton.GetComponent<Image>().sprite = _spriteManager.CrouchUI;
    }
}
