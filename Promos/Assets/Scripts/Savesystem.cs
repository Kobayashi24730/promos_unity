using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ItemSalvo
{
    public string nome;
    public int nivel;
    public int nivelMaximo;
    public string categoria; 
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
}

public static class SaveSystem
{
    private const string NomeArquivo = "save.json";
    private static DadosDoJogo _atual;

    public static string CaminhoDoArquivo => Path.Combine(Application.persistentDataPath, NomeArquivo);
    public static bool ExisteSave => File.Exists(CaminhoDoArquivo);

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

    public static void NovoJogo()
    {
        _atual = new DadosDoJogo();
        Salvar();
    }

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
}