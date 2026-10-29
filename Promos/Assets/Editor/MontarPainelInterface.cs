#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MontarPainelInterface
{
    private const bool MostrarTextos = true;
    private const string Pasta = "Assets/VeuDeFerro/Interface";

    private static Font fonte;

    private static readonly Color Fundo = Cor("#0B1216");
    private static readonly Color Painel = Cor("#111D23");
    private static readonly Color Ouro = Cor("#BA9B66");
    private static readonly Color OuroClaro = Cor("#E3CA99");
    private static readonly Color Texto = Cor("#E9E6DC");
    private static readonly Color Suave = Cor("#A1B0B5");
    private static readonly Color Apagado = Cor("#65777F");
    private static readonly Color Borda = Cor("#37464B");

    [MenuItem("Tools/Véu de Ferro/Criar painel de interface")]
    public static void Criar()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (fonte == null)
        {
            EditorUtility.DisplayDialog(
                "Fonte indisponível",
                "Não foi possível localizar a fonte padrão da Unity.",
                "OK");
            return;
        }

        GarantirPastas(Pasta);

        var cena = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        var cameraObj = new GameObject("Camera");
        var camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Fundo;
        camera.cullingMask = 0;
        cameraObj.transform.position = new Vector3(0, 0, -10);

        var canvasObj = new GameObject(
            "Interface_VeuDeFerro",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));

        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1440, 760);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var raiz = canvasObj.GetComponent<RectTransform>();

        var fundo = Caixa(
            raiz, "Fundo", 0, 0, 1440, 760, Fundo);

        fundo.rectTransform.anchorMin = Vector2.zero;
        fundo.rectTransform.anchorMax = Vector2.one;
        fundo.rectTransform.offsetMin = Vector2.zero;
        fundo.rectTransform.offsetMax = Vector2.zero;

        CriarCabecalho(raiz);
        CriarBotoes(raiz);
        CriarAbas(raiz);
        CriarRecursos(raiz);

        Rotulo(
            raiz, "Assinatura",
            "PEDRA · NÉVOA · FERRO · BRASA",
            48, 722, 450, 18, 11, Suave);

        Rotulo(
            raiz, "Nota",
            "Uma identidade original para as terras do norte.",
            900, 722, 492, 18, 11, Apagado,
            TextAnchor.MiddleRight);

        Canvas.ForceUpdateCanvases();

        string caminhoCena = AssetDatabase.GenerateUniqueAssetPath(
            Pasta + "/ElementosInterface.unity");

        string caminhoPrefab = AssetDatabase.GenerateUniqueAssetPath(
            Pasta + "/PainelInterface.prefab");

        if (!EditorSceneManager.SaveScene(cena, caminhoCena))
        {
            EditorUtility.DisplayDialog(
                "Cena não salva",
                "Não foi possível salvar a cena. Confira as permissões da pasta.",
                "OK");
            return;
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(
            canvasObj, caminhoPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (prefab == null)
        {
            EditorUtility.DisplayDialog(
                "Cena criada",
                "A cena foi salva, mas o Prefab não pôde ser criado.",
                "OK");
            return;
        }

        Selection.activeGameObject = canvasObj;

        bool exportar = EditorUtility.DisplayDialog(
            "Painel criado",
            "Cena e Prefab salvos em:\n" + Pasta +
            "\n\nDeseja gerar também um ZIP e um unitypackage?",
            "Exportar",
            "Agora não");

        if (exportar)
            Exportar(caminhoCena, caminhoPrefab);
    }

    private static void CriarCabecalho(RectTransform pai)
    {
        var selo = Retangulo(pai, "Emblema_aproximado", 56, 50, 34, 34);
        Contorno(selo, Ouro);
        selo.localRotation = Quaternion.Euler(0, 0, 45);

        Rotulo(pai, "Identidade", "VÉU DE FERRO / LINGUAGEM DE INTERFACE", 116, 42, 660, 20, 11, Ouro);
        Rotulo(pai, "Titulo", "FORJADO PARA A JORNADA", 116, 64, 700, 40, 28, Texto);
        Rotulo(pai, "Direcao", "Metal envelhecido. Luz contida. Foco inequívoco.", 940, 48, 452, 18, 12, Suave, TextAnchor.MiddleRight);
        Rotulo(pai, "Formato", "Prancha visual • Escala desktop", 940, 70, 452, 18, 11, Apagado, TextAnchor.MiddleRight);
    }

    private static void CriarBotoes(RectTransform pai)
    {
        var painel = Caixa(pai, "01_Botoes_e_estados", 48, 124, 1344, 284, Painel).rectTransform;
        Contorno(painel, Borda);

        Rotulo(painel, "Titulo", "01 / BOTÕES E ESTADOS", 24, 20, 450, 22, 11, Ouro);
        Rotulo(painel, "Tipo_primario", "Primário", 24, 96, 150, 56, 12, Suave);
        Rotulo(painel, "Tipo_secundario", "Secundário", 24, 168, 150, 56, 12, Suave);

        string[] estados = { "NORMAL", "FOCO / HOVER", "PRESSIONADO", "DESABILITADO" };

        for (int i = 0; i < estados.Length; i++)
        {
            float x = 204 + i * 284;

            Rotulo(painel, "Estado_" + i, estados[i], x, 55, 264, 24, 11, i == 1 ? OuroClaro : Suave, TextAnchor.MiddleCenter);
            BotaoExemplo(painel, "Primario_" + i, "CONTINUAR", x, 96, i, true);
            BotaoExemplo(painel, "Secundario_" + i, "CONFIGURAÇÕES", x, 168, i, false);
        }

        Rotulo(painel, "Orientacao", "Exemplos visuais: normal, foco, pressionado e desabilitado. Nenhuma ação de jogo está conectada.", 24, 238, 1296, 26, 11, Suave);
    }

    private static void BotaoExemplo(RectTransform pai, string nome, string texto, float x, float y, int estado, bool primario)
    {
        Color preenchimento = primario ? Ouro : Cor("#0C151B");
        Color borda = primario ? Ouro : Borda;
        Color textoCor = primario ? Fundo : Texto;

        if (estado == 1)
        {
            preenchimento = primario ? OuroClaro : new Color(Ouro.r, Ouro.g, Ouro.b, 0.15f);
            borda = OuroClaro;
            textoCor = primario ? Fundo : OuroClaro;
        }
        else if (estado == 2)
        {
            preenchimento = Cor("#786342");
            textoCor = Fundo;
        }
        else if (estado == 3)
        {
            preenchimento = Cor("#172126");
            borda = Borda;
            textoCor = Apagado;
        }

        var imagem = Caixa(pai, nome, x, y, 264, 56, preenchimento);
        var rect = imagem.rectTransform;
        Contorno(rect, borda);

        var botao = imagem.gameObject.AddComponent<Button>();
        botao.targetGraphic = imagem;
        botao.transition = Selectable.Transition.None;
        botao.interactable = estado != 3;

        var navegacao = botao.navigation;
        navegacao.mode = Navigation.Mode.None;
        botao.navigation = navegacao;

        Rotulo(rect, "Texto", texto, 22, 0, 220, 56, 14, textoCor, TextAnchor.MiddleCenter);

        if (estado == 1)
        {
            var foco = Caixa(rect, "Indicador_de_foco", 21, 25, 5, 5, textoCor).rectTransform;
            foco.localRotation = Quaternion.Euler(0, 0, 45);
        }
    }

    private static void CriarAbas(RectTransform pai)
    {
        var painel = Caixa(pai, "02_Abas_e_navegacao", 48, 436, 650, 258, Painel).rectTransform;
        Contorno(painel, Borda);

        Rotulo(painel, "Titulo", "02 / ABAS E NAVEGAÇÃO", 24, 20, 600, 22, 11, Ouro);

        string[] nomes = { "Armas", "Armaduras", "Consumíveis", "Materiais" };
        float[] larguras = { 145, 145, 160, 152 };
        float x = 24;

        for (int i = 0; i < nomes.Length; i++)
        {
            float largura = larguras[i];

            var aba = Caixa(painel, "Aba_" + nomes[i], x, 62, largura, 48, i == 0 ? new Color(Ouro.r, Ouro.g, Ouro.b, 0.14f) : Color.clear).rectTransform;

            Rotulo(aba, "Texto", nomes[i], 0, 0, largura, 46, 13, i == 0 ? OuroClaro : Suave, TextAnchor.MiddleCenter);

            if (i == 0)
                Caixa(aba, "Linha_de_selecao", 0, 46, largura, 2, Ouro);

            x += largura;
        }

        Caixa(painel, "Linha_base", 24, 111, 602, 1, Borda);

        Rotulo(painel, "Descricao", "Seleção persistente com acento metálico e linha de base.\nComandos curtos permanecem visíveis no rodapé de cada menu.", 24, 132, 602, 42, 12, Suave);

        Tecla(painel, "Q E", "Abas", 24, 196, 34, 52);
        Tecla(painel, "Enter", "Confirmar", 126, 196, 44, 80);
        Tecla(painel, "Esc", "Voltar", 272, 196, 34, 60);
    }

    private static void Tecla(RectTransform pai, string tecla, string acao, float x, float y, float largura, float larguraAcao)
    {
        var baseTecla = Caixa(pai, "Tecla_" + tecla, x, y, largura, 25, Color.clear).rectTransform;
        Contorno(baseTecla, Cor("#536064"));

        Rotulo(baseTecla, "Texto", tecla, 0, 0, largura, 25, 11, Texto, TextAnchor.MiddleCenter);
        Rotulo(pai, "Acao_" + acao, acao, x + largura + 8, y, larguraAcao, 25, 12, Suave);
    }

    private static void CriarRecursos(RectTransform pai)
    {
        var painel = Caixa(pai, "03_Recursos", 722, 436, 670, 258, Painel).rectTransform;
        Contorno(painel, Borda);

        Rotulo(painel, "Titulo", "03 / RECURSOS DO PERSONAGEM", 36, 20, 598, 22, 11, Ouro);

        Barra(painel, "Vida", "280 / 320", 36, 64, 280f / 320f, Cor("#A65F58"));
        Barra(painel, "Magia", "160 / 160", 36, 124, 1f, Cor("#689CA9"));
        Barra(painel, "Vigor", "195 / 240", 36, 184, 195f / 240f, Cor("#91A07A"));
    }

    private static void Barra(RectTransform pai, string nome, string valor, float x, float y, float proporcao, Color cor)
    {
        var grupo = Retangulo(pai, "Barra_" + nome, x, y, 598, 36);

        Rotulo(grupo, "Nome", nome, 0, 0, 260, 18, 12, Suave);
        Rotulo(grupo, "Valor", valor, 310, 0, 288, 18, 12, Texto, TextAnchor.MiddleRight);

        var trilho = Caixa(grupo, "Trilho", 0, 24, 598, 8, Cor("#253138")).rectTransform;
        Caixa(trilho, "Preenchimento", 2, 2, 594 * Mathf.Clamp01(proporcao), 4, cor);
    }

    private static RectTransform Retangulo(Transform pai, string nome, float x, float y, float largura, float altura)
    {
        var obj = new GameObject(nome, typeof(RectTransform));
        var rect = obj.GetComponent<RectTransform>();

        rect.SetParent(pai, false);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(largura, altura);

        return rect;
    }

    private static Image Caixa(Transform pai, string nome, float x, float y, float largura, float altura, Color cor)
    {
        var rect = Retangulo(pai, nome, x, y, largura, altura);
        var imagem = rect.gameObject.AddComponent<Image>();
        imagem.color = cor;
        imagem.raycastTarget = false;
        return imagem;
    }

    private static Text Rotulo(Transform pai, string nome, string conteudo, float x, float y, float largura, float altura, int tamanho, Color cor, TextAnchor alinhamento = TextAnchor.MiddleLeft)
    {
        var rect = Retangulo(pai, nome, x, y, largura, altura);
        var texto = rect.gameObject.AddComponent<Text>();
        texto.font = fonte;
        texto.text = conteudo;
        texto.fontSize = tamanho;
        texto.color = cor;
        texto.alignment = alinhamento;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Truncate;
        texto.raycastTarget = false;
        texto.supportRichText = false;

        rect.gameObject.SetActive(MostrarTextos);
        return texto;
    }

    private static void Contorno(RectTransform pai, Color cor)
    {
        float largura = pai.sizeDelta.x;
        float altura = pai.sizeDelta.y;

        Caixa(pai, "Borda_superior", 0, 0, largura, 1, cor);
        Caixa(pai, "Borda_inferior", 0, altura - 1, largura, 1, cor);
        Caixa(pai, "Borda_esquerda", 0, 0, 1, altura, cor);
        Caixa(pai, "Borda_direita", largura - 1, 0, 1, altura, cor);
    }

    private static Color Cor(string hexadecimal)
    {
        ColorUtility.TryParseHtmlString(hexadecimal, out Color cor);
        return cor;
    }

    private static void GarantirPastas(string caminho)
    {
        string[] partes = caminho.Split('/');
        string atual = partes[0];

        for (int i = 1; i < partes.Length; i++)
        {
            string proxima = atual + "/" + partes[i];

            if (!AssetDatabase.IsValidFolder(proxima))
                AssetDatabase.CreateFolder(atual, partes[i]);

            atual = proxima;
        }
    }

    private static void Exportar(string cena, string prefab)
    {
        string destino = EditorUtility.SaveFolderPanel(
            "Escolha onde salvar o ZIP e o pacote",
            Directory.GetParent(Application.dataPath).FullName,
            "");

        if (string.IsNullOrEmpty(destino))
            return;

        string nome = "ElementosInterface_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string pacote = Path.Combine(destino, nome + ".unitypackage");
        string zip = Path.Combine(destino, nome + ".zip");

        string[] principais = { cena, prefab };

        try
        {
            AssetDatabase.ExportPackage(principais, pacote, ExportPackageOptions.IncludeDependencies);

            string raizProjeto = Directory.GetParent(Application.dataPath).FullName;
            string[] dependencias = AssetDatabase.GetDependencies(principais, true);

            var entradas = new HashSet<string>();

            using (var arquivo = ZipFile.Open(zip, ZipArchiveMode.Create))
            {
                foreach (string caminho in dependencias)
                {
                    if (!caminho.StartsWith("Assets/", StringComparison.Ordinal))
                        continue;

                    string absoluto = Path.Combine(raizProjeto, caminho);

                    if (File.Exists(absoluto) && entradas.Add(caminho))
                    {
                        arquivo.CreateEntryFromFile(absoluto, caminho);
                    }

                    string meta = absoluto + ".meta";
                    string entradaMeta = caminho + ".meta";

                    if (File.Exists(meta) && entradas.Add(entradaMeta))
                    {
                        arquivo.CreateEntryFromFile(meta, entradaMeta);
                    }
                }

                var instrucoes = arquivo.CreateEntry("LEIA-ME.txt");

                using (var escritor = new StreamWriter(instrucoes.Open(), new System.Text.UTF8Encoding(false)))
                {
                    escritor.WriteLine("PAINEL DE INTERFACE - VEU DE FERRO");
                    escritor.WriteLine();
                    escritor.WriteLine("Cena visual para Unity 6 com Unity UI instalado.");
                    escritor.WriteLine("Extraia o ZIP na raiz do projeto, preservando Assets/ e os .meta.");
                    escritor.WriteLine("Cena: " + cena);
                    escritor.WriteLine("Prefab: " + prefab);
                    escritor.WriteLine("Os estados apresentados sao estaticos e nao possuem acoes de jogo.");
                    escritor.WriteLine("A fonte padrao e fornecida pelo Unity Editor.");
                }
            }

            EditorUtility.DisplayDialog("Exportação concluída", "ZIP e unitypackage salvos em:\n" + destino, "OK");
            EditorUtility.RevealInFinder(pacote);
        }
        catch (Exception erro)
        {
            EditorUtility.DisplayDialog(
                "Não foi possível concluir a exportação",
                erro.Message + "\n\nA cena e o Prefab continuam salvos no projeto. Se algum pacote foi criado parcialmente, descarte-o.",
                "OK");
        }
    }
}
#endif