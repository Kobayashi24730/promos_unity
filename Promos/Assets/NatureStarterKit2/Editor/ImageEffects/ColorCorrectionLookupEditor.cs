using System;
using UnityEditor;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [CustomEditor (typeof(ColorCorrectionLookup))]
    class ColorCorrectionLookupEditor : Editor
    {
        SerializedObject serObj;

        void OnEnable () {
            serObj = new SerializedObject (target);
        }

        private Texture2D tempClutTex2D;

        public override void OnInspectorGUI () {
            serObj.Update ();

            EditorGUILayout.LabelField("Converts textures into color lookup volumes (for grading)", EditorStyles.miniLabel);

            Rect r; Texture2D t;

            tempClutTex2D = EditorGUILayout.ObjectField (" Based on", tempClutTex2D, typeof(Texture2D), false) as Texture2D;
            if (tempClutTex2D == null) {
                t = AssetDatabase.LoadAssetAtPath<Texture2D>(((ColorCorrectionLookup)target).basedOnTempTex);
                if (t) tempClutTex2D = t;
            }

            Texture2D tex = tempClutTex2D;

            if (tex && (target as ColorCorrectionLookup).basedOnTempTex != AssetDatabase.GetAssetPath(tex))
            {
                EditorGUILayout.Separator();
                if (!(target as ColorCorrectionLookup).ValidDimensions(tex))
                {
                    EditorGUILayout.HelpBox ("Invalid texture dimensions!\nPick another texture or adjust dimension to e.g. 256x16.", MessageType.Warning);
                }
                else if (GUILayout.Button ("Convert and Apply"))
                {
                    string path = AssetDatabase.GetAssetPath (tex);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    
                    if (textureImporter != null)
                    {
                        bool doImport = !textureImporter.isReadable;
                        if (textureImporter.mipmapEnabled) {
                            doImport = true;
                        }

                        // Obtém as definições da plataforma padrão (Default)
                        TextureImporterPlatformSettings defaultSettings = textureImporter.GetDefaultPlatformTextureSettings();
                        if (defaultSettings.format != TextureImporterFormat.RGBA32) {
                            doImport = true;
                        }

                        if (doImport)
                        {
                            textureImporter.isReadable = true;
                            textureImporter.mipmapEnabled = false;
                            
                            defaultSettings.format = TextureImporterFormat.RGBA32;
                            defaultSettings.overridden = false;
                            
                            // Correção aplicada aqui: usa SetPlatformTextureSettings passando o objeto de configurações
                            textureImporter.SetPlatformTextureSettings(defaultSettings);
                            
                            AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
                        }

                        ((ColorCorrectionLookup)target).Convert(tex, path);
                    }
                }
            }

            if (!string.IsNullOrEmpty(((ColorCorrectionLookup)target).basedOnTempTex))
            {
                EditorGUILayout.HelpBox("Using " + ((ColorCorrectionLookup)target).basedOnTempTex, MessageType.Info);
                t = AssetDatabase.LoadAssetAtPath<Texture2D>(((ColorCorrectionLookup)target).basedOnTempTex);
                if (t) {
                    r = GUILayoutUtility.GetLastRect();
                    r = GUILayoutUtility.GetRect(r.width, 20);
                    r.x += r.width * 0.05f/2.0f;
                    r.width *= 0.95f;
                    GUI.DrawTexture (r, t);
                    GUILayoutUtility.GetRect(r.width, 4);
                }
            }

     }
   }
}