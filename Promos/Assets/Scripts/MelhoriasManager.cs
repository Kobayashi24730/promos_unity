using UnityEngine;
using UnityEngine.UI;

public class MelhoriasManager : MonoBehaviour
{
    [Header("Painéis de Conteúdo")]
    public GameObject painelArmas;
    public GameObject painelArmaduras;
    public GameObject painelConsumiveis; // Opcional
    public GameObject painelMateriais;   // Opcional

    [Header("Sistema de Pontos")]
    public int pontosArmas = 3;
    public int pontosArmaduras = 3;
    
    [Header("Referências de Texto (Opcional)")]
    public Text textoPontosArmas;
    public Text textoPontosArmaduras;

    // Chamado ao clicar na aba "Armas"
    public void SelecionarAbaArmas()
    {
        AtivarPainel(painelArmas);
    }

    // Chamado ao clicar na aba "Armaduras"
    public void SelecionarAbaArmaduras()
    {
        AtivarPainel(painelArmaduras);
    }

    public void SelecionarAbaConsumiveis()
    {
        AtivarPainel(painelConsumiveis);
    }

    public void SelecionarAbaMateriais()
    {
        AtivarPainel(painelMateriais);
    }

    private void AtivarPainel(GameObject painelAtivo)
    {
        // Desativa todos os painéis
        if (painelArmas != null) painelArmas.SetActive(false);
        if (painelArmaduras != null) painelArmaduras.SetActive(false);
        if (painelConsumiveis != null) painelConsumiveis.SetActive(false);
        if (painelMateriais != null) painelMateriais.SetActive(false);

        // Ativa apenas o selecionado
        if (painelAtivo != null) painelAtivo.SetActive(true);
    }

    // Exemplo de melhoria para Armas (ex: + Dano Geral)
    public void MelhorarDanoArmas(int valorDano)
    {
        if (pontosArmas > 0)
        {
            pontosArmas--;
            // Aqui aplicas a lógica de somar o dano global nas armas do teu jogo
            Debug.Log("Dano de armas aumentado! Pontos restantes: " + pontosArmas);
            AtualizarTextos();
        }
        else
        {
            Debug.Log("Sem pontos de armas disponíveis!");
        }
    }

    // Exemplo de melhoria para Armaduras (ex: + Defesa)
    public void MelhorarDefesaArmaduras(int valorDefesa)
    {
        if (pontosArmaduras > 0)
        {
            pontosArmaduras--;
            // Aqui aplicas a lógica de somar a defesa global nas armaduras
            Debug.Log("Defesa de armaduras aumentada! Pontos restantes: " + pontosArmaduras);
            AtualizarTextos();
        }
        else
        {
            Debug.Log("Sem pontos de armaduras disponíveis!");
        }
    }

    void AtualizarTextos()
    {
        if (textoPontosArmas != null) textoPontosArmas.text = "Pontos Armas: " + pontosArmas;
        if (textoPontosArmaduras != null) textoPontosArmaduras.text = "Pontos Armaduras: " + pontosArmaduras;
    }
}