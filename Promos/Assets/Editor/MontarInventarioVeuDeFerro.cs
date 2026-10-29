#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MontarInventarioVeuDeFerro
{
    private const bool MostrarTextos = true;
    private const string Pasta = "Assets/VeuDeFerro/Inventario";
    private const string Arte = "Assets/VeuDeFerro/Arte";

    private static Font fonte;
    private static Font fonteTitulo;

    private static readonly Color Fundo = Cor("#0B1216");
    private static readonly Color Painel = Cor("#0D171D");
    private static readonly Color Ouro = Cor("#BA9B66");
    private static readonly Color OuroClaro = Cor("#E3CA99");
    private static readonly Color Texto = Cor("#E9E6DC");
    private static readonly Color Suave = Cor("#A1B0B5");
    private static readonly Color Apagado = Cor("#65777F");
    private static readonly Color Borda = Cor("#37464B");

    [MenuItem("Tools/Véu de Ferro/Criar inventário")]
    public static void Criar()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        fonte = AssetDatabase.LoadAssetAtPath<Font>(
            Arte + "/Inter-Regular.ttf");

        if (fonte == null)
            fonte = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");

        fonteTitulo = AssetDatabase.LoadAssetAtPath<Font>(
            Arte + "/Cinzel-Regular.ttf");

        if (fonteTitulo == null)
            fonteTitulo = fonte;

        if (fonte == null)
        {
            EditorUtility.DisplayDialog(
                "Fonte indisponível",
                "Não foi possível localizar uma fonte para o inventário.",
                "OK");
            return;
        }

        GarantirPastas(Pasta);
        GarantirPastas(Arte);

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
            "Inventario_VeuDeFerro",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));

        canvasObj.GetComponent<Canvas>().renderMode =
            RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1440, 900);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var raiz = canvasObj.GetComponent<RectTransform>();

        var baseFundo = Caixa(
            raiz, "Fundo", 0, 0, 1440, 900, Fundo);

        Esticar(baseFundo.rectTransform);

        var texturaFundo = AssetDatabase.LoadAssetAtPath<Texture2D>(
            Arte + "/fundo-inventario-escurecido.png");

        if (texturaFundo != null)
        {
            var imagem = Imagem(
                raiz, "Cenario",
                0, 0, 1440, 900, texturaFundo, false);

            Esticar(imagem.rectTransform);
        }

        Cabecalho(raiz);
        Categorias(raiz);
        ListaDeItens(raiz);
        Detalhes(raiz);
        Condicao(raiz);
        Rodape(raiz);

        // Prancha visual: sem EventSystem, ações ou navegação conectada.
        Canvas.ForceUpdateCanvases();

        string cenaPath = AssetDatabase.GenerateUniqueAssetPath(
            Pasta + "/InventarioVeuDeFerro.unity");

        string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
            Pasta + "/InventarioVeuDeFerro.prefab");

        if (!EditorSceneManager.SaveScene(cena, cenaPath))
        {
            EditorUtility.DisplayDialog(
                "Cena não salva",
                "Não foi possível salvar a cena.",
                "OK");
            return;
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(
            canvasObj, prefabPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = canvasObj;

        if (prefab == null)
        {
            EditorUtility.DisplayDialog(
                "Cena criada",
                "A cena foi salva, mas o Prefab não pôde ser criado.",
                "OK");
            return;
        }

        if (EditorUtility.DisplayDialog(
            "Inventário criado",
            "Cena e Prefab salvos em:\n" + Pasta +
            "\n\nDeseja gerar ZIP e unitypackage?",
            "Exportar", "Agora não"))
        {
            Exportar(cenaPath, prefabPath);
        }
    }

    private static void Cabecalho(RectTransform pai)
    {
        Rotulo(pai, "Legenda", "PERTENCES DA VIAJANTE",
            48, 32, 420, 20, 11, Ouro);

        Rotulo(pai, "Titulo", "Inventário",
            48, 53, 600, 50, 34, Texto,
            TextAnchor.MiddleLeft, true);

        Rotulo(pai, "Personagem", "Eira do Vale Cinzento",
            940, 38, 378, 34, 22, Texto,
            TextAnchor.MiddleRight, true);

        Rotulo(pai, "Classe",
            "Guardiã do Véu • Clã do Freixo",
            940, 76, 378, 20, 12, Suave,
            TextAnchor.MiddleRight);

        var nivel = Caixa(
            pai, "Nivel", 1338, 40, 54, 54,
            Color.clear).rectTransform;

        Contorno(nivel, Ouro);

        Rotulo(nivel, "Valor", "18",
            0, 0, 54, 54, 24, OuroClaro,
            TextAnchor.MiddleCenter, true);
    }

    private static void Categorias(RectTransform pai)
    {
        var painel = CriarPainel(
            pai, "Categorias", 48, 126, 210, 574);

        Rotulo(painel, "Titulo", "CATEGORIAS",
            16, 16, 178, 28, 11, Suave);

        string[] nomes =
        {
            "Todos os itens", "Armas", "Armaduras",
            "Consumíveis", "Materiais", "Missão"
        };

        string[] quantidades = { "34", "8", "6", "9", "7", "4" };

        string[] icones =
        {
            "categorias", "armas", "escudo",
            "pocao", "gema", "pergaminho"
        };

        for (int i = 0; i < nomes.Length; i++)
        {
            bool selecionado = i == 1;

            var linha = Caixa(
                painel, "Categoria_" + icones[i],
                16, 62 + i * 55, 178, 48,
                selecionado
                    ? new Color(Ouro.r, Ouro.g, Ouro.b, 0.14f)
                    : Color.clear).rectTransform;

            Icone(linha, icones[i], 10, 15, 18,
                selecionado ? OuroClaro : Suave);

            Rotulo(linha, "Nome", nomes[i],
                37, 0, 116, 48, 12,
                selecionado ? OuroClaro : Suave);

            Rotulo(linha, "Quantidade", quantidades[i],
                150, 0, 18, 48, 10, Apagado,
                TextAnchor.MiddleRight);
        }

        Divisor(painel, 16, 432, 178);

        Rotulo(painel, "Filtro", "Mostrar todos",
            16, 448, 178, 28, 11, Suave);
    }

    private static void ListaDeItens(RectTransform pai)
    {
        var painel = CriarPainel(
            pai, "Lista_de_itens", 280, 126, 478, 574);

        Rotulo(painel, "Titulo", "ARMAS · 8 ITENS",
            15, 12, 300, 32, 11, Ouro);

        Rotulo(painel, "Coluna_dano", "Dano",
            374, 12, 40, 32, 10, Suave,
            TextAnchor.MiddleRight);

        Rotulo(painel, "Coluna_peso", "Peso",
            427, 12, 37, 32, 10, Suave,
            TextAnchor.MiddleRight);

        string[] nomes =
        {
            "Machado da Vigília",
            "Espada de ferro",
            "Arco de freixo",
            "Adaga do viajante",
            "Martelo da geada",
            "Flecha de ferro",
            "Cajado das brumas",
            "Escudo do vale"
        };

        string[] descricoes =
        {
            "Uma mão • Equipado",
            "Uma mão • Comum",
            "À distância • Incomum",
            "Uma mão • Comum",
            "Duas mãos • Raro",
            "Munição • 36 unidades",
            "Catalisador • Incomum",
            "Defesa • Comum"
        };

        string[] danos =
        {
            "64", "42", "38", "24", "86", "8", "51", "—"
        };

        string[] pesos =
        {
            "8,0", "6,0", "4,0", "1,5",
            "14,0", "0,1", "5,0", "7,0"
        };

        string[] icones =
        {
            "machado", "espada", "arco", "espada",
            "martelo", "flecha", "cajado", "escudo"
        };

        for (int i = 0; i < nomes.Length; i++)
        {
            bool selecionado = i == 0;

            var linha = Caixa(
                painel, "Item_" + i,
                0, 56 + i * 63, 478, 63,
                selecionado
                    ? new Color(Ouro.r, Ouro.g, Ouro.b, 0.14f)
                    : Color.clear).rectTransform;

            Caixa(linha, "Separador",
                0, 62, 478, 1,
                new Color(Borda.r, Borda.g, Borda.b, 0.5f));

            if (selecionado)
                Caixa(linha, "Acento", 0, 0, 2, 63, Ouro);

            Icone(linha, icones[i], 14, 20, 23,
                selecionado ? OuroClaro : Suave);

            Rotulo(linha, "Nome", nomes[i],
                50, 10, 300, 25, 14,
                selecionado ? OuroClaro : Texto);

            Rotulo(linha, "Descricao", descricoes[i],
                50, 34, 300, 20, 10, Suave);

            Rotulo(linha, "Dano", danos[i],
                374, 0, 40, 63, 13, Texto,
                TextAnchor.MiddleRight);

            Rotulo(linha, "Peso", pesos[i],
                427, 0, 37, 63, 13, Suave,
                TextAnchor.MiddleRight);
        }
    }

    private static void Detalhes(RectTransform pai)
    {
        var painel = CriarPainel(
            pai, "Detalhe_do_item", 780, 126, 612, 574);

        Rotulo(painel, "Raridade", "RARO • UMA MÃO",
            28, 16, 300, 22, 11, Ouro);

        Rotulo(painel, "Estado", "EQUIPADO",
            380, 16, 204, 22, 10, OuroClaro,
            TextAnchor.MiddleRight);

        var arteItem = AssetDatabase.LoadAssetAtPath<Texture2D>(
            Arte + "/ilustracao-machado.png");

        var area = Retangulo(
            painel, "Previa_do_item",
            28, 46, 556, 204);

        if (arteItem != null)
        {
            Imagem(area, "Ilustracao",
                0, 0, 556, 204, arteItem, true);
        }
        else
        {
            var reserva = Caixa(
                area, "Area_reservada",
                0, 0, 556, 204,
                new Color(0, 0, 0, 0.12f)).rectTransform;

            Contorno(reserva, Borda);

            Rotulo(reserva, "Instrucao",
                "PRÉVIA DO ITEM\n" +
                "Adicione ilustracao-machado.png em\n" + Arte,
                18, 30, 520, 144, 14, Apagado,
                TextAnchor.MiddleCenter);
        }

        Rotulo(painel, "Nome", "Machado da Vigília",
            28, 263, 556, 42, 27, Texto,
            TextAnchor.MiddleLeft, true);

        Atributo(painel, "DANO", "64", 28);
        Atributo(painel, "PESO", "8,0", 221);
        Atributo(painel, "VALOR", "320", 414);

        Divisor(painel, 28, 383, 556);

        Rotulo(painel, "Encantamento",
            "Geada persistente · +12 de dano de gelo.",
            28, 400, 556, 24, 13, Cor("#689CA9"));

        Rotulo(painel, "Historia",
            "Forjado para os sentinelas do último farol. Sua lâmina " +
            "ainda guarda o frio da noite em que a chama se apagou.",
            28, 429, 556, 42, 12, Suave,
            TextAnchor.UpperLeft);

        BotaoVisual(painel, "Desequipar",
            "DESEQUIPAR", 28, 480, true);

        BotaoVisual(painel, "Favoritar",
            "FAVORITAR", 312, 480, false);
    }

    private static void Atributo(
        RectTransform pai, string nome, string valor, float x)
    {
        var grupo = Retangulo(
            pai, "Atributo_" + nome, x, 312, 169, 58);

        Rotulo(grupo, "Legenda", nome,
            0, 0, 169, 18, 11, Suave);

        Rotulo(grupo, "Valor", valor,
            0, 20, 169, 36, 26, OuroClaro,
            TextAnchor.MiddleLeft, true);
    }

    private static void Condicao(RectTransform pai)
    {
        var grupo = Retangulo(
            pai, "Condicao_do_personagem",
            48, 724, 1344, 70);

        Caixa(grupo, "Separador", 0, 0, 1344, 1, Borda);

        Barra(grupo, "Vida", "280 / 320",
            0, 19, 280f / 320f, Cor("#A65F58"));

        Barra(grupo, "Magia", "160 / 160",
            360, 19, 1f, Cor("#689CA9"));

        Barra(grupo, "Vigor", "195 / 240",
            720, 19, 195f / 240f, Cor("#91A07A"));

        Rotulo(grupo, "Legenda_carga", "CARGA",
            1080, 12, 115, 18, 11, Suave);

        Rotulo(grupo, "Carga", "92,4 / 180",
            1080, 34, 115, 24, 15, Texto);

        Rotulo(grupo, "Legenda_prata", "PRATA",
            1230, 12, 114, 18, 11, Suave,
            TextAnchor.MiddleRight);

        Rotulo(grupo, "Prata", "2.480",
            1230, 34, 114, 24, 15, OuroClaro,
            TextAnchor.MiddleRight);
    }

    private static void Barra(
        RectTransform pai, string nome, string valor,
        float x, float y, float proporcao, Color cor)
    {
        var grupo = Retangulo(
            pai, "Barra_" + nome, x, y, 235, 36);

        Rotulo(grupo, "Nome", nome,
            0, 0, 90, 18, 12, Suave);

        Rotulo(grupo, "Valor", valor,
            105, 0, 130, 18, 12, Texto,
            TextAnchor.MiddleRight);

        var trilho = Caixa(
            grupo, "Trilho", 0, 24, 235, 8,
            Cor("#253138")).rectTransform;

        Caixa(trilho, "Preenchimento",
            2, 2, 231 * Mathf.Clamp01(proporcao), 4, cor);
    }

    private static void Rodape(RectTransform pai)
    {
        var grupo = Retangulo(
            pai, "Comandos", 48, 818, 1344, 28);

        Tecla(grupo, "↑ ↓", "Selecionar", 0, 37, 80);
        Tecla(grupo, "Q E", "Categoria", 150, 34, 80);
        Tecla(grupo, "Enter", "Equipar / remover", 300, 44, 140);
        Tecla(grupo, "F", "Favoritar", 520, 25, 90);
        Tecla(grupo, "Esc", "Voltar ao jogo", 1180, 34, 120);
    }

    private static void Tecla(
        RectTransform pai, string tecla, string acao,
        float x, float largura, float larguraAcao)
    {
        var caixa = Caixa(
            pai, "Tecla_" + tecla,
            x, 0, largura, 25, Color.clear).rectTransform;

        Contorno(caixa, Cor("#536064"));

        Rotulo(caixa, "Texto", tecla,
            0, 0, largura, 25, 11, Texto,
            TextAnchor.MiddleCenter);

        Rotulo(pai, "Acao_" + acao, acao,
            x + largura + 8, 0, larguraAcao, 25,
            12, Suave);
    }

    private static void BotaoVisual(
        RectTransform pai, string nome, string texto,
        float x, float y, bool primario)
    {
        var imagem = Caixa(
            pai, nome, x, y, 272, 48,
            primario ? Ouro : Cor("#0C151B"));

        Contorno(imagem.rectTransform, primario ? Ouro : Borda);

        var botao = imagem.gameObject.AddComponent<Button>();
        botao.targetGraphic = imagem;
        botao.transition = Selectable.Transition.None;

        var navegacao = botao.navigation;
        navegacao.mode = Navigation.Mode.None;
        botao.navigation = navegacao;

        Rotulo(imagem.rectTransform, "Texto", texto,
            8, 0, 256, 48, 13, primario ? Fundo : Texto,
            TextAnchor.MiddleCenter);
    }

    private static void Icone(
        RectTransform pai, string nome,
        float x, float y, float tamanho, Color cor)
    {
        var textura = AssetDatabase.LoadAssetAtPath<Texture2D>(
            Arte + "/icone-" + nome + ".png");

        var grupo = Retangulo(
            pai, "Icone_" + nome, x, y, tamanho, tamanho);

        if (textura != null)
        {
            Imagem(grupo, "Imagem",
                0, 0, tamanho, tamanho, textura, true);
        }
        else
        {
            var marcador = Caixa(
                grupo, "Marcador_provisorio",
                tamanho / 2f, tamanho / 2f, 5, 5, cor);

            marcador.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            marcador.rectTransform.localRotation =
                Quaternion.Euler(0, 0, 45);
        }
    }

    private static RectTransform CriarPainel(
        Transform pai, string nome,
        float x, float y, float largura, float altura)
    {
        var painel = Caixa(
            pai, nome, x, y, largura, altura,
            Painel).rectTransform;

        Contorno(painel, Borda);
        return painel;
    }

    private static void Divisor(
        RectTransform pai, float x, float y, float largura)
    {
        var grupo = Retangulo(
            pai, "Divisor", x, y, largura, 8);

        float centro = largura / 2f;

        Caixa(grupo, "Linha_esquerda",
            0, 3, centro - 14, 1, Borda);

        Caixa(grupo, "Linha_direita",
            centro + 14, 3, centro - 14, 1, Borda);

        var losango = Caixa(
            grupo, "Losango", centro, 3, 5, 5, Ouro);

        losango.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        losango.rectTransform.localRotation =
            Quaternion.Euler(0, 0, 45);
    }

    private static RawImage Imagem(
        Transform pai, string nome,
        float x, float y, float largura, float altura,
        Texture2D textura, bool preservarProporcao)
    {
        var area = Retangulo(
            pai, nome, x, y, largura, altura);

        var imagemRect = Retangulo(
            area, "Textura", 0, 0, largura, altura);

        Esticar(imagemRect);

        var imagem = imagemRect.gameObject.AddComponent<RawImage>();
        imagem.texture = textura;
        imagem.color = Color.white;
        imagem.raycastTarget = false;

        if (preservarProporcao)
        {
            var ajuste = imagemRect.gameObject
                .AddComponent<AspectRatioFitter>();

            ajuste.aspectRatio =
                textura.width / (float)textura.height;

            ajuste.aspectMode =
                AspectRatioFitter.AspectMode.FitInParent;
        }

        return imagem;
    }

    private static RectTransform Retangulo(
        Transform pai, string nome,
        float x, float y, float largura, float altura)
    {
        var objeto = new GameObject(
            nome, typeof(RectTransform));

        var rect = objeto.GetComponent<RectTransform>();
        rect.SetParent(pai, false);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(largura, altura);
        return rect;
    }

    private static Image Caixa(
        Transform pai, string nome,
        float x, float y, float largura, float altura,
        Color cor)
    {
        var rect = Retangulo(
            pai, nome, x, y, largura, altura);

        var imagem = rect.gameObject.AddComponent<Image>();
        imagem.color = cor;
        imagem.raycastTarget = false;
        return imagem;
    }

    private static Text Rotulo(
        Transform pai, string nome, string conteudo,
        float x, float y, float largura, float altura,
        int tamanho, Color cor,
        TextAnchor alinhamento = TextAnchor.MiddleLeft,
        bool titulo = false)
    {
        var rect = Retangulo(
            pai, nome, x, y, largura, altura);

        var texto = rect.gameObject.AddComponent<Text>();
        texto.font = titulo ? fonteTitulo : fonte;
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

    private static void Esticar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Color Cor(string hexadecimal)
    {
        ColorUtility.TryParseHtmlString(
            hexadecimal, out Color cor);
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
        string raizProjeto =
            Directory.GetParent(Application.dataPath).FullName;

        string destino = EditorUtility.SaveFolderPanel(
            "Salvar ZIP e pacote do inventário",
            raizProjeto, "");

        if (string.IsNullOrEmpty(destino))
            return;

        string nome = "InventarioVeuDeFerro_" +
            DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");

        string pacote = Path.Combine(
            destino, nome + ".unitypackage");

        string zip = Path.Combine(destino, nome + ".zip");
        string[] principais = { cena, prefab };

        try
        {
            AssetDatabase.ExportPackage(
                principais, pacote,
                ExportPackageOptions.IncludeDependencies);

            string[] dependencias =
                AssetDatabase.GetDependencies(principais, true);

            var entradas = new HashSet<string>();

            using (var arquivo = ZipFile.Open(
                zip, ZipArchiveMode.Create))
            {
                foreach (string caminho in dependencias)
                {
                    if (!caminho.StartsWith(
                        "Assets/", StringComparison.Ordinal))
                        continue;

                    string absoluto = Path.Combine(
                        raizProjeto, caminho);

                    if (File.Exists(absoluto) &&
                        entradas.Add(caminho))
                    {
                        arquivo.CreateEntryFromFile(
                            absoluto, caminho);
                    }

                    string meta = absoluto + ".meta";
                    string entradaMeta = caminho + ".meta";

                    if (File.Exists(meta) &&
                        entradas.Add(entradaMeta))
                    {
                        arquivo.CreateEntryFromFile(
                            meta, entradaMeta);
                    }
                }

                var instrucoes = arquivo.CreateEntry("LEIA-ME.txt");

                using (var escritor = new StreamWriter(
                    instrucoes.Open(),
                    new System.Text.UTF8Encoding(false)))
                {
                    escritor.WriteLine(
                        "INVENTARIO VISUAL - VEU DE FERRO");
                    escritor.WriteLine();
                    escritor.WriteLine(
                        "Unity 6 com Unity UI instalado.");
                    escritor.WriteLine(
                        "Extraia na raiz do projeto, preservando Assets/ e os .meta.");
                    escritor.WriteLine("Cena: " + cena);
                    escritor.WriteLine("Prefab: " + prefab);
                    escritor.WriteLine(
                        "Referencia visual: 1440 x 900.");
                    escritor.WriteLine(
                        "Nao inclui logica de inventario, cliques, saves ou gameplay.");
                    escritor.WriteLine(
                        "Somente imagens e fontes atribuidas sao incluidas como dependencias.");
                }
            }

            EditorUtility.DisplayDialog(
                "Exportação concluída",
                "ZIP e unitypackage salvos em:\n" + destino,
                "OK");

            EditorUtility.RevealInFinder(pacote);
        }
        catch (Exception erro)
        {
            EditorUtility.DisplayDialog(
                "Exportação não concluída",
                erro.Message +
                "\n\nA cena e o Prefab continuam salvos no projeto. " +
                "Descarte qualquer pacote criado parcialmente.",
                "OK");
        }
    }
}
#endif
