using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    // Instância global para facilitar o acesso de qualquer item
    public static InteractionPrompt Instance;

    [Header("Elementos de UI")]
    [Tooltip("O painel ou fundo do aviso na tela.")]
    public GameObject promptPanel;
    
    [Tooltip("O componente de Texto (TextMeshPro) que mostra a mensagem.")]
    public TextMeshProUGUI promptText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Hide();
    }

    public void Show(string message)
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (promptText != null) promptText.text = message;
    }

    public void Hide()
    {
        if (promptPanel != null) promptPanel.SetActive(false);
    }
}