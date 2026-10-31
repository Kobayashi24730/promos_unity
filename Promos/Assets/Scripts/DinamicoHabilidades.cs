using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public class HabilidadeUI
{
    [Tooltip("Identificador salvo no save (ex.: aumentar_vida). Não mude depois que existirem saves.")]
    public string id = "habilidade_1";

    [Tooltip("Nome que aparece no item gerado.")]
    public string nomeHabilidade = "Habilidade";

    [Tooltip("Id de OUTRA habilidade que precisa estar desbloqueada antes. Deixe VAZIO se não houver (não é um número).")]
    public string requer = "";

    [Tooltip("Custo da primeira compra.")]
    public int custoInicial = 3;

    [Tooltip("Quanto o custo aumenta a cada nível.")]
    public int aumentoCusto = 2;

    [Tooltip("Nível máximo. 0 = ilimitado.")]
    public int nivelMaximo = 0;

    [Tooltip("Executa só na primeira compra (efeitos, som...).")]
    public UnityEvent aoDesbloquear;

    [Tooltip("Executa a cada nível comprado, inclusive o primeiro.")]
    public UnityEvent aoMelhorar;

    [NonSerialized] public Button botaoGerado;
}

// GameObject: DinamicoHabilidades
// Lista de habilidades compráveis com pontos de habilidade. Salva nível e desbloqueio no save.
public class DinamicoHabilidades : MonoBehaviour
{
    [Header("Pontos")]
    public Graphic textoPontosHabilidade;
    [Tooltip("{0} vira o número de pontos.")]
    public string formatoPontos = "PONTOS DE HABILIDADE {0}";

    [Header("Container e prefab")]
    public Transform containerItens;
    [Tooltip("Molde de cada habilidade. Deve ter um filho chamado 'tecla'.")]
    [FormerlySerializedAs("profabeItenUI")]
    public GameObject prefabItenUI;

    [Header("Cores")]
    public Color corBloqueada = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color corDesbloqueada = Color.white;

    [Header("Nomes dos objetos no prefab")]
    public string nomeTextoNome = "nameItem";
    public string nomeTextoCusto = "pontosItem";
    public string nomeObjetoTecla = "tecla";

    [Header("Habilidades")]
    public List<HabilidadeUI> habilidades = new List<HabilidadeUI>();

    private readonly List<GameObject> itensGerados = new List<GameObject>();

    void Start()
    {
        if (prefabItenUI != null && prefabItenUI.scene.IsValid())
            prefabItenUI.SetActive(false);

        ValidarRequisitos();
        Atualizar();
    }

    // Avisa se 'Requer' aponta para um id que não existe (a habilidade ficaria bloqueada para sempre)
    private void ValidarRequisitos()
    {
        foreach (HabilidadeUI hab in habilidades)
        {
            if (!string.IsNullOrEmpty(hab.requer) && !habilidades.Exists(o => o.id == hab.requer))
            {
                Debug.LogWarning("Habilidade '" + hab.id + "': o campo Requer ('" + hab.requer +
                                 "') não é o id de nenhuma habilidade. Deixe vazio se não há requisito.", this);
            }
        }
    }

    // ------------------------- ações -------------------------

    // Compra o próximo nível (a primeira compra também desbloqueia)
    public void MelhorarHabilidade(HabilidadeUI hab)
    {
        HabilidadesData dados = SaveSystem.Atual.habilidades;
        if (!PodeMelhorar(hab, dados.pontosHabilidadeDisponiveis)) return;

        bool primeiraVez = !SaveSystem.HabilidadeDesbloqueada(hab.id);
        int nivel = SaveSystem.GetNivelHabilidade(hab.id);

        dados.pontosHabilidadeDisponiveis -= CalcularCusto(hab);
        if (primeiraVez) SaveSystem.DesbloquearHabilidade(hab.id);
        SaveSystem.SetNivelHabilidade(hab.id, nivel + 1);
        SaveSystem.Salvar();

        if (primeiraVez) hab.aoDesbloquear?.Invoke();
        hab.aoMelhorar?.Invoke();

        Atualizar();
    }

    // Dá pontos de habilidade (ex.: ao subir de nível)
    public void AdicionarPontosHabilidade(int quantidade)
    {
        SaveSystem.Atual.habilidades.pontosHabilidadeDisponiveis += quantidade;
        SaveSystem.Salvar();
        Atualizar();
    }

    [ContextMenu("Teste: dar 3 pontos de habilidade")]
    private void TesteDarPontos() { AdicionarPontosHabilidade(3); }

    // ------------------------- regras -------------------------

    private bool PodeMelhorar(HabilidadeUI hab, int pontos)
    {
        bool jaTem = SaveSystem.HabilidadeDesbloqueada(hab.id);

        // o requisito só vale para a primeira compra
        bool requisito = jaTem || string.IsNullOrEmpty(hab.requer) || SaveSystem.HabilidadeDesbloqueada(hab.requer);
        if (!requisito) return false;

        if (NoMaximo(hab)) return false;

        return pontos >= CalcularCusto(hab);
    }

    private bool NoMaximo(HabilidadeUI hab)
    {
        return hab.nivelMaximo > 0 && SaveSystem.GetNivelHabilidade(hab.id) >= hab.nivelMaximo;
    }

    private int CalcularCusto(HabilidadeUI hab)
    {
        return hab.custoInicial + SaveSystem.GetNivelHabilidade(hab.id) * hab.aumentoCusto;
    }

    // ------------------------- UI -------------------------

    public void Atualizar()
    {
        int pontos = SaveSystem.Atual.habilidades.pontosHabilidadeDisponiveis;

        if (textoPontosHabilidade != null)
            SetTexto(textoPontosHabilidade, string.Format(formatoPontos, pontos));

        LimparItens();
        if (prefabItenUI == null || containerItens == null) return;

        foreach (HabilidadeUI hab in habilidades)
            CriarItem(hab, pontos);

        RectTransform rt = containerItens as RectTransform;
        if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    private void LimparItens()
    {
        foreach (GameObject go in itensGerados)
        {
            if (go == null) continue;
            go.SetActive(false);   // some do layout já neste frame
            Destroy(go);
        }
        itensGerados.Clear();
    }

    private void CriarItem(HabilidadeUI hab, int pontos)
    {
        GameObject ui = Instantiate(prefabItenUI, containerItens, false);
        ui.SetActive(true);
        itensGerados.Add(ui);

        int nivel = SaveSystem.GetNivelHabilidade(hab.id);
        bool podeComprar = PodeMelhorar(hab, pontos);

        PreencherTextos(
            ui.transform,
            hab.nomeHabilidade + " (Nv. " + nivel + ")",
            NoMaximo(hab) ? "MÁXIMO" : "Custo: " + CalcularCusto(hab));

        Button botao = ui.GetComponent<Button>();
        if (botao == null) botao = ui.GetComponentInChildren<Button>();
        hab.botaoGerado = botao;

        if (botao != null)
        {
            botao.interactable = podeComprar;

            if (botao.targetGraphic != null)
                botao.targetGraphic.color = SaveSystem.HabilidadeDesbloqueada(hab.id) ? corDesbloqueada : corBloqueada;

            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => MelhorarHabilidade(hab));
        }

        // a tecla só aparece quando dá para comprar agora
        Transform tecla = ui.transform.Find(nomeObjetoTecla);
        if (tecla != null) tecla.gameObject.SetActive(podeComprar);
    }

    private void PreencherTextos(Transform raiz, string nome, string custoTexto)
    {
        List<Component> textos = new List<Component>();
        textos.AddRange(raiz.GetComponentsInChildren<TMP_Text>(true));
        textos.AddRange(raiz.GetComponentsInChildren<Text>(true));

        Component txtNome = null, txtCusto = null;

        foreach (Component c in textos)
        {
            if (c.gameObject.name == nomeTextoNome) txtNome = c;
            else if (c.gameObject.name == nomeTextoCusto) txtCusto = c;
        }
        foreach (Component c in textos)
        {
            if (txtNome == null && c != txtCusto) txtNome = c;
            else if (txtCusto == null && c != txtNome) txtCusto = c;
        }

        if (txtNome != null) SetTexto(txtNome, nome);
        if (txtCusto != null) SetTexto(txtCusto, custoTexto);
    }

    private static void SetTexto(Component alvo, string valor)
    {
        if (alvo is TMP_Text tmp) tmp.text = valor;
        else if (alvo is Text legado) legado.text = valor;
    }
}