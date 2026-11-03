using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class toastNotifications : MonoBehaviour
{
    public static toastNotifications Instance;
    public GameObject painelToast;
    public TMP_Text textoMessagem;
    public Image fundoToast;

    public Color corSucesso = new Color(0.15f, 0.45f, 0.25f, 0.95f); // Verde-escuro elegante (Sucesso)
    public Color corErro = new Color(0.55f, 0.15f, 0.15f, 0.95f);    // Vermelho-escuro/Rubi (Erro)
    public Color corWarning = new Color(0.75f, 0.55f, 0.2f, 0.95f);   // Amarelo-dourado alinhado com o menu (Aviso)

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
        if (painelToast != null) painelToast.SetActive(false);
    }
    public void mostrarToast(string message, bool isErro)
    {
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
        if (painelToast != null) painelToast.SetActive(false);
        gameObject.SetActive(false);
    }
}
