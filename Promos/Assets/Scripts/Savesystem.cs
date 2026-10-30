using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ---------- Estrutura do que é salvo (vira o arquivo save.json) ----------

[Serializable]
public class ItemSalvo
{
    public string nome;
    public int nivel;
    public int nivelMaximo;
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
    public List<AbaSalva> abas = new List<AbaSalva>();

    // Mais para frente você pode acrescentar aqui, por exemplo:
    // public Vector3 posicaoDoPlayer;
    // public List<string> inventario = new List<string>();
}

// ---------- Salvar / carregar (não precisa estar em nenhuma cena) ----------

public static class SaveSystem
{
    private const string NomeArquivo = "save.json";
    private static DadosDoJogo _atual;

    public static string CaminhoDoArquivo => Path.Combine(Application.persistentDataPath, NomeArquivo);
    public static bool ExisteSave => File.Exists(CaminhoDoArquivo);

    // Os dados em memória. Todas as cenas leem e escrevem aqui.
    // Se ainda não foram carregados, tenta ler o arquivo; se não houver, cria dados vazios.
    public static DadosDoJogo Atual
    {
        get
        {
            if (_atual == null)
            {
                if (!Carregar()) _atual = new DadosDoJogo();
            }
            return _atual;
        }
    }

    // Botão "Novo jogo": começa do zero e já grava o arquivo
    public static void NovoJogo()
    {
        _atual = new DadosDoJogo();
        Salvar();
    }

    // Botão "Continuar": lê o arquivo. Retorna false se não existir ou estiver corrompido.
    public static bool Carregar()
    {
        if (!ExisteSave) return false;

        try
        {
            string json = File.ReadAllText(CaminhoDoArquivo);
            DadosDoJogo dados = JsonUtility.FromJson<DadosDoJogo>(json);
            if (dados == null) return false;

            _atual = dados;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Falha ao carregar o save: " + e.Message);
            return false;
        }
    }

    // Botão "Salvar game" (e chamado automaticamente quando você melhora um item)
    public static void Salvar()
    {
        try
        {
            File.WriteAllText(CaminhoDoArquivo, JsonUtility.ToJson(Atual, true));
        }
        catch (Exception e)
        {
            Debug.LogError("Falha ao salvar: " + e.Message);
        }
    }

    public static void ApagarSave()
    {
        _atual = null;
        if (ExisteSave) File.Delete(CaminhoDoArquivo);
    }

    // ---------- Consultas para o player e o resto do jogo ----------

    // Nível atual de um item pelo nome exato. Ex.: SaveSystem.GetNivel("Espada")
    public static int GetNivel(string nomeItem)
    {
        foreach (AbaSalva aba in Atual.abas)
            foreach (ItemSalvo item in aba.itens)
                if (item.nome == nomeItem) return item.nivel;
        return 0;
    }

    // Pontos que sobraram em uma aba. Ex.: SaveSystem.GetPontos("Armas")
    public static int GetPontos(string nomeAba)
    {
        foreach (AbaSalva aba in Atual.abas)
            if (aba.nome == nomeAba) return aba.pontos;
        return 0;
    }
}