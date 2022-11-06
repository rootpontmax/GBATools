using UnityEditor;
using UnityEngine;

namespace msSoft.GBATools.Editor
{
    public class GraphicsConverterWindow : EditorWindow
    {
        [MenuItem("GBATools/Editors/Graphics Converter")]
        private static void Init()
        {
            GraphicsConverterWindow window = EditorWindow.GetWindow<GraphicsConverterWindow>("Graphics Converter");
            window.Show();
        }

        private void OnGUI()
        {
            _graphicsData = EditorGUILayout.ObjectField("Graphics Data", _graphicsData, typeof(GraphicsData), false) as GraphicsData;
            if( null != _graphicsData )
            {                
                if( GUILayout.Button("Convert") )
                {
                    _converter = new GraphicsConverter(_graphicsData);
                    _converter.Convert();
                }

                if( null != _converter )
                    DrawTextures();
            }
        }

        private void DrawTextures()
        {
            if( null != _graphicsData.imagesCollections )
                for( int i = 0; i < _graphicsData.imagesCollections.Length; ++i )
                    if( null != _graphicsData.imagesCollections[i].images )
                        for( int j = 0; j < _graphicsData.imagesCollections[i].images.Length; ++j )                            
                        {
                            string name = _graphicsData.imagesCollections[i].images[j].variableName;
                            Texture2D original = _graphicsData.imagesCollections[i].images[j].image;
                            Texture2D paletted = _converter.GetPalettedImageByOriginal(original);
                            DrawOriginalTextureAndPaletted(name, original, paletted);
                        }
        }

        private void DrawOriginalTextureAndPaletted(string name, Texture2D original, Texture2D paletted)
        {
            GUILayout.BeginHorizontal();
            //var style = new GUIStyle(GUI.skin.label);
            //style.alignment = TextAnchor.UpperCenter;
            //style.fixedWidth = TEXTURE_SIZE;
            //GUILayout.Label(name, style);
            EditorGUILayout.ObjectField(original, typeof(Texture2D), false, GUILayout.Width(TEXTURE_SIZE), GUILayout.Height(TEXTURE_SIZE));
            if( null != paletted )
                EditorGUILayout.ObjectField(paletted, typeof(Texture2D), false, GUILayout.Width(TEXTURE_SIZE), GUILayout.Height(TEXTURE_SIZE));
            GUILayout.EndHorizontal();
        }

        private static readonly int TEXTURE_SIZE = 400;

        private GraphicsData _graphicsData;
        private GraphicsConverter _converter;
    }
}
