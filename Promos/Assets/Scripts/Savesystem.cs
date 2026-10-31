using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// =====================================================================
//  ESTRUTURA DO SAVE  (os nomes dos campos são as chaves do save.json)
// =====================================================================

[Serializable]
public class DadosPlayerData
{
    public string nome = "";
    public string classe = "";
    public float tempoDeJogo = 0f;          // em segundos
    public int moedas = 0;
    public int quantidadeMortes = 0;
    public string dificuldade = "Normal";
    public Vector3 ultimaPosicao = Vector3.zero;   // salvo como {x, y, z}
}

[Serializable]
public class ProgressoData
{
    public List<string> missoesFeitas = new List<string>();
    public List<string> missoesAtivas = new List<string>();
    public List<string> areasExploradas = new List<string>();
}

// Atributo personalizado (fora de Vida/Stamina/Vigor), para você criar outros sem mexer na estrutura
[Serializable]
public class AtributoExtra
{
    public string nome;
    public int valor;
}

[Serializable]
public class AtributosData
{
    public int pontosStatusDisponiveis = 2;
    public float vidaMaxima = 100f;
    public float vidaAtual = 100f;
    public float vigor = 50f;
    public float staminaMaxima = 100f;
    public float staminaAtual = 100f;
    public List<AtributoExtra> extras = new List<AtributoExtra>();
}

[Serializable]
public class HabilidadesData
{
    public int pontosHabilidadeDisponiveis = 0;

    public List<string> habilidadesDesbloqueadas =
        new List<string>();

    public List<HabilidadeNivelData> niveis =
        new List<HabilidadeNivelData>();
}
[Serializable]
public class ItemSalvo
{
    public string nome;
    public int nivel;
    public int nivelMaximo;
    public string categoria;
}

[Serializable]
public class HabilidadeNivelData
{
    public string id;
    public int nivel;
}

[Serializable]
public class AbaSalva
{
    public string nome;
    public int pontos;
    public List<ItemSalvo> itens = new List<ItemSalvo>();
}

[Serializable]
public class DadosDoJogo
{
    // Número da versão do formato. Serve para migrar saves antigos no futuro.
    public int versao = 1;

    public DadosPlayerData dadosPlayer = new DadosPlayerData();
    public ProgressoData progresso = new ProgressoData();
    public AtributosData atributos = new AtributosData();
    public HabilidadesData habilidades = new HabilidadesData();
    public List<AbaSalva> abas = CriarAbasPadrao();

    // Nomes das abas de melhoria. O 'Nome Categoria' de cada aba no Inspector
    // do gerenciador precisa ser IGUAL a estes (inclusive acentos).
    public static readonly string[] NomesDasAbas = { "Armas", "Armadura", "Consumíveis", "Materiais" };

    public static List<AbaSalva> CriarAbasPadrao()
    {
        List<AbaSalva> lista = new List<AbaSalva>();
        foreach (string nome in NomesDasAbas)
            lista.Add(new AbaSalva { nome = nome, pontos = 0 });
        return lista;
    }
}

// =====================================================================
//  SALVAR / CARREGAR  (não precisa estar em nenhuma cena)
// =====================================================================

public static class SaveSystem
{
    private const string NomeArquivo = "save.json";
    private static DadosDoJogo _atual;

    public static string CaminhoDoArquivo => Path.Combine(Application.persistentDataPath, NomeArquivo);
    private static string CaminhoDoBackup => CaminhoDoArquivo + ".bak";
    private static string CaminhoTemporario => CaminhoDoArquivo + ".tmp";

    public static bool ExisteSave => File.Exists(CaminhoDoArquivo) || File.Exists(CaminhoDoBackup);

    // Os dados em memória. Todas as cenas leem e escrevem aqui.
    public static DadosDoJogo Atual
    {
        get
        {
            if (_atual == null)
            {
                if (!Carregar()) _atual = Normalizar(null);
            }
            return _atual;
        }
    }

    // Completa o que faltar (acontece com saves antigos ou editados à mão)
    private static DadosDoJogo Normalizar(DadosDoJogo d)
    {
        if (d == null) d = new DadosDoJogo();

        if (d.dadosPlayer == null) d.dadosPlayer = new DadosPlayerData();
        if (string.IsNullOrEmpty(d.dadosPlayer.dificuldade)) d.dadosPlayer.dificuldade = "Normal";

        if (d.progresso == null) d.progresso = new ProgressoData();
        if (d.progresso.missoesFeitas == null) d.progresso.missoesFeitas = new List<string>();
        if (d.progresso.missoesAtivas == null) d.progresso.missoesAtivas = new List<string>();
        if (d.progresso.areasExploradas == null) d.progresso.areasExploradas = new List<string>();

        if (d.atributos == null) d.atributos = new AtributosData();
        if (d.atributos.extras == null) d.atributos.extras = new List<AtributoExtra>();
        d.atributos.pontosStatusDisponiveis = Mathf.Max(0, d.atributos.pontosStatusDisponiveis);
        d.atributos.vidaMaxima = Mathf.Max(1f, d.atributos.vidaMaxima);
        d.atributos.vidaAtual = Mathf.Clamp(d.atributos.vidaAtual, 0f, d.atributos.vidaMaxima);
        d.atributos.staminaMaxima = Mathf.Max(1f, d.atributos.staminaMaxima);
        d.atributos.staminaAtual = Mathf.Clamp(d.atributos.staminaAtual, 0f, d.atributos.staminaMaxima);

        if (d.habilidades == null) d.habilidades = new HabilidadesData();
        if (d.habilidades.habilidadesDesbloqueadas == null) d.habilidades.habilidadesDesbloqueadas = new List<string>();

        if (d.abas == null) d.abas = new List<AbaSalva>();
        foreach (string nome in DadosDoJogo.NomesDasAbas)
        {
            if (!d.abas.Exists(a => a != null && a.nome == nome))
                d.abas.Add(new AbaSalva { nome = nome, pontos = 0 });
        }
        d.abas.RemoveAll(a => a == null);
        foreach (AbaSalva aba in d.abas)
        {
            if (aba.itens == null) aba.itens = new List<ItemSalvo>();
        }

        return d;
    }

    // ------------------------- arquivo ---------------------------

    // Botão "Novo jogo": começa do zero e já grava o arquivo
    public static void NovoJogo()
    {
        _atual = Normalizar(null);
        Salvar();
    }

    // Botão "Continuar". Se o arquivo principal estiver corrompido, usa o backup.
    public static bool Carregar()
    {
        if (TentarLer(CaminhoDoArquivo)) return true;

        if (TentarLer(CaminhoDoBackup))
        {
            Debug.LogWarning("O save principal estava inválido: carregado do backup.");
            return true;
        }

        return false;
    }

    private static bool TentarLer(string caminho)
    {
        if (!File.Exists(caminho)) return false;

        try
        {
            string json = File.ReadAllText(caminho);
            DadosDoJogo dados = JsonUtility.FromJson<DadosDoJogo>(json);
            if (dados == null) return false;

            _atual = Normalizar(dados);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Falha ao ler " + Path.GetFileName(caminho) + ": " + e.Message);
            return false;
        }
    }

    // Grava num arquivo temporário e só então troca o principal (um travamento no meio
    // da gravação não corrompe o save). O save anterior fica guardado como .bak.
    public static void Salvar()
    {
        try
        {
            string json = JsonUtility.ToJson(Atual, true);

            File.WriteAllText(CaminhoTemporario, json);

            if (File.Exists(CaminhoDoArquivo))
            {
                File.Copy(CaminhoDoArquivo, CaminhoDoBackup, true);
                File.Delete(CaminhoDoArquivo);
            }

            File.Move(CaminhoTemporario, CaminhoDoArquivo);
        }
        catch (Exception e)
        {
            Debug.LogError("Falha ao salvar: " + e.Message);
        }
    }

    public static void ApagarSave()
    {
        _atual = null;
        foreach (string caminho in new[] { CaminhoDoArquivo, CaminhoDoBackup, CaminhoTemporario })
        {
            if (File.Exists(caminho)) File.Delete(caminho);
        }
    }

    // ---------------- melhorias (abas e itens) ----------------

    // Nível de um item pelo nome exato. Ex.: SaveSystem.GetNivel("Espada")
    public static int GetNivel(string nomeItem)
    {
        foreach (AbaSalva aba in Atual.abas)
            foreach (ItemSalvo item in aba.itens)
                if (item.nome == nomeItem) return item.nivel;
        return 0;
    }

    // Pontos que sobraram numa aba. Ex.: SaveSystem.GetPontos("Armas")
    public static int GetPontos(string nomeAba)
    {
        foreach (AbaSalva aba in Atual.abas)
            if (aba.nome == nomeAba) return aba.pontos;
        return 0;
    }

    // ---------------- atributos do player ----------------

    private static string Chave(string nome)
    {
        return (nome ?? "").Trim().ToLowerInvariant();
    }

    // Valor "de nível" de um atributo, usado pelas barras do menu:
    //   Vida -> vidaMaxima | Stamina ou Magia -> staminaMaxima | Vigor -> vigor
    // Qualquer outro nome é guardado na lista 'extras'.
    public static int GetAtributo(string nome, int valorPadrao = 0)
    {
        AtributosData a = Atual.atributos;

        switch (Chave(nome))
        {
            case "vida": return Mathf.RoundToInt(a.vidaMaxima);
            case "stamina":
            case "magia": return Mathf.RoundToInt(a.staminaMaxima);
            case "vigor": return Mathf.RoundToInt(a.vigor);
        }

        string chave = Chave(nome);
        foreach (AtributoExtra e in a.extras)
            if (Chave(e.nome) == chave) return e.valor;

        return valorPadrao;
    }

    public static void SetAtributo(string nome, int valor)
    {
        AtributosData a = Atual.atributos;

        switch (Chave(nome))
        {
            case "vida":
            {
                float aumento = Mathf.Max(0f, valor - a.vidaMaxima);
                a.vidaMaxima = valor;
                a.vidaAtual = Mathf.Clamp(a.vidaAtual + aumento, 0f, a.vidaMaxima);
                return;
            }
            case "stamina":
            case "magia":
            {
                float aumento = Mathf.Max(0f, valor - a.staminaMaxima);
                a.staminaMaxima = valor;
                a.staminaAtual = Mathf.Clamp(a.staminaAtual + aumento, 0f, a.staminaMaxima);
                return;
            }
            case "vigor":
                a.vigor = valor;
                return;
        }

        string chave = Chave(nome);
        foreach (AtributoExtra e in a.extras)
        {
            if (Chave(e.nome) == chave) { e.valor = valor; return; }
        }
        a.extras.Add(new AtributoExtra { nome = nome, valor = valor });
    }

    public static int PontosStatus => Atual.atributos.pontosStatusDisponiveis;

    // ---------------- dados do player ----------------

    public static void AdicionarTempoDeJogo(float segundos) { Atual.dadosPlayer.tempoDeJogo += segundos; }

    public static void AdicionarMoedas(int quantidade)
    {
        Atual.dadosPlayer.moedas = Mathf.Max(0, Atual.dadosPlayer.moedas + quantidade);
    }

    public static void RegistrarMorte() { Atual.dadosPlayer.quantidadeMortes++; }

    public static void SetUltimaPosicao(Vector3 posicao) { Atual.dadosPlayer.ultimaPosicao = posicao; }
    public static Vector3 GetUltimaPosicao() { return Atual.dadosPlayer.ultimaPosicao; }

    // ---------------- progresso ----------------

    public static void IniciarMissao(string id)
    {
        if (MissaoFeita(id)) return;
        AdicionarSemRepetir(Atual.progresso.missoesAtivas, id);
    }

    public static void ConcluirMissao(string id)
    {
        Atual.progresso.missoesAtivas.Remove(id);
        AdicionarSemRepetir(Atual.progresso.missoesFeitas, id);
    }

    public static bool MissaoFeita(string id) { return Atual.progresso.missoesFeitas.Contains(id); }
    public static bool MissaoAtiva(string id) { return Atual.progresso.missoesAtivas.Contains(id); }

    public static void ExplorarArea(string id) { AdicionarSemRepetir(Atual.progresso.areasExploradas, id); }
    public static bool AreaExplorada(string id) { return Atual.progresso.areasExploradas.Contains(id); }

    // ---------------- habilidades ----------------

    public static void DesbloquearHabilidade(string id)
    {
        AdicionarSemRepetir(Atual.habilidades.habilidadesDesbloqueadas, id);
    }

    public static bool HabilidadeDesbloqueada(string id)
    {
        return Atual.habilidades.habilidadesDesbloqueadas.Contains(id);
    }

    private static void AdicionarSemRepetir(List<string> lista, string id)
    {
        if (!string.IsNullOrEmpty(id) && !lista.Contains(id)) lista.Add(id);
    }

    public static int GetNivelHabilidade(string id)
    {
        var dados = Atual.habilidades;

        var habilidade = dados.niveis.Find(h => h.id == id);

        return habilidade != null ? habilidade.nivel : 0;
    }

    public static void SetNivelHabilidade(string id, int nivel)
    {
        var dados = Atual.habilidades;

        var habilidade = dados.niveis.Find(h => h.id == id);

        if (habilidade == null)
        {
            habilidade = new HabilidadeNivelData
            {
                id = id,
                nivel = 0
            };

            dados.niveis.Add(habilidade);
        }

        habilidade.nivel = Mathf.Max(0, nivel);
    }
}


