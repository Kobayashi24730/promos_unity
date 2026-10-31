using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Uma barra de atributo do player (Vida, Magia, Vigor...)
[Serializable]
public class BarraAtributoUI
{
    [Tooltip("Vida, Stamina (ou Magia) e Vigor usam os campos do save. Outros nomes viram atributos extras.")]
    public string nomeAtributo = "Vida";

    [Header("Objetos da barra na Hierarchy")]
    [Tooltip("O texto 'Valor': mostra atual/máximo, ex.: 30/560.")]
    public Graphic textoValor;
    [Tooltip("O 'Trilho' (fundo da barra): define a largura máxima.")]
    public RectTransform trilho;
    [Tooltip("O 'Preenchimento' (filho do Trilho): é ele que cresce.")]
    public RectTransform preenchimento;
    [Tooltip("Onde a tecla é criada quando há pontos (ex.: o próprio Barra_Vida).")]
    public RectTransform ondeCriarTecla;
    [Tooltip("Posição da tecla dentro de 'Onde Criar Tecla'. 0,0 = usa a posição do prefab.")]
    public Vector2 posicaoDaTecla = Vector2.zero;

    [Header("Valores")]
    [Tooltip("Só vale para atributos extras. Vida, Stamina e Vigor usam o save.")]
    public int valorInicial = 30;
    [Tooltip("Valor em que o trilho fica cheio.")]
    public int valorMaximo = 560;
    [Tooltip("Quanto o atributo sobe a cada ponto gasto.")]
    public int incrementoPorPonto = 10;
    [Tooltip("Largura do Preenchimento cheio. 0 = calcula pela largura do Trilho.")]
    public float larguraMaximaPreenchimento = 0f;

    [NonSerialized] public GameObject teclaCriada;
}

// GameObject: DinamicoRecursos
// Painel "Recursos do personagem": barras, texto de pontos de status e tecla de aumentar.
public class DinamicoRecursos : MonoBehaviour
{
    [Header("Texto geral")]
    public Graphic textoPontosStatus;
    [Tooltip("{0} vira o número de pontos.")]
    public string formatoPontosStatus = "PONTOS DISPONÍVEIS {0}";

    [Header("Barras (uma por atributo)")]
    public List<BarraAtributoUI> barras = new List<BarraAtributoUI>();

    [Header("Tecla de aumentar")]
    public GameObject prefabTeclaUI;
    [Tooltip("Texto escrito na tecla. Vazio = não mexe no texto do prefab.")]
    public string textoDaTecla = "+";

    void Start()
    {
        UIUtil.EsconderSeEstiverNaCena(prefabTeclaUI);
        Atualizar();
    }

    // Redesenha textos, barras e teclas com o que está no save
    public void Atualizar()
    {
        AtributosData atr = SaveSystem.Atual.atributos;

        if (textoPontosStatus != null)
            UIUtil.SetTexto(textoPontosStatus, string.Format(formatoPontosStatus, atr.pontosStatusDisponiveis));

        foreach (BarraAtributoUI barra in barras)
            AtualizarBarra(barra, atr.pontosStatusDisponiveis);
    }

    // Dá pontos de status (ex.: ao subir de nível)
    public void AdicionarPontosStatus(int quantidade)
    {
        SaveSystem.Atual.atributos.pontosStatusDisponiveis += quantidade;
        SaveSystem.Salvar();
        Atualizar();
    }

    // Gasta 1 ponto de status na barra indicada
    public void AumentarAtributo(BarraAtributoUI barra)
    {
        AtributosData atr = SaveSystem.Atual.atributos;
        if (atr.pontosStatusDisponiveis <= 0) return;

        int valor = SaveSystem.GetAtributo(barra.nomeAtributo, barra.valorInicial);
        if (valor >= barra.valorMaximo) return;

        atr.pontosStatusDisponiveis--;
        SaveSystem.SetAtributo(barra.nomeAtributo, Mathf.Min(valor + barra.incrementoPorPonto, barra.valorMaximo));
        SaveSystem.Salvar();

        Atualizar();
    }

    // Para testar: botão ⋮ do componente > esta opção (em Play)
    [ContextMenu("Teste: dar 5 pontos de status")]
    private void TesteDarPontos() { AdicionarPontosStatus(5); }

    // ------------------------- barras -------------------------

    private void AtualizarBarra(BarraAtributoUI barra, int pontos)
    {
        int maximo = Mathf.Max(1, barra.valorMaximo);
        int valor = Mathf.Clamp(SaveSystem.GetAtributo(barra.nomeAtributo, barra.valorInicial), 0, maximo);

        if (barra.textoValor != null)
            UIUtil.SetTexto(barra.textoValor, valor + "/" + maximo);

        if (barra.preenchimento != null)
        {
            barra.preenchimento.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                LarguraMaxima(barra) * ((float)valor / maximo));
        }

        AtualizarTecla(barra, pontos > 0 && valor < maximo);
    }

    private float LarguraMaxima(BarraAtributoUI barra)
    {
        if (barra.larguraMaximaPreenchimento > 0f) return barra.larguraMaximaPreenchimento;

        if (barra.trilho != null)
        {
            // desconta a margem do Preenchimento em relação ao Trilho (nos dois lados)
            float margem = barra.preenchimento != null ? Mathf.Abs(barra.preenchimento.anchoredPosition.x) * 2f : 0f;
            return Mathf.Max(0f, barra.trilho.rect.width - margem);
        }

        return 560f;
    }

    private void AtualizarTecla(BarraAtributoUI barra, bool mostrar)
    {
        if (prefabTeclaUI == null || barra.ondeCriarTecla == null) return;

        if (mostrar && barra.teclaCriada == null)
        {
            GameObject tecla = Instantiate(prefabTeclaUI, barra.ondeCriarTecla);
            tecla.name = "tecla_" + barra.nomeAtributo;
            tecla.SetActive(true);

            RectTransform rt = tecla.transform as RectTransform;
            if (rt != null && barra.posicaoDaTecla != Vector2.zero)
                rt.anchoredPosition = barra.posicaoDaTecla;

            UIUtil.TornarClicavel(tecla, () => AumentarAtributo(barra));
            UIUtil.EscreverNaTecla(tecla, textoDaTecla);
            barra.teclaCriada = tecla;
        }

        if (barra.teclaCriada != null)
            barra.teclaCriada.SetActive(mostrar);
    }
}