using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class toastNotifications : MonoBehaviour
{
    public static toastNotifications Instance;
    public GameObject painelToast;
    public TMP_Text textoMessagem;
    public Image fundoToast;

    public Color corSucesso = new Color(0.1f, 0.6f, 0.2f, 1f); 
    public Color corErro = new Color(0.8f, 0.2f, 0.2f, 1f);

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void mostrarToast(string message, bool isErro)
    {
        Debug.Log("Toast executado visualmente com a mensagem: " + message);

        // Garante que o GameObject principal e o painel ligam
        gameObject.SetActive(true);
        if (painelToast != null)
        {
            painelToast.SetActive(true);
        }

        if (textoMessagem != null)
        {
            textoMessagem.text = message;
        }

        if (fundoToast != null)
        {
            fundoToast.color = isErro ? corErro : corSucesso;
        }
    }

    public void esconderToast()
    {
        if (painelToast != null) 
        {
            painelToast.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}