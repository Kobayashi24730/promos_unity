using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemMelhoriaData
{
    public string nomeItem = "Novo item";
    public int nivelAtual = 0;       // pontos já investidos neste item
    public int nivelMaximo = 5;

    [Tooltip("Categoria ou tipo específico deste item (ex: Espada, Machado, Cajado, Escudo...).")]
    public string categoriaItem = "Espada";

    [Tooltip("Ícone específico deste item (opcional: se deixares vazio, o script tenta associar pelo tipo).")]
    public Sprite iconeItem;
}

[Serializable]
public class AbaCategoriaData
{
    public string nomeCategoria = "Armas";
    public int pontosDisponiveisNaAba = 5;

    [Tooltip("Botão da aba (Armas, Armaduras...) na UI. O script liga o clique sozinho.")]
    public Button botaoAba;

    [Tooltip("Texto específico que mostra os pontos desta aba na UI. Aceita TextMeshPro ou Text normal.")]
    public Graphic textoPontosDaAba;

    public List<ItemMelhoriaData> itensDaAba = new List<ItemMelhoriaData>();

    // Item selecionado nesta aba (não aparece no Inspector)
    [NonSerialized] public int selecionado;
}

public class GerenciadorMelhoriasDinamico : MonoBehaviour
{
    [Header("Abas e seus itens (configure no Inspector)")]
    public List<AbaCategoriaData> categorias = new List<AbaCategoriaData>();

    [Header("Biblioteca Central de Ícones Automáticos")]
    [Tooltip("Associe aqui os sprites para que o script os encontre automaticamente pelo nome da categoria do item.")]
    public List<IconePorCategoria> bibliotecaIcones = new List<IconePorCategoria>();

    [Header("UI na cena")]
    [Tooltip("O objeto 'itens' onde os itens são criados.")]
    public Transform containerItens;
    [Tooltip("Molde do item (o objeto 'iten'). Pode estar na cena: o script o esconde e nunca o apaga.")]
    public GameObject prefabItemUI;
    [Tooltip("Botão que gasta 1 ponto no item selecionado.")]
    public Button botaoMelhorar;

    [Header("Nomes dos componentes dentro do item")]
    public string nomeTextoNome = "nameItem";
    public string nomeTextoPontos = "pontosItem";
    [Tooltip("Nome do GameObject filho no prefab que contém o componente Image do ícone.")]
    public string nomeObjetoIcone = "imgItem";

    [Header("Cores das Abas")]
    public Color corAbaNormal = new Color(1f, 1f, 1f, 0f); // Transparente/Apagado por padrão
    public Color corAbaAtiva = new Color(0.96f, 0.82f, 0.55f); // Dourado do Véu de Ferro

    [Header("Cores dos Itens")]
    public Color corItemNormal = new Color(1f, 1f, 1f, 0.2f);
    public Color corItemSelecionado = new Color(0.96f, 0.82f, 0.55f);

    private int indiceAbaAtual = -1;
    private int contadorNovosItens = 1;
    private readonly List<GameObject> itensGerados = new List<GameObject>();

    void Start()
    {
        if (categorias.Count == 0)
        {
            Debug.LogWarning("GerenciadorMelhoriasDinamico: a lista 'Categorias' está vazia.", this);
            return;
        }

        // Se o molde está na cena, esconde-o
        if (prefabItemUI != null && prefabItemUI.scene.IsValid())
            prefabItemUI.SetActive(false);

        // Liga o clique de cada aba pelo código
        for (int i = 0; i < categorias.Count; i++)
        {
            int indice = i;
            Button botao = categorias[i].botaoAba;
            if (botao == null) continue;

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

    public void SelecionarAbaArmas() => CarregarAba(0);
    public void SelecionarAbaArmaduras() => CarregarAba(1);

    public void CarregarAba(int indiceAba)
    {
        if (indiceAba < 0 || indiceAba >= categorias.Count) return;

        indiceAbaAtual = indiceAba;
        AtualizarCoresDasAbas();
        Redesenhar();
    }

    public void SelecionarItem(int indiceItem)
    {
        if (indiceAbaAtual < 0 || indiceAbaAtual >= categorias.Count) return;

        AbaCategoriaData aba = categorias[indiceAbaAtual];
        aba.selecionado = Mathf.Clamp(indiceItem, 0, Mathf.Max(0, aba.itensDaAba.Count - 1));
        Redesenhar();
    }

    public void MelhorarSelecionado()
    {
        if (indiceAbaAtual < 0 || indiceAbaAtual >= categorias.Count) return;

        AbaCategoriaData aba = categorias[indiceAbaAtual];
        if (aba.itensDaAba.Count == 0) return;

        int indexSeguro = Mathf.Clamp(aba.selecionado, 0, aba.itensDaAba.Count - 1);
        ItemMelhoriaData item = aba.itensDaAba[indexSeguro];

        if (aba.pontosDisponiveisNaAba <= 0)
        {
            Debug.Log("Sem pontos disponíveis na aba " + aba.nomeCategoria + ".");
            return;
        }

        if (item.nivelAtual >= item.nivelMaximo)
        {
            Debug.Log(item.nomeItem + " já está no nível máximo.");
            return;
        }

        aba.pontosDisponiveisNaAba--;
        item.nivelAtual++;

        SalvarProgresso();
        Redesenhar();
    }

    public void AdicionarItem(int indiceAba, string nome, string categoriaTipo = "Espada", int nivelMaximo = 5, Sprite icone = null)
    {
        if (indiceAba < 0 || indiceAba >= categorias.Count) return;

        categorias[indiceAba].itensDaAba.Add(new ItemMelhoriaData
        {
            nomeItem = nome,
            categoriaItem = categoriaTipo,
            nivelAtual = 0,
            nivelMaximo = nivelMaximo,
            iconeItem = icone
        });

        SalvarProgresso();
        if (indiceAba == indiceAbaAtual) Redesenhar();
    }

    public void AdicionarItemNaAbaAtiva()
    {
        if (indiceAbaAtual < 0) return;
        AdicionarItem(indiceAbaAtual, "Novo item " + contadorNovosItens++, "Espada", 5, null);
    }

    public void AdicionarPontos(int indiceAba, int quantidade)
    {
        if (indiceAba < 0 || indiceAba >= categorias.Count) return;

        categorias[indiceAba].pontosDisponiveisNaAba += quantidade;
        SalvarProgresso();
        if (indiceAba == indiceAbaAtual) AtualizarTextoPontos();
    }

    // Método que descobre o ícone automaticamente com base no nome/ID do item salvo
    private Sprite ObterIconeDoItem(string nomeDoItem)
    {
        // Podes normalizar o nome para minúsculas para evitar erros de digitação (ex: "Machado" vira "machado")
        string chave = nomeDoItem.ToLower().Trim();

        foreach (var entry in bibliotecaIcones)
        {
            if (entry.nomeCategoria.ToLower() == chave)
            {
                return entry.icone;
            }
        }
        
        return null; // Retorna nulo se não encontrar (ou podes colocar um ícone padrão de interrogação)
    }

    // ---------------------------- SAVE ----------------------------------

    private void AplicarSave()
    {
        if (SaveSystem.Atual == null || SaveSystem.Atual.abas == null) return;

        DadosDoJogo dados = SaveSystem.Atual;

        foreach (AbaCategoriaData aba in categorias)
        {
            // Encontra a aba correspondente no save
            AbaSalva salva = dados.abas.Find(a => a.nome == aba.nomeCategoria);
            if (salva == null) continue;

            aba.pontosDisponiveisNaAba = salva.pontos;

            // Opcional: Limpa a lista atual para recriar com base estrita no save, 
            // evitando duplicações ao recarregar a cena.
            aba.itensDaAba.Clear();

            foreach (ItemSalvo s in salva.itens)
            {
                // Validação para evitar itens sem nome no save
                if (string.IsNullOrEmpty(s.nome)) continue;

                // Cria e adiciona o item diretamente garantindo que puxa os dados corretos do JSON
                ItemMelhoriaData novoItem = new ItemMelhoriaData
                {
                    nomeItem = s.nome,
                    nivelAtual = s.nivel,
                    nivelMaximo = s.nivelMaximo,
                    categoriaItem = string.IsNullOrEmpty(s.categoria) ? "Espada" : s.categoria
                };
                
                aba.itensDaAba.Add(novoItem);
            }
        }
        
        // Redesenha a interface visual após atualizar os dados
        Redesenhar();
    }

    private void SalvarProgresso()
    {
        DadosDoJogo dados = SaveSystem.Atual;

        foreach (AbaCategoriaData aba in categorias)
        {
            AbaSalva salva = new AbaSalva
            {
                nome = aba.nomeCategoria,
                pontos = aba.pontosDisponiveisNaAba
            };

            foreach (ItemMelhoriaData item in aba.itensDaAba)
            {
                salva.itens.Add(new ItemSalvo
                {
                    nome = item.nomeItem,
                    nivel = item.nivelAtual,
                    nivelMaximo = item.nivelMaximo,
                    categoria = item.categoriaItem // <-- Guarda a categoria individual no JSON
                });
            }

            int idx = dados.abas.FindIndex(a => a.nome == aba.nomeCategoria);
            if (idx >= 0) dados.abas[idx] = salva;
            else dados.abas.Add(salva);
        }

        SaveSystem.Salvar();
    }

    // ---------------------------- UI ------------------------------------

    private void Redesenhar()
    {
        if (indiceAbaAtual < 0 || indiceAbaAtual >= categorias.Count) return;

        AbaCategoriaData aba = categorias[indiceAbaAtual];

        aba.selecionado = Mathf.Clamp(aba.selecionado, 0, Mathf.Max(0, aba.itensDaAba.Count - 1));

        AtualizarTextoPontos();

        foreach (GameObject go in itensGerados)
        {
            if (go != null)
            {
                go.SetActive(false);
                Destroy(go);
            }
        }
        itensGerados.Clear();

        if (prefabItemUI == null || containerItens == null) return;

        for (int i = 0; i < aba.itensDaAba.Count; i++)
        {
            int indiceItem = i;
            ItemMelhoriaData dado = aba.itensDaAba[i];

            GameObject ui = Instantiate(prefabItemUI, containerItens);
            ui.SetActive(true);
            itensGerados.Add(ui);

            PreencherTextos(ui.transform, dado.nomeItem, "Pontos: " + dado.nivelAtual + "/" + dado.nivelMaximo);

            // Configura o Ícone do item com base na categoria individual do item
            Transform tIcone = ui.transform.Find(nomeObjetoIcone);
            if (tIcone != null)
            {
                Image imgIcone = tIcone.GetComponent<Image>();
                if (imgIcone != null)
                {
                    // Prioridade 1: Ícone próprio do item. Prioridade 2: Ícone mapeado pela categoria do item.
                    Sprite spriteParaUsar = dado.iconeItem != null ? dado.iconeItem : ObterIconeDoItem(dado.categoriaItem);

                    if (spriteParaUsar != null)
                    {
                        imgIcone.gameObject.SetActive(true);
                        imgIcone.sprite = spriteParaUsar;
                    }
                    else
                    {
                        imgIcone.gameObject.SetActive(false);
                    }
                }
            }

            Graphic fundo = ui.GetComponent<Graphic>();
            if (fundo != null)
                fundo.color = (i == aba.selecionado) ? corItemSelecionado : corItemNormal;

            Button botao = ui.GetComponent<Button>();
            if (botao == null) botao = ui.AddComponent<Button>();
            if (botao.targetGraphic == null && fundo != null) botao.targetGraphic = fundo;

            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => SelecionarItem(indiceItem));
        }
    }

    private void AtualizarCoresDasAbas()
    {
        for (int i = 0; i < categorias.Count; i++)
        {
            Button botao = categorias[i].botaoAba;
            if (botao != null)
            {
                ColorBlock cb = botao.colors;

                if (i == indiceAbaAtual)
                {
                    cb.normalColor = corAbaAtiva;
                    cb.highlightedColor = corAbaAtiva;
                }
                else
                {
                    cb.normalColor = corAbaNormal;
                    cb.highlightedColor = corAbaAtiva;
                }

                botao.colors = cb;
            }
        }
    }

    private void AtualizarTextoPontos()
    {
        if (indiceAbaAtual < 0 || indiceAbaAtual >= categorias.Count) return;

        AbaCategoriaData aba = categorias[indiceAbaAtual];
        
        if (aba.textoPontosDaAba != null)
        {
            SetTexto(aba.textoPontosDaAba, "" + aba.pontosDisponiveisNaAba);
        }
    }

    private void PreencherTextos(Transform raiz, string nome, string pontos)
    {
        List<Component> textos = new List<Component>();
        textos.AddRange(raiz.GetComponentsInChildren<TMP_Text>(true));
        textos.AddRange(raiz.GetComponentsInChildren<Text>(true));

        Component txtNome = null;
        Component txtPontos = null;

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
            SetTexto(txtNome, nome + " (" + pontos + ")");
        }
        else
        {
            SetTexto(txtNome, nome);
            SetTexto(txtPontos, pontos);
        }
    }

    private static void SetTexto(Component alvo, string valor)
    {
        if (alvo is TMP_Text tmp) tmp.text = valor;
        else if (alvo is Text legado) legado.text = valor;
    }
}

// Estrutura auxiliar para mapear categorias de itens aos respetivos ícones no Inspector
[Serializable]
public struct IconePorCategoria
{
    public string nomeCategoria; // Ex: "Espada", "Machado", "Cajado"
    public Sprite icone;        // O sprite correspondente
}