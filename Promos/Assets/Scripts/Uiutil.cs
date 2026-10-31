using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Funções de UI usadas pelos três gerenciadores (não vai em nenhum GameObject)
public static class UIUtil
{
    // Escreve um texto em TextMeshPro ou Text normal
    public static void SetTexto(Component alvo, string valor)
    {
        if (alvo is TMP_Text tmp) tmp.text = valor;
        else if (alvo is Text legado) legado.text = valor;
    }

    // Deixa um objeto clicável. Se ele não tem imagem, cria uma invisível só para receber o clique.
    public static Button TornarClicavel(GameObject go, UnityAction acao)
    {
        Graphic alvo = go.GetComponent<Graphic>();
        if (alvo == null)
        {
            Image img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.canvasRenderer.cullTransparentMesh = false;
            alvo = img;
        }

        Button botao = go.GetComponent<Button>();
        if (botao == null) botao = go.AddComponent<Button>();
        if (botao.targetGraphic == null) botao.targetGraphic = alvo;

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(acao);
        return botao;
    }

    // Escreve o texto dentro de uma tecla (procura TextMeshPro, depois Text)
    public static void EscreverNaTecla(GameObject tecla, string texto)
    {
        if (string.IsNullOrEmpty(texto)) return;

        TMP_Text tmp = tecla.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) { tmp.text = texto; return; }

        Text legado = tecla.GetComponentInChildren<Text>(true);
        if (legado != null) legado.text = texto;
    }

    // Esconde um molde que está na cena (só as cópias devem aparecer)
    public static void EsconderSeEstiverNaCena(GameObject molde)
    {
        if (molde != null && molde.scene.IsValid()) molde.SetActive(false);
    }
}

