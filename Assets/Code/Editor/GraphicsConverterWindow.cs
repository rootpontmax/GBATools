using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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
            GraphicsData newData = EditorGUILayout.ObjectField("Graphics Data", _graphicsData, typeof(GraphicsData), false) as GraphicsData;
            if( newData != _graphicsData )
                CreateEditorData(newData);
            _graphicsData = newData;

            // CRAP
            if( GUILayout.Button("Reload") )
                CreateEditorData(_graphicsData);
            // end of CRAP

            if( null == _graphicsData )
                return;

            _textureViewSize = (int)EditorGUILayout.Slider(_textureViewSize, TEXTURE_VIEW_SIZE_MIN, TEXTURE_VIEW_SIZE_MAX);

            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll, false, true);
                            
            if( GUILayout.Button("Convert") )
            {
                GraphicsConverter.Convert(_graphicsData);
                AssetDatabase.SaveAssets();
                CreateEditorData(_graphicsData);
            }


            DrawImageSets();


            EditorGUILayout.EndScrollView();
        }

        private void CreateEditorData(GraphicsData data)
        {
            _dataMap.Clear();
            _imageToIndexedMap.Clear();
            _imageSetFoldout = null;

            if( null != data && null != data.imageSets )
            {
                _imageSetFoldout = new bool[data.imageSets.Length];
                for( int i = 0; i < data.imageSets.Length; ++i )
                {
                    ImageSetEditorData editorData = new ImageSetEditorData(data.imageSets[i]);
                    _dataMap.Add(data.imageSets[i], editorData);
                }
            }
        }

        private void DrawImageSets()
        {
            _foldputImageSet = EditorGUILayout.Foldout(_foldputImageSet, "Image Sets");
            if( !_foldputImageSet )
                return;

            ++EditorGUI.indentLevel;
            if( null != _graphicsData.imageSets )
                for( int i = 0; i < _graphicsData.imageSets.Length; ++i )
                {
                    _imageSetFoldout[i] = EditorGUILayout.Foldout(_imageSetFoldout[i], _graphicsData.imageSets[i].name);
                    if( _imageSetFoldout[i] )
                    {
                        ImageSetEditorData data = _dataMap[_graphicsData.imageSets[i]];
                        if( null != data )
                        {
                            ++EditorGUI.indentLevel;
                            DrawPalette(data);
                            DrawImages(data);
                            --EditorGUI.indentLevel;
                        }
                    }
                }
            ++EditorGUI.indentLevel;
        }

        private void DrawPalette(ImageSetEditorData data)
        {
            if( null != data.paletteTexture )
            {
                data.paletteFoldout = EditorGUILayout.Foldout(data.paletteFoldout, data.paletteName);
                if( data.paletteFoldout )
                    DrawTexture(data.paletteTexture);
            }
        }

        private void DrawImages(ImageSetEditorData data)
        {
            for( int i = 0; i < data.images.Length; ++i )
            {
                data.images[i].foldout = EditorGUILayout.Foldout(data.images[i].foldout, data.images[i].name);
                if( data.images[i].foldout )
                {
                    GUILayout.BeginHorizontal();
                    DrawTexture(data.images[i].original);
                    DrawTexture(data.images[i].indexed);
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void DrawTexture(Texture2D image)
        {
            if( null == image )
                return;

            GUILayout.Label("", GUILayout.Height(_textureViewSize), GUILayout.Width(_textureViewSize));
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), image);
        }

        private class Data
        {
            public Texture2D palette;
        }

        private static readonly int TEXTURE_VIEW_SIZE_MIN = 50;
        private static readonly int TEXTURE_VIEW_SIZE_MAX = 400;

        private readonly Dictionary<ImageSet, ImageSetEditorData> _dataMap = new Dictionary<ImageSet, ImageSetEditorData>();

        private readonly Dictionary<ImageSet.Image, Texture2D> _imageToIndexedMap = new Dictionary<ImageSet.Image, Texture2D>();
        private bool[] _imageSetFoldout;
        private readonly Dictionary<ImageSet, Texture2D> _paletteMap = new Dictionary<ImageSet, Texture2D>();





        private GraphicsData _graphicsData;
        private int _textureViewSize;
        private Vector2 _mainScroll;
        private bool _foldputImageSet;
    }
}
