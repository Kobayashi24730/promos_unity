#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class AtalhoSave : MonoBehaviour
{
    [MenuItem("Ferramentas/Abrir Pasta de Save")]
    public static void AbrirPasta()
    {
        string caminho = Application.persistentDataPath;
        if (Directory.Exists(caminho))
        {
            EditorUtility.RevealInFinder(caminho);
        }
        else
        {
            Debug.LogWarning("A pasta de save ainda não foi criada (jogue e salve primeiro). Caminho: " + caminho);
        }
    }
}
#endif
