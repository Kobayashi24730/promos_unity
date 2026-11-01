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

    [Tooltip("Categoria ou Aba correspondente (ex: Vida, Magica, Armas, Necromancer).")]
    public string categoria = "Vida";

    [Tooltip("Id de OUTRA habilidade que precisa estar desbloqueada antes. Deixe VAZIO se não houver.")]
    public string requer = "";

    [Tooltip("Custo da primeira compra.")]
    public int custoInicial = 3;

    [Tooltip("Quanto o custo aumenta a cada nível.")]
    public int aumentoCusto = 2;

    [Tooltip("Nível máximo. 0 = ilimitado.")]
    public int nivelMaximo = 0;

    [Tooltip("Executa só na primeira compra.")]
    public UnityEvent aoDesbloquear;

    [Tooltip("Executa a cada nível comprado.")]
    public UnityEvent aoMelhorar;

    [NonSerialized] public Button botaoGerado;
}

[Serializable]
public class CategoriaContainer
{
    [Tooltip("Nome exato da categoria (ex: Vida, Magica, Armas, Necromancer).")]
    public string nomeCategoria;
    
    [Tooltip("O Content correspondente à Scroll View desta categoria.")]
    public Transform containerItens;

    [Tooltip("Prefab específico para os itens desta categoria (deve conter um filho chamado 'tecla').")]
    public GameObject prefabEspecifico;
}

public class DinamicoHabilidades : MonoBehaviour
{
    [Header("Pontos")]
    public Graphic textoPontosHabilidade;
    [Tooltip("{0} vira o número de pontos.")]
    public string formatoPontos = "PONTOS DE HABILIDADE {0}";

    [Header("Containers e Prefabs por Categoria")]
    [Tooltip("Configure o Content e o Prefab específico para cada categoria/scroll.")]
    public List<CategoriaContainer> containersCategorias = new List<CategoriaContainer>();

    [Header("Nomes dos objetos no prefab")]
    public string nomeTextoNome = "nameItem";
    public string nomeTextoCusto = "pontosItem";
    public string nomeObjetoTecla = "tecla";

    [Header("Habilidades")]
    public List<HabilidadeUI> habilidades = new List<HabilidadeUI>();

    private readonly List<GameObject> itensGerados = new List<GameObject>();

    void Start()
    {
        // Desativa todos os prefabs configurados nas categorias para servirem apenas de molde
        foreach (var cat in containersCategorias)
        {
            if (cat.prefabEspecifico != null && cat.prefabEspecifico.scene.IsValid())
                cat.prefabEspecifico.SetActive(false);
        }

        ValidarConfiguracao();
        Atualizar();
    }

    private void ValidarConfiguracao()
    {
        if (habilidades.Count == 0)
            Debug.LogWarning("DinamicoHabilidades: a lista 'Habilidades' está vazia.", this);

        HashSet<string> ids = new HashSet<string>();
        foreach (HabilidadeUI hab in habilidades)
        {
            if (string.IsNullOrEmpty(hab.id) || !ids.Add(hab.id))
                Debug.LogWarning("Habilidade com id vazio ou repetido: '" + hab.id + "'.", this);

            if (!string.IsNullOrEmpty(hab.requer) && !habilidades.Exists(o => o.id == hab.requer))
                Debug.LogWarning("Habilidade '" + hab.id + "': Requer ('" + hab.requer + "') não é o id de nenhuma habilidade.", this);

            CategoriaContainer c = ObterDadosCategoria(hab.categoria);
            if (c == null || c.containerItens == null || c.prefabEspecifico == null)
                Debug.LogWarning("Habilidade '" + hab.id + "': a categoria '" + hab.categoria + "' não existe ou está sem Container/Prefab.", this);
        }
    }

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

    public void AdicionarPontosHabilidade(int quantidade)
    {
        SaveSystem.Atual.habilidades.pontosHabilidadeDisponiveis += quantidade;
        SaveSystem.Salvar();
        Atualizar();
    }

    [ContextMenu("Teste: dar 3 pontos de habilidade")]
    private void TesteDarPontos() { AdicionarPontosHabilidade(3); }

    private bool PodeMelhorar(HabilidadeUI hab, int pontos)
    {
        bool jaTem = SaveSystem.HabilidadeDesbloqueada(hab.id);
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

    public void Atualizar()
    {
        int pontos = SaveSystem.Atual.habilidades.pontosHabilidadeDisponiveis;

        if (textoPontosHabilidade != null)
            SetTexto(textoPontosHabilidade, string.Format(formatoPontos, pontos));

        LimparItens();

        // Cria as habilidades separando-as por categoria nos respetivos containers e com o prefab próprio
        foreach (HabilidadeUI hab in habilidades)
        {
            CategoriaContainer dadosCat = ObterDadosCategoria(hab.categoria);
            if (dadosCat != null && dadosCat.containerItens != null && dadosCat.prefabEspecifico != null)
            {
                CriarItemNoContainer(hab, pontos, dadosCat.containerItens, dadosCat.prefabEspecifico);
            }
        }

        // Reconstrói o layout de todos os containers ativos
        foreach (var cat in containersCategorias)
        {
            if (cat.containerItens != null)
            {
                RectTransform rt = cat.containerItens as RectTransform;
                if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }
        
        Debug.Log("[Habilidades] configuradas: " + habilidades.Count + " | itens criados: " + itensGerados.Count);
    }

    private CategoriaContainer ObterDadosCategoria(string categoria)
    {
        return containersCategorias.Find(c =>
            string.Equals(c.nomeCategoria, categoria, StringComparison.OrdinalIgnoreCase));
    }

    private void LimparItens()
    {
        foreach (GameObject go in itensGerados)
        {
            if (go == null) continue;
            go.SetActive(false);
            Destroy(go);
        }
        itensGerados.Clear();
    }

    private void CriarItemNoContainer(HabilidadeUI hab, int pontos, Transform containerPai, GameObject prefabAlvo)
    {
        GameObject ui = Instantiate(prefabAlvo, containerPai, false);
        ui.SetActive(true);
        itensGerados.Add(ui);

        int nivel = SaveSystem.GetNivelHabilidade(hab.id);
        bool podeComprar = PodeMelhorar(hab, pontos);

        PreencherTextos(
            ui.transform,
            hab.nomeHabilidade + " (Nv. " + nivel + ")",
            NoMaximo(hab) ? "MÁXIMO" : "Custo: " + CalcularCusto(hab));

        Button botaoItem = ui.GetComponent<Button>();
        if (botaoItem == null) botaoItem = ui.GetComponentInChildren<Button>();
        if (botaoItem != null)
        {
            botaoItem.onClick.RemoveAllListeners();
            botaoItem.interactable = false; 
        }

        Transform tTecla = ui.transform.Find(nomeObjetoTecla);
        if (tTecla != null)
        {
            tTecla.gameObject.SetActive(podeComprar);

            if (podeComprar)
            {
                Button botaoTecla = tTecla.GetComponent<Button>();
                if (botaoTecla == null)
                {
                    botaoTecla = tTecla.gameObject.AddComponent<Button>();
                }

                botaoTecla.interactable = true;
                
                HabilidadeUI atual = hab;
                botaoTecla.onClick.RemoveAllListeners();
                botaoTecla.onClick.AddListener(() => MelhorarHabilidade(atual));
            }
        }
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