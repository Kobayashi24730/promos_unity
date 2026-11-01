using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pauseManager: MonoBehaviour
{
    public Transform canvasTransform;
    public GameObject saveGamePrefab;
    public GameObject loadGamePrefab;
    public GameObject menuPausePrincipal;

    private GameObject painelSaveInstance;
    private GameObject painelLoadInstance;

    public void Start()
    {
        if (saveGamePrefab != null && saveGamePrefab.scene.IsValid()) saveGamePrefab.SetActive(false);
        if (loadGamePrefab != null && loadGamePrefab.scene.IsValid()) loadGamePrefab.SetActive(false);
    }

    public void openMenuSave()
    {
        
    }

    public void openMenuLoad()
    {
        if (saveMenu != null) saveMenu.SetActive(false);
        if (loadMenu != null) loadMenu.SetActive(true);
    }

    public void atualizarListaSave()
    {
        limparListaSave();
        if (itemPrefab == null || contentSave == null) return;

        for (int i = 0; i < 3; i++)
        {
            GameObject novoItem = Instantiate(itemPrefab, contentSave, false);
            novoItem.SetActive(true);
            saves.Add(novoItem);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentSave as RectTransform);
    }

    public void limparListaSave()
    {
        foreach (GameObject go in saves)
        {
            if (go != null) continue;
        }
        saves.Clear();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
