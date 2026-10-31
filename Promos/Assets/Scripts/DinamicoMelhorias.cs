using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemMelhoriaData
{
    public string nomeItem = "Novo item";
    public int nivelAtual = 0;
    public int nivelMaximo = 5;

    [Tooltip("Tipo do item (ex.: Espada, Machado, Cajado, Escudo...).")]
    public string categoriaItem = "Espada";

    [Tooltip("Ícone deste item. Se ficar vazio, usa a biblioteca de ícones pela categoria.")]
    public Sprite iconeItem;
}

[Serializable]
public class AbaCategoriaData
{
    [Tooltip("Precisa ser igual ao nome da aba no save: Armas, Armadura, Consumíveis, Materiais.")]
    public string nomeCategoria = "Armas";
    public int pontosDisponiveisNaAba = 5;
    public Button botaoAba;
    [Tooltip("Texto com os pontos desta aba.")]
    public Graphic textoPontosDaAba;
    public List<ItemMelhoriaData> itensDaAba = new List<ItemMelhoriaData>();

    [NonSerialized] public int selecionado;
}

[Serializable]
public struct IconePorCategoria
{
    public string nomeCategoria;
    public Sprite icone;
}

// GameObject: DinamicoMelhorias
// Abas (Armas, Armadura...), lista de itens e gasto de pontos de melhoria.
public class DinamicoMelhorias : MonoBehaviour
{
    [Header("Abas e itens (valores iniciais)")]
    public List<AbaCategoriaData> categorias = new List<AbaCategoriaData>();
    public List<IconePorCategoria> bibliotecaIcones = new List<IconePorCategoria>();

    [Header("UI")]
    public Transform containerItens;
    [Tooltip("Molde do item (o objeto 'iten'). O script o esconde e nunca o apaga.")]
    public GameObject prefabItemUI;
    public Button botaoMelhorar;
    [Tooltip("Prefab 'tecla': aparece dentro de cada item quando a aba tem pontos.")]
    public GameObject prefabTeclaUI;

    [Header("Nomes dos objetos dentro do item")]
    public string nomeTextoNome = "nameItem";
    public string nomeTextoPontos = "pontosItem";
    public string nomeObjetoIcone = "imgItem";

    [Header("Cores")]
    public Color corAbaNormal = new Color(1f, 1f, 1f, 0f);
    public Color corAbaAtiva = new Color(0.96f, 0.82f, 0.55f);
    public Color corItemNormal = new Color(1f, 1f, 1f, 0.2f);
    public Color corItemSelecionado = new Color(0.96f, 0.82f, 0.55f);

    private int abaAtual = -1;
    private int contadorNovosItens = 1;
    private readonly List<GameObject> itensGerados = new List<GameObject>();

    void Start()
    {
        if (categorias.Count == 0)
        {
            Debug.LogWarning("DinamicoMelhorias: a lista 'Categorias' está vazia.", this);
            return;
        }

        UIUtil.EsconderSeEstiverNaCena(prefabItemUI);
        UIUtil.EsconderSeEstiverNaCena(prefabTeclaUI);

        for (int i = 0; i < categorias.Count; i++)
        {
            int indice = i;
            Button botao = categorias[i].botaoAba;
            if (botao == null)
            {
                Debug.LogWarning("A categoria '" + categorias[i].nomeCategoria + "' está sem Botao Aba.", this);
                continue;
            }
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => CarregarAba(indice));
        }

        if (botaoMelhorar != null)
        {
            botaoMelhorar.onClick.RemoveAllListeners();
            botaoMelhorar.onClick.AddListener(MelhorarSelecionado);
        }

        AplicarSave();
        CarregarAba(0);
    }

    // ------------------------- ações -------------------------

    public void CarregarAba(int indice)
    {
        if (indice < 0 || indice >= categorias.Count) return;

        abaAtual = indice;
        AtualizarCoresDasAbas();
        Redesenhar();
    }

    public void SelecionarItem(int indice)
    {
        if (!AbaValida()) return;

        AbaCategoriaData aba = categorias[abaAtual];
        aba.selecionado = Mathf.Clamp(indice, 0, Mathf.Max(0, aba.itensDaAba.Count - 1));
        Redesenhar();
    }

    // Gasta 1 ponto da aba no item selecionado
    public void MelhorarSelecionado()
    {
        if (!AbaValida()) return;

        AbaCategoriaData aba = categorias[abaAtual];
        if (aba.itensDaAba.Count == 0) return;

        ItemMelhoriaData item = aba.itensDaAba[Mathf.Clamp(aba.selecionado, 0, aba.itensDaAba.Count - 1)];

        if (aba.pontosDisponiveisNaAba <= 0) { Debug.Log("Sem pontos na aba " + aba.nomeCategoria + "."); return; }
        if (item.nivelAtual >= item.nivelMaximo) { Debug.Log(item.nomeItem + " já está no máximo."); return; }

        aba.pontosDisponiveisNaAba--;
        item.nivelAtual++;

        SalvarProgresso();
        Redesenhar();
    }

    public void AdicionarItem(int indiceAba, string nome, string categoria = "Espada", int nivelMaximo = 5, Sprite icone = null)
    {
        if (indiceAba < 0 || indiceAba >= categorias.Count) return;

        categorias[indiceAba].itensDaAba.Add(new ItemMelhoriaData
        {
            nomeItem = nome,
            categoriaItem = categoria,
            nivelMaximo = nivelMaximo,
            iconeItem = icone
        });

        SalvarProgresso();
        if (indiceAba == abaAtual) Redesenhar();
    }

    public void AdicionarItemNaAbaAtiva()
    {
        if (abaAtual >= 0) AdicionarItem(abaAtual, "Novo item " + contadorNovosItens++);
    }

    public void AdicionarPontos(int indiceAba, int quantidade)
    {
        if (indiceAba < 0 || indiceAba >= categorias.Count) return;

        categorias[indiceAba].pontosDisponiveisNaAba += quantidade;
        SalvarProgresso();
        if (indiceAba == abaAtual) Redesenhar();
    }

    // ------------------------- save -------------------------

    // Copia o save para as categorias da cena (mantém os itens e ícones do Inspector)
    private void AplicarSave()
    {
        DadosDoJogo dados = SaveSystem.Atual;

        foreach (AbaCategoriaData aba in categorias)
        {
            AbaSalva salva = dados.abas.Find(a => a.nome == aba.nomeCategoria);
            if (salva == null) continue;

            aba.pontosDisponiveisNaAba = salva.pontos;

            foreach (ItemSalvo s in salva.itens)
            {
                if (string.IsNullOrEmpty(s.nome)) continue;

                ItemMelhoriaData item = aba.itensDaAba.Find(i => i.nomeItem == s.nome);
                if (item == null)
                {
                    aba.itensDaAba.Add(new ItemMelhoriaData
                    {
                        nomeItem = s.nome,
                        nivelAtual = s.nivel,
                        nivelMaximo = s.nivelMaximo,
                        categoriaItem = string.IsNullOrEmpty(s.categoria) ? "Espada" : s.categoria
                    });
                }
                else
                {
                    item.nivelAtual = Mathf.Min(s.nivel, item.nivelMaximo);
                }
            }
        }
    }

    private void SalvarProgresso()
    {
        DadosDoJogo dados = SaveSystem.Atual;

        foreach (AbaCategoriaData aba in categorias)
        {
            AbaSalva salva = new AbaSalva { nome = aba.nomeCategoria, pontos = aba.pontosDisponiveisNaAba };

            foreach (ItemMelhoriaData item in aba.itensDaAba)
            {
                salva.itens.Add(new ItemSalvo
                {
                    nome = item.nomeItem,
                    nivel = item.nivelAtual,
                    nivelMaximo = item.nivelMaximo,
                    categoria = item.categoriaItem
                });
            }

            int idx = dados.abas.FindIndex(a => a.nome == aba.nomeCategoria);
            if (idx >= 0) dados.abas[idx] = salva;
            else dados.abas.Add(salva);
        }

        SaveSystem.Salvar();
    }

    // ------------------------- UI -------------------------

    private bool AbaValida() { return abaAtual >= 0 && abaAtual < categorias.Count; }

    private void Redesenhar()
    {
        if (!AbaValida()) return;

        AbaCategoriaData aba = categorias[abaAtual];
        aba.selecionado = Mathf.Clamp(aba.selecionado, 0, Mathf.Max(0, aba.itensDaAba.Count - 1));

        if (aba.textoPontosDaAba != null)
            UIUtil.SetTexto(aba.textoPontosDaAba, aba.pontosDisponiveisNaAba.ToString());

        // apaga só as cópias criadas (nunca o molde)
        foreach (GameObject go in itensGerados)
        {
            if (go == null) continue;
            go.SetActive(false);
            Destroy(go);
        }
        itensGerados.Clear();

        if (prefabItemUI == null || containerItens == null) return;

        for (int i = 0; i < aba.itensDaAba.Count; i++)
        {
            int indice = i;
            GameObject ui = CriarItemUI(aba.itensDaAba[i], i == aba.selecionado, aba.pontosDisponiveisNaAba > 0);
            UIUtil.TornarClicavel(ui, () => SelecionarItem(indice));
            itensGerados.Add(ui);
        }
    }

    private GameObject CriarItemUI(ItemMelhoriaData dado, bool selecionado, bool abaTemPontos)
    {
        GameObject ui = Instantiate(prefabItemUI, containerItens);
        ui.SetActive(true);

        PreencherTextos(ui.transform, dado.nomeItem, "Pontos: " + dado.nivelAtual + "/" + dado.nivelMaximo);
        AplicarIcone(ui.transform, dado);
        AtualizarTeclaDoItem(ui.transform, abaTemPontos);

        Graphic fundo = ui.GetComponent<Graphic>();
        if (fundo != null) fundo.color = selecionado ? corItemSelecionado : corItemNormal;

        return ui;
    }

    private void AplicarIcone(Transform item, ItemMelhoriaData dado)
    {
        Transform t = item.Find(nomeObjetoIcone);
        if (t == null) return;

        Image img = t.GetComponent<Image>();
        if (img == null) return;

        Sprite sprite = dado.iconeItem != null ? dado.iconeItem : IconeDaCategoria(dado.categoriaItem);
        img.gameObject.SetActive(sprite != null);
        if (sprite != null) img.sprite = sprite;
    }

    private Sprite IconeDaCategoria(string categoria)
    {
        if (string.IsNullOrEmpty(categoria)) return null;

        string chave = categoria.ToLower().Trim();
        foreach (IconePorCategoria e in bibliotecaIcones)
        {
            if (!string.IsNullOrEmpty(e.nomeCategoria) && e.nomeCategoria.ToLower().Trim() == chave)
                return e.icone;
        }
        return null;
    }

    private void AtualizarTeclaDoItem(Transform item, bool mostrar)
    {
        if (prefabTeclaUI == null) return;

        Transform existente = item.Find("tecla");
        if (existente != null)
        {
            existente.gameObject.SetActive(mostrar);
        }
        else if (mostrar)
        {
            GameObject tecla = Instantiate(prefabTeclaUI, item);
            tecla.name = "tecla";
            tecla.SetActive(true);
        }
    }

    private void AtualizarCoresDasAbas()
    {
        for (int i = 0; i < categorias.Count; i++)
        {
            Button botao = categorias[i].botaoAba;
            if (botao == null) continue;

            ColorBlock cb = botao.colors;
            cb.normalColor = (i == abaAtual) ? corAbaAtiva : corAbaNormal;
            cb.highlightedColor = corAbaAtiva;
            botao.colors = cb;
        }
    }

    // Escreve nome e pontos nos textos do item (por nome do objeto, ou pela ordem)
    private void PreencherTextos(Transform raiz, string nome, string pontos)
    {
        List<Component> textos = new List<Component>();
        textos.AddRange(raiz.GetComponentsInChildren<TMPro.TMP_Text>(true));
        textos.AddRange(raiz.GetComponentsInChildren<Text>(true));

        Component txtNome = null, txtPontos = null;

        foreach (Component c in textos)
        {
            if (c.gameObject.name == nomeTextoNome) txtNome = c;
            else if (c.gameObject.name == nomeTextoPontos) txtPontos = c;
        }
        foreach (Component c in textos)
        {
            if (txtNome == null && c != txtPontos) txtNome = c;
            else if (txtPontos == null && c != txtNome) txtPontos = c;
        }

        if (txtNome == null) return;

        if (txtPontos == null)
        {
            UIUtil.SetTexto(txtNome, nome + " (" + pontos + ")");
        }
        else
        {
            UIUtil.SetTexto(txtNome, nome);
            UIUtil.SetTexto(txtPontos, pontos);
        }
    }
}