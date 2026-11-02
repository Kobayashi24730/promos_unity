using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class toastNotifications : MonoBehaviour
{
    public static toastNotifications Instance;
    public TMP_Text textoMessagem;
    public Image fundoToast;

    public Color corSucesso = new Color(0.1f, 0.6f, 0.2f, 1f); 
    public Color corErro = new Color(0.8f, 0.2f, 0.2f, 1f);

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
    }
    public void mostrarToast(string message, bool isErro, float duracao = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(rotinaExibirToast(message, isErro, duracao));
    }

    public IEnumerator rotinaExibirToast(string message, bool isErro, float duracao)
    {
        if (textoMessagem != null)
        {
            textoMessagem.text = message;
        }
        if (fundoToast != null)
        {
            fundoToast.color = isErro ? corErro : corSucesso;
        }
        transform.root.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(duracao);
        transform.root.gameObject.SetActive(false);
    }
}
