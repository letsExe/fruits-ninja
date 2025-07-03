using System.Collections.Generic;
using UnityEngine;
using EzySlice;

/// <summary>
/// Corta objetos “fatiáveis” (Fruit-Ninja style) no VR.
/// A lâmina é definida por dois pontos (start e end) presas no controller.
/// Usa o EzySlice para separar a malha em duas metades.
/// </summary>
public class SliceObject : MonoBehaviour
{
    // ─────────── Campos configuráveis no Inspector ───────────

    [Header("Pontos de corte")]
    public Transform startSlicePoint;   // posição da ponta inicial da lâmina
    public Transform endSlicePoint;     // posição da ponta final da lâmina

    [Header("Configurações")]
    public LayerMask sliceableLayer;    // somente objetos nessa Layer podem ser cortados
    public VelocityEstimator velocityEstimator;  // mede a velocidade do controller
    public Material crossSectionMaterial;        // material da “parte interna” do corte

    [Tooltip("Impulso dado às metades após o corte")]
    public float cutForce = 2f;         // força do empurrão

    [Tooltip("Velocidade mínima para considerar que houve corte")]
    public float minCutSpeed = 0.25f;   // evita cortes acidentais quando o controle está parado

    [Tooltip("Raio virtual da lâmina (para o CapsuleCast)")]
    public float bladeRadius = 0.02f;   // espessura do feixe que detecta o acerto

    // Guarda quem já foi cortado nesse frame, para não fatiar duas vezes
    private readonly HashSet<GameObject> _alreadySliced = new HashSet<GameObject>();

    // ─────────── Loop de Física ───────────
    private void FixedUpdate()
    {
        // 1) Mede quão rápido o controller está se movendo
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();

        // Se a mão está lenta, sai sem cortar nada
        if (velocity.magnitude < minCutSpeed)
            return;

        // 2) Define a direção da lâmina entre os dois pontos
        Vector3 dir = endSlicePoint.position - startSlicePoint.position;
        dir.Normalize();

        // 3) Verifica se a lâmina intercepta algum objeto fatiável
        //    CapsuleCast é mais “gordo” que Linecast e acerta objetos pequenos
        RaycastHit[] hits = Physics.CapsuleCastAll(
            startSlicePoint.position,
            endSlicePoint.position,
            bladeRadius,
            dir,
            0f,
            sliceableLayer);

        if (hits.Length == 0) return; // nada foi atingido

        _alreadySliced.Clear();       // limpa cache por frame

        foreach (RaycastHit hit in hits)
        {
            GameObject target = hit.transform.gameObject;

            // pula se já cortamos esse objeto neste FixedUpdate
            if (_alreadySliced.Contains(target))
                continue;

            Slice(target, velocity);     // tenta fatiar
            _alreadySliced.Add(target);  // marca como já cortado
        }
    }

    // ─────────── Faz o corte propriamente dito ───────────
    private void Slice(GameObject target, Vector3 velocity)
    {
        // 1) Calcula o plano de corte.
        //    Normal = (trajetória da lâmina) × (direção da mão)
        Vector3 sliceDir = endSlicePoint.position - startSlicePoint.position;
        Vector3 planeNormal = Vector3.Cross(sliceDir, velocity);

        // Se a normal ficou quase zero, aborta (evita divisões degeneradas)
        if (planeNormal.sqrMagnitude < 1e-4f)
            return;

        planeNormal.Normalize();   // deixa a normal com comprimento 1

        // 2) Usa o EzySlice para cortar
        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
        if (hull == null) return;  // falhou? sai

        // 3) Cria as duas metades
        GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
        SetupSliceComponent(upperHull,  planeNormal);  // empurra pra cima

        GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
        SetupSliceComponent(lowerHull, -planeNormal);  // empurra pra baixo

        // 4) Remove o objeto original
        Destroy(target);

        // 5) Soma pontos no placar
        ScoreManager.Instance?.AddScore(1);
    }

    // ─────────── Prepara cada metade para “ganhar vida” ───────────
    private void SetupSliceComponent(GameObject sliceObj, Vector3 impulseDir)
    {
        // Mantém layer e tag iguais ao original (opcional)
        sliceObj.layer = gameObject.layer;
        sliceObj.tag   = gameObject.tag;

        // Adiciona física
        var rb  = sliceObj.AddComponent<Rigidbody>();
        var col = sliceObj.AddComponent<MeshCollider>();
        col.convex = true;                 // obrigatório para MeshCollider funcionar com Rigidbody

        // Dá um empurrão leve para separar visualmente as metades
        rb.AddForce(impulseDir * cutForce, ForceMode.Impulse);
    }
}
