using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class SliceObject : MonoBehaviour
{
    [Header("Pontos de corte")]
    public Transform startSlicePoint;   // ponta inicial da lâmina
    public Transform endSlicePoint;     // ponta final da lâmina

    [Header("Configurações")]
    public LayerMask sliceableLayer;    // layer dos objetos fatiáveis
    public VelocityEstimator velocityEstimator;
    public Material crossSectionMaterial;

    [Tooltip("Força de impulso aplicada às metades (N·s)")]
    public float cutForce = 2f;

    [Tooltip("Velocidade mínima (m/s) para considerar que houve corte")]
    public float minCutSpeed = 0.25f;

    [Tooltip("Raio virtual da lâmina (m) – usado no CapsuleCast")]
    public float bladeRadius = 0.02f;

    // cache para não cortar o mesmo objeto duas vezes no mesmo frame
    private readonly HashSet<GameObject> _alreadySliced = new HashSet<GameObject>();

    private void FixedUpdate()
    {
        // estimativa da velocidade do controlador
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();

        // ignora se estamos “parados”
        if (velocity.magnitude < minCutSpeed)
            return;

        // direção do movimento da lâmina
        Vector3 dir = endSlicePoint.position - startSlicePoint.position;
        float distance = dir.magnitude;
        dir.Normalize();

        // CapsuleCast pega objetos pequenos que Linecast costuma “pular”
        RaycastHit[] hits = Physics.CapsuleCastAll(
            startSlicePoint.position,
            endSlicePoint.position,
            bladeRadius,
            dir,
            0f,
            sliceableLayer);

        if (hits.Length == 0)
            return;

        _alreadySliced.Clear();

        foreach (RaycastHit hit in hits)
        {
            GameObject target = hit.transform.gameObject;

            // evita fatiar duas vezes o mesmo objeto no mesmo FixedUpdate
            if (_alreadySliced.Contains(target))
                continue;

            Slice(target, velocity);
            _alreadySliced.Add(target);
        }
    }

    private void Slice(GameObject target, Vector3 velocity)
    {
        // plano de corte: normal = trajetória × velocidade
        Vector3 sliceDir = endSlicePoint.position - startSlicePoint.position;
        Vector3 planeNormal = Vector3.Cross(sliceDir, velocity);

        // se a normal ficou quase zero, aborta
        if (planeNormal.sqrMagnitude < 1e-4f)
            return;

        planeNormal.Normalize();

        // corta o objeto
        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
        if (hull == null)
            return;

        // cria metades
        GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
        SetupSliceComponent(upperHull, planeNormal);

        GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
        SetupSliceComponent(lowerHull, -planeNormal);

        // remove original
        Destroy(target);
    }

    private void SetupSliceComponent(GameObject sliceObj, Vector3 impulseDir)
    {
        // mantém a mesma layer/tag do objeto original
        sliceObj.layer = gameObject.layer;
        sliceObj.tag = gameObject.tag;

        // colisão e física
        var rb = sliceObj.AddComponent<Rigidbody>();
        var col = sliceObj.AddComponent<MeshCollider>();
        col.convex = true;

        // dá um pequeno empurrão na direção da normal do corte
        rb.AddForce(impulseDir * cutForce, ForceMode.Impulse);
    }
}
