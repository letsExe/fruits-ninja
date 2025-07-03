using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnFruitSimple : MonoBehaviour
{
    [Header("Prefabs das frutas")]
    public GameObject[] fruitPrefabs;      // vários tipos de fruta

    [Header("Quantos e onde")]
    public int quantidade = 20;            // quantas frutas criar
    public float spawnRadius = 3f;         // raio horizontal em torno do ponto-base
    public float heightOffset = -1f;       // altura relativa ao ponto-base

    [Header("Velocidade")]
    public float speedMin = 4f;
    public float speedMax = 7f;

    void Start()
    {
        SpawnFruits();
    }

    void SpawnFruits()
    {
        // Ponto-base = posição deste objeto na cena
        Vector3 basePos = transform.position;

        Transform cam = Camera.main.transform;   // alvo = câmera (cabeça do player)

        for (int i = 0; i < quantidade; i++)
        {
            // 1) Posição aleatória dentro de um círculo
            Vector2 offset2D = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = basePos +
                               new Vector3(offset2D.x, heightOffset, offset2D.y);

            // 2) Escolhe prefab aleatoriamente
            GameObject prefab = fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
            GameObject fruit  = Instantiate(prefab, spawnPos, Quaternion.identity);

            // 3) Garante Rigidbody
            Rigidbody rb = fruit.GetComponent<Rigidbody>();
            if (rb == null) rb = fruit.AddComponent<Rigidbody>();

            // 4) Calcula direção → câmera  (+ variação pequena p/ não ficar tudo igual)
            Vector3 dir = (cam.position - spawnPos).normalized;
            float   speed = Random.Range(speedMin, speedMax);

            rb.velocity = dir * speed;

            // 5) Gira a fruta no ar (opcional)
            rb.angularVelocity = Random.insideUnitSphere * 4f;
        }
    }
}
