using TMPro;
using UnityEngine;
using UnityEngine.XR;            // p/ pegar posição da câmera em VR

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Placar")]
    public TextMeshProUGUI scoreText;

    [Header("Meta de cortes")]
    public int targetScore = 1;            // mude aqui a quantidade X
    private int _score = 0;
    private bool _victoryTriggered = false;

    [Header("Efeitos de Vitória")]
    public GameObject animationPrefab;     // arraste o prefab do Bob
    public AudioClip victoryMusic;          // arraste a música (wav/mp3)
    public Canvas victoryCanvas;            // painel “você cortou tudo”

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // esconde o painel de vitória no início
        if (victoryCanvas != null)
            victoryCanvas.enabled = false;
    }

    // ───────────────────────────────────────
    public void AddScore(int value = 1)
    {
        _score += value;
        if (scoreText != null)
            scoreText.text = $"Score: {_score}";

        if (!_victoryTriggered && _score >= targetScore)
            TriggerVictory();
    }

    // ───────────────────────────────────────
    private void TriggerVictory()
    {
        _victoryTriggered = true;

        // 1. Mostra texto 
        if (victoryCanvas != null)
            victoryCanvas.enabled = true;

        // 2. Mostra a animação
        if (animationPrefab != null)
        {
            animationPrefab.SetActive(true);
        }

        // 3. Toca música
        if (victoryMusic != null)
            AudioSource.PlayClipAtPoint(victoryMusic, Camera.main.transform.position);
    }
}
