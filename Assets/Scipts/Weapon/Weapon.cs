using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponsUI))]
public class Weapon : MonoBehaviour
{
    public int MaxAttackCount;
    public int AttackSpeed;

    public string Name;

    public bool isReturn = false;

    [HideInInspector] public int AttackCount;

    protected Camera CurrentCamera;

    [SerializeField] protected PlayerAttack PlayerAttack;

    [SerializeField] protected Camera PlayerCamera, ScopeCamera;

    [SerializeField] protected Canvas GameCanvas;

    protected bool isScope;

    private void Start()
    {
        PlayerAttack.Reloaded += Reload;
        PlayerAttack.Scoped += Scope;

        AttackCount = MaxAttackCount;

        SwitchCamera(PlayerCamera);

        CurrentCamera.enabled = true;

        isScope = false;
    }

    protected void Scope()
    {
        isScope = !isScope;

        if (isScope)
            SwitchCamera(ScopeCamera);
        else
            SwitchCamera(PlayerCamera);

        GameCanvas.worldCamera = CurrentCamera;
    }
    private void SwitchCamera(Camera newCamera)
    {
        if (PlayerCamera == newCamera)
            ScopeCamera.enabled = false;
        else
            PlayerCamera.enabled = false;

        newCamera.enabled = true;

        CurrentCamera = newCamera;
    }
    public virtual void Attack()
    {
        Debug.Log("Attack");
    }

    public virtual void Reload()
    {
        Debug.Log("Reload");
    }
}
