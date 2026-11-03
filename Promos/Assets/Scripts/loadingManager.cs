using UnityEngine;
using TMPro;
using System.Collections;

public class loadingManager : MonoBehaviour
{
    public RectTransform imagemRodando; 
    public TMP_Text textoLoading;     
    public float velocidadeRotacao = -200f; 
    public float intervaloTexto = 0.5f;    

    private Vector3 escalaOriginal;

    private void Awake()
    {
        if (imagemRodando!= null)
        {
            escalaOriginal = imagemRodando.localScale;
        } else
        {
            escalaOriginal = Vector3.one;
        }
    }
    private void OnEnable()
    {
        StartCoroutine(AnimarTextoLoading());
    }

    private void Update()
    {
        if (imagemRodando != null)
        {
            float fatorx = Mathf.Sin(Time.unscaledTime * velocidadeRotacao);
            Vector3 novaEscala = escalaOriginal;
            novaEscala.x = escalaOriginal.x * fatorx;
            imagemRodando.localScale = novaEscala;
        }
    }

    private IEnumerator AnimarTextoLoading()
    {
        string baseTexto = "Loading";
        int pontos = 0;

        while (true)
        {
            if (textoLoading != null)
            {
                string p = "";
                for (int i = 0; i < pontos; i++)
                {
                    p += ".";
                }
                textoLoading.text = baseTexto + p;

                pontos = (pontos + 1) % 4; 
            }

            yield return new WaitForSecondsRealtime(intervaloTexto);
        }
    }
}