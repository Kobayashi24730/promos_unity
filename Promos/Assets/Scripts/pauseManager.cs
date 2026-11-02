using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class pauseManager: MonoBehaviour
{
    public GameObject canvasMenu; //? Painel principal do menu onde aparecerar a toast.
    public GameObject loadGamePrefab; //? prefab do painel do load.
    public Transform scrollViewLoad; //? Scroll Content referente do painel do laod (loadGamePrefab).
    public GameObject saveToast; //? Toast que aaprecerar quando o save for efetuado.

    public GameObject itenPrefab; //? iten que mostrara as infos do save para cada save;

    private readonly List<GameObject> itens = new List<GameObject>(); //? Lista para gurdar cada save feito pelo usuario.
    public void Start()
    {
        if (loadGamePrefab != null && loadGamePrefab.scene.IsValid()) loadGamePrefab.SetActive(false);
        if (saveToast != null && saveToast.scene.IsValid()) saveToast.SetActive(false);
        if (itenPrefab != null && itenPrefab.scene.IsValid()) itenPrefab.SetActive(false);
    }

    public void SalvarNoSlotBotao(int slotIndex)
    {
        SaveSystem.Salvar(slotIndex);
        StartCoroutine(mostrarToast());
        atualizarListaSave();
    }

    private IEnumerator mostrarToast()
    {
        if (saveToast != null)
        {
            saveToast.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            saveToast.SetActive(false);
        }
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

        bool temSave = SaveSystem.ExisteSave;
        int numberSaves = temSave ? 1 : 0;
        for (int i=0; i < 3; i++)
        {
            GameObject novoItem = Instantiate(itenPrefab, scrollViewLoad, false);
            novoItem.SetActive(true);
            itens.Add(novoItem);
            TMP_Text textoTitulo = novoItem.GetComponentInChildren<TMP_Text>();
            Button botaoItem = novoItem.GetComponent<Button>();
            int slotAtual = i;
            if (SaveSystem.ExisteSlot(slotAtual))
            {
                if (textoTitulo != null)
                {    
                    textoTitulo.text = $"Slot {slotAtual + 1}: Jogo Salvo";
                }
                else
                {
                    if (textoTitulo != null) {
                        textoTitulo.text = $"Slot {slotAtual + 1} - Vazio";
                    }
                }

                if (botaoItem != null)
                {
                    botaoItem.onClick.RemoveAllListeners();
                    botaoItem.onClick.AddListener(() => SalvarNoSlotBotao(slotAtual));
                }
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
