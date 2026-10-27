using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InteractionPrompt : MonoBehaviour
{
    // Instância global para facilitar o acesso de qualquer item
    public static InteractionPrompt Instance;

    [Header("Elementos de UI")]
    [Tooltip("O painel ou fundo do aviso na tela.")]
    public GameObject promptPanel;
    
    [Tooltip("O componente de Texto (TextMeshPro) que mostra a mensagem.")]
    public TextMeshProUGUI promptText;
    private HashSet<object> _activePrompts = new HashSet<object>();

    private void Start()
    {
        // Força a mensagem a começar invisível/desativada ao iniciar a cena
        gameObject.SetActive(false); // Ou esconde o componente de texto
    }
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

    private void Start(object requester)
    {
        Hide(requester);
    }

    public void Show(string message, object requester)
    {
        if (!_activePrompts.Contains(requester)) _activePrompts.Add(requester);
        if (promptPanel != null) promptPanel.SetActive(true);
        if (promptText != null) promptText.text = message;
    }

    public void Hide(object requester)
    {
        if (_activePrompts.Contains(requester)) _activePrompts.Remove(requester);
        if (promptPanel != null) promptPanel.SetActive(false);
    }
}