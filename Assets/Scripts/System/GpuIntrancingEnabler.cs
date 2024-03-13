using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class GpuIntrancingEnabler : MonoBehaviour
{
    private void Awake()
    {
        MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(matPropertyBlock);
    }
}
