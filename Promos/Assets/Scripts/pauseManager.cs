using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class pauseManager: MonoBehaviour
{
    public GameObject canvasMenu; //? Painel principal do menu onde aparecerar a toast.
    public GameObject loadGamePrefab; //? prefab do painel do load.
    public Transform scrollViewLoad; //? Scroll Content referente do painel do laod (loadGamePrefab).

    public GameObject itenPrefab; //? iten que mostrara as infos do save para cada save;

    private readonly List<GameObject> itens = new List<GameObject>(); //? Lista para gurdar cada save feito pelo usuario.
    public void Start()
    {
        if (loadGamePrefab != null && loadGamePrefab.scene.IsValid()) loadGamePrefab.SetActive(false);
        if (itenPrefab != null && itenPrefab.scene.IsValid()) itenPrefab.SetActive(false);
    }

    public void openSave()
    {
        if (loadGamePrefab != null) loadGamePrefab.SetActive(false);
        desenharSave();
    }

    public void openMenuLoad()
    {
        if (loadGamePrefab != null) loadGamePrefab.SetActive(true);
        atualizarListaSave();
    }

    public void closeMenuLoad() {if (loadGamePrefab != null) loadGamePrefab.SetActive(false);}

    public void atualizarListaSave()
    {
        limparListaSave();
        if (scrollViewLoad == null || itenPrefab == null)
        {
            Debug.LogWarning("Content ou Prefab do item não atribuídos!");
            return;
        }

        int nuberSaves = 3;
        for (int i=0; i < nuberSaves; i++)
        {
            GameObject novoItem = Instantiate(itenPrefab, scrollViewLoad, false);
            novoItem.SetActive(true);
            itens.Add(novoItem);
            TMP_Text textoTitulo = novoItem.GetComponentInChildren<TMP_Text>();
            if (textoTitulo != null)
            {
                textoTitulo.text = $"Save Slot {i + 1} - Jogo Salvo";
            }
        }

        RectTransform rt = scrollViewLoad as RectTransform;
        if (rt != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    public void limparListaSave()
    {
        foreach (GameObject iten in itens)
        {
            if (iten != null)
            {
                Destroy(iten);
            }
        }
        itens.Clear();
    }

    public void desenharSave()
    {
        
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
