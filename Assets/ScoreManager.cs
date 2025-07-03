using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] Canvas victoryCanvas;       // painel “você cortou tudo” (desativado)

    [Header("Pontuação")]
    [SerializeField] int targetScore = 10;       // meta para vencer
    int _score;
    bool _victoryTriggered;

    [Header("Efeitos de Vitória")]
    [SerializeField] GameObject animationGO;     // animação já na cena, mas desativada
    [SerializeField] AudioSource victoryAudio;   // AudioSource com clip pré-carregado

    void Awake()
    {
        // Singleton básico
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Garante tudo oculto no início
        if (victoryCanvas)  victoryCanvas.gameObject.SetActive(false);
        if (animationGO)    animationGO.SetActive(false);

        // ── Warm-up: ativa e desativa 1x para subir texturas/meshes p/ GPU ──
        if (animationGO)
        {
            animationGO.SetActive(true);
            animationGO.SetActive(false);
        }
    }

    // Chamado pelo SliceObject a cada corte
    public void AddScore(int value = 1)
    {
        _score += value;
        if (scoreText) scoreText.text = $"Score: {_score}";

        if (!_victoryTriggered && _score >= targetScore)
            TriggerVictory();
    }

    // Dispara tela + animação + som sem travar
    void TriggerVictory()
    {
        _victoryTriggered = true;

        if (victoryCanvas)  victoryCanvas.gameObject.SetActive(true); // mostra “vc cortou tudo”
        if (animationGO)    animationGO.SetActive(true);              // animação já pré-carregada
        if (victoryAudio)   victoryAudio.Play();                      // áudio já na memória
    }
}
