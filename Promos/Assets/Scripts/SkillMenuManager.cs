using UnityEngine;
using UnityEngine.UI;

public class SkillMenuManager : MonoBehaviour
{
    [Header("Estatísticas do Personagem")]
    public int vidaAtual = 280;
    public int vidaMaxima = 320;
    public int pontosDisponiveis = 3; // Pontos para gastar nas skills

    [Header("Referências da UI")]
    public Text textoVida;              // O texto que mostra "280 / 320" na secção de Recursos
    public Image barraVidaPreenchimento; // A imagem de preenchimento da barra de vida
    public Text textoPontos;            // (Opcional) Texto para mostrar quantos pontos tens

    void Start()
    {
        AtualizarUI();
    }

    // Função chamada ao clicar no botão de aumentar Vida
    public void AumentarVida(int quantidadeValor)
    {
        if (pontosDisponiveis > 0)
        {
            vidaMaxima += quantidadeValor;
            vidaAtual += quantidadeValor; // Cura um pouco ao subir a vida máxima
            pontosDisponiveis--;

            AtualizarUI();
            Debug.Log("Vida aumentada! Nova vida máxima: " + vidaMaxima);
        }
        else
        {
            Debug.Log("Não tens pontos suficientes!");
        }
    }

    void AtualizarUI()
    {
        // Atualiza o texto da vida (ex: "300 / 340")
        if (textoVida != null)
        {
            textoVida.text = vidaAtual + " / " + vidaMaxima;
        }

        // Atualiza visualmente o tamanho da barra de vida proporcionalmente
        if (barraVidaPreenchimento != null)
        {
            float proporcao = (float)vidaAtual / vidaMaxima;
            
            // Obtém a largura do trilho pai para calcular o preenchimento correto
            RectTransform rectTrilho = barraVidaPreenchimento.rectTransform.parent as RectTransform;
            float larguraTotal = rectTrilho != null ? rectTrilho.rect.width : 598f;

            // Correção aplicada aqui (removido o rótulo incorreto da tupla)
            float larguraPreenchimento = (larguraTotal - 4f) * Mathf.Clamp01(proporcao);

            barraVidaPreenchimento.rectTransform.sizeDelta = new Vector2(
                larguraPreenchimento, 
                barraVidaPreenchimento.rectTransform.sizeDelta.y
            );
        }

        // Atualiza o texto dos pontos disponíveis (caso tenhes o campo atribuído)
        if (textoPontos != null)
        {
            textoPontos.text = "Pontos: " + pontosDisponiveis;
        }
    }
}