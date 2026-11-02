using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Playables;

public class pauseManager: MonoBehaviour
{
    public GameObject canvasMenu; //? Painel principal do menu onde aparecerar a toast.
    public GameObject loadGamePrefab; //? prefab do painel do load.
    public GameObject notificacao; //? prefab do toast;
    public Transform scrollViewLoad; //? Scroll Content referente do painel do laod (loadGamePrefab).
    public GameObject itenPrefab; //? iten que mostrara as infos do save para cada save;

    private const int maximoSlotSaves = 16; //? Define quantos slots maximos de saves.
    private int slotSelecionado = -1;
    private readonly List<GameObject> itens = new List<GameObject>(); //? Lista para gurdar cada save feito pelo usuario.
    public void Start()
    {
        if (loadGamePrefab != null && loadGamePrefab.scene.IsValid()) loadGamePrefab.SetActive(false);
        if (notificacao != null && notificacao.scene.IsValid()) notificacao.SetActive(false);
        if (itenPrefab != null && itenPrefab.scene.IsValid()) itenPrefab.SetActive(false);
    }

    public void SalvarNoSlotBotao()
    {
        int slotDisponivel = -1;
        for (int i = 0; i < maximoSlotSaves; i++)
        {
            if (!SaveSystem.ExisteSlot(i))
            {
                slotDisponivel = i;
                break;
            }
        }

        if (slotDisponivel != -1)
        {
            SaveSystem.Salvar(slotDisponivel);
            if (toastNotifications.Instance != null)
            {
                toastNotifications.Instance.mostrarToast("Succeso ao salvar!", isErro: false);
            }
            atualizarListaSave();
        } else
        {
            if (toastNotifications.Instance != null)
            {
                toastNotifications.Instance.mostrarToast("Erro ao salvar!", isErro: true);
            }
        }
    }

    public void openMenuLoad()
    {
        if (loadGamePrefab != null) loadGamePrefab.SetActive(true);
        slotSelecionado = -1;
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

        for (int i=0; i < maximoSlotSaves; i++)
        {
            GameObject novoItem = Instantiate(itenPrefab, scrollViewLoad, false);
            novoItem.SetActive(true);
            itens.Add(novoItem);
            TMP_Text textoTitulo = novoItem.GetComponentInChildren<TMP_Text>();
            Button botaoItem = novoItem.GetComponentInChildren<Button>();
            int slotAtual = i;

            if (textoTitulo != null)
            {    
                if (SaveSystem.ExisteSlot(slotAtual))
                {
                    textoTitulo.text = $"Slot {slotAtual + 1}: Jogo Salvo";
                }
                else
                {
                    textoTitulo.text = $"Slot {slotAtual + 1} - Vazio";
                }
            }

            if (botaoItem != null)
            {
                botaoItem.onClick.RemoveAllListeners();
                botaoItem.onClick.AddListener(() =>
                {
                    selecionarSlot(slotAtual);
                });
            }
        }

        RectTransform rt = scrollViewLoad as RectTransform;
        if (rt != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    public void selecionarSlot(int index)
    {
        slotSelecionado = index;
    }

    public void CarregarSlotEspecifico()
    {
        if (slotSelecionado == -1)
        {
            if (toastNotifications.Instance != null)
            {

                toastNotifications.Instance.mostrarToast("Slot não selecionado!", isErro: false);
            }
            return;
        }
        if (SaveSystem.ExisteSlot(slotSelecionado))
        {
            SaveSystem.CarregarSlot(slotSelecionado);
            if (toastNotifications.Instance != null)
            {
                toastNotifications.Instance.mostrarToast("Sucesso ao carregar o jogo!", isErro: false);
            }
        } else
        {
            if (toastNotifications.Instance != null)
            {            
                toastNotifications.Instance.mostrarToast("Slot vazios!", isErro: false);
            }
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

    public void ExitGame()
    {
        Application.Quit();
    }
}
