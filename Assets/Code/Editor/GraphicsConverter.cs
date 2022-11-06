using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace msSoft.GBATools.Editor
{
    public class GraphicsConverter
    {
        public GraphicsConverter(GraphicsData data)
        {
            _data = data;
            _imageRawData = new List<ImageRawData>();
            _palette = new HashSet<ColorRGB>();
            _paletteMap = new Dictionary<ColorRGB, ColorRGB>();
            _originalToPalette = new Dictionary<Texture2D, Texture2D>();
        }

        public void Convert()
        {
            if( null == _data )
                return;

            Clear();
            CollectImages();
            CreatePalette();
            ConvertImages();

            

            string path = EditorUtility.OpenFolderPanel("Choose folder to save template", "", "");
            string fullPath = path + "/" + _data.filename + ".h";
            Debug.LogError(fullPath);
            
            //TextAsset textAsset = 
        }

        public Texture2D GetPalettedImageByOriginal(Texture2D original)
        {
            Texture2D paletted = null;
            if( _originalToPalette.TryGetValue(original, out paletted ) )
                return paletted;

            return null;
        }

        private void Clear()
        {
            _imageRawData.Clear();
            _palette.Clear();
            _paletteMap.Clear();
            _originalToPalette.Clear();
        }

        private void CollectImages()
        {
            if( null != _data && null != _data.imagesCollections )
                for( int i = 0; i < _data.imagesCollections.Length; ++i )
                    if( null != _data.imagesCollections[i].images )
                        for( int j = 0; j < _data.imagesCollections[i].images.Length; ++j )
                            AddImage(_data.imagesCollections[i].images[j]);
        }

        private void CreatePalette()
        {
            // Collect all colors that have to be resolved by palette
            HashSet<ColorRGB> colorSet = new HashSet<ColorRGB>();
            for( int i = 0; i < _imageRawData.Count; ++i )
                if( _imageRawData[i].needPalette )
                    for( int j = 0; j < _imageRawData[i].pixels.Length; ++j )
                    {
                        ColorRGB cRGB = new ColorRGB(_imageRawData[i].pixels[j]);
                        colorSet.Add(cRGB);
                    }

            // Create colorList
            List<Color3> colorList = new List<Color3>(colorSet.Count);
            foreach(var item in colorSet)
            {
                Color3 c3 = new Color3(item.r, item.g, item.b);
                colorList.Add(c3);
            }

            // Iteration sorting
            SortColorList(colorList, 0, colorList.Count, MEDIAN_CUT_ITERATION_COUNT);


            Debug.LogError("Unique colors: " + colorList.Count);
            Debug.LogError("Palette colors: " + _palette.Count);
        }

        private void ConvertImages()
        {
            for( int i = 0; i < _imageRawData.Count; ++i )
                if( _imageRawData[i].needPalette )
                {
                    _imageRawData[i].palettedImage = new Texture2D(_imageRawData[i].sizeX, _imageRawData[i].sizeY,_imageRawData[i].originalImage.format, false);
                    Color32[] colors32 = new Color32[_imageRawData[i].pixels.Length];
                    for( int j = 0; j < _imageRawData[i].pixels.Length; ++j )
                    {
                        ColorRGB originalColor = new ColorRGB(_imageRawData[i].pixels[j]);
                        ColorRGB paletteColor = _paletteMap[originalColor];

                        colors32[j].r = paletteColor.r;
                        colors32[j].g = paletteColor.g;
                        colors32[j].b = paletteColor.b;
                        colors32[j].a = 255;
                    }
                    _imageRawData[i].palettedImage.SetPixels32(colors32);
                    _imageRawData[i].palettedImage.Apply(false, true);

                    _originalToPalette.Add(_imageRawData[i].originalImage, _imageRawData[i].palettedImage);
                }
        }

        private void SortColorList(List<Color3> list, int startID, int count, int iterationCount)
        {
            if( 0 == count )
                return;

            if( 0 == iterationCount || count < 2 )
            {
                ColorRGB avgCol = DefineAverageColor(list, startID, count);
                for( int i = 0; i < count; ++i )
                {
                    int id = startID + i;
                    ColorRGB colRGB = new ColorRGB(list[id].cmp[0],list[id].cmp[1], list[id].cmp[2]);
                    _palette.Add(avgCol);
                    _paletteMap.Add(colRGB, avgCol);
                }
            }
            else
            {
                // Define widest color range
                Vector2Int minMaxR = new Vector2Int(256, -1);
                Vector2Int minMaxG = new Vector2Int(256, -1);
                Vector2Int minMaxB = new Vector2Int(256, -1);
                for( int i = 0; i < count; ++i )
                {
                    int id = startID + i;
                    CalcMinMax(ref minMaxR, list[id].cmp[0]);
                    CalcMinMax(ref minMaxG, list[id].cmp[1]);
                    CalcMinMax(ref minMaxB, list[id].cmp[2]);
                }
                int rangeR = minMaxR.y - minMaxR.x + 1;
                int rangeG = minMaxG.y - minMaxG.x + 1;
                int rangeB = minMaxB.y - minMaxB.x + 1;
                int mode = DefineWidestRage(rangeR, rangeG, rangeB);

                // Sorting
                IComparer<Color3> comparer = new ColorComparer(mode);
                list.Sort(startID, count, comparer);

                // Split range and start process again
                bool isOdd = ( count % 2 ) > 0;
                int countL = count / 2;
                int countR = isOdd ? count - countL : countL;
                Debug.Assert(count == countL + countR, "Counts calculates wrong");
                int idL = startID;
                int idR = idL + countL;
                --iterationCount;

                SortColorList(list, idL, countL, iterationCount);
                SortColorList(list, idR, countR, iterationCount);
            }
        }

        private ColorRGB DefineAverageColor(List<Color3> list, int startID, int count)
        {
            int colR = 0;
            int colG = 0;
            int colB = 0;

            for( int i = 0; i < count; ++i )
            {
                int id = startID + i;
                colR += list[id].cmp[0];
                colG += list[id].cmp[1];
                colB += list[id].cmp[2];
            }

            colR /= count;
            colG /= count;
            colB /= count;
            Debug.Assert(colR >= 0 && colR < 256, "Wrong color R");
            Debug.Assert(colG >= 0 && colG < 256, "Wrong color G");
            Debug.Assert(colB >= 0 && colB < 256, "Wrong color B");

            return new ColorRGB((byte)colR, (byte)colG, (byte)colB);
        }

        private void AddImage(ImagesCollection.Image collection)
        {
            if( null == collection || null == collection.image )
                return;

            ImageRawData data = new ImageRawData();
            data.originalImage = collection.image;
            data.sizeX = collection.image.width;
            data.sizeY = collection.image.height;
            data.pixels = collection.image.GetPixels32(MIP_LEVEL);
            data.variableName = collection.variableName;
            data.needPalette = ( collection.type == ImagesCollection.Image.Type.RGB );
            _imageRawData.Add(data);
        }

        private int DefineWidestRage(int r, int g, int b)
        {
            if( r >= g && r >= b )
                return 0;

            if( g >= r && g >= b )
                return 1;

            if( b >= r && b >= g )
                return 2;

            Debug.LogError("Impossible situation. Check Max of three definition");
            return 3;
        }

        private void CalcMinMax(ref Vector2Int minMax, int color)
        {
            minMax.x = Mathf.Min(minMax.x, color);
            minMax.y = Mathf.Max(minMax.y, color);
        }

        private class ColorComparer : IComparer<Color3>
        {
            public ColorComparer(int mode)
            {
                _mode = mode;
            }

            public int Compare(Color3 lhs, Color3 rhs)
            {
                if( lhs.cmp[_mode] < rhs.cmp[_mode] )
                    return 1;
                else if( lhs.cmp[_mode] > rhs.cmp[_mode] )
                    return -1;
                return 0;
            }

            private readonly int _mode;
        }
        

        private struct ColorRGB
        {
            public ColorRGB(Color32 col)
            {
                r = col.r;
                g = col.g;
                b = col.b;
            }

            public ColorRGB(byte _r, byte _g, byte _b)
            {
                r = _r;
                g = _g;
                b = _b;
            }

            public readonly byte r;
            public readonly byte g;
            public readonly byte b;
        }

        private struct Color3
        {
            public Color3(Color32 col)
            {
                cmp = new byte[3];
                cmp[0] = col.r;
                cmp[1] = col.g;
                cmp[2] = col.b;
            }

            public Color3(int r, int g, int b)
            {
                cmp = new byte[3];
                cmp[0] = (byte)r;
                cmp[1] = (byte)g;
                cmp[2] = (byte)b;
            }

            public readonly byte[] cmp;
        }

        private class ImageRawData
        {
            public Texture2D originalImage;
            public Texture2D palettedImage;
            public int sizeX;
            public int sizeY;
            public Color32[] pixels;
            public string variableName;
            public bool needPalette;
        }

        private const int MEDIAN_CUT_ITERATION_COUNT = 8;
        private const int MIP_LEVEL = 0;

        private readonly GraphicsData _data;
        private readonly List<ImageRawData> _imageRawData;
        private readonly HashSet<ColorRGB> _palette;
        private readonly Dictionary<ColorRGB, ColorRGB> _paletteMap;
        private readonly Dictionary<Texture2D, Texture2D> _originalToPalette;
    }
}
