using System.Collections.Generic;
using UnityEngine;

namespace msSoft.GBATools
{
    public static class ImageSetConverter
    {
        public static void Convert(ImageSet set)
        {
            if( null == set || null == set.images )
                return;

            List<ImageRawData> datas = CollectImages(set);
            Palette palette = CreatePalette(datas);
            SaveDataToSet(set, datas, palette);
        }

        private static List<ImageRawData> CollectImages(ImageSet set)
        {
            List<ImageRawData> list = new List<ImageRawData>();
            for( int i = 0; i < set.images.Length; ++i )
                list.Add( GetRawImageData(set.images[i]) );

            return list;
        }

        private static Palette CreatePalette(List<ImageRawData> datas)
        {
            // Collect all colors that have to be resolved by palette
            HashSet<ColorRGB> colorSet = new HashSet<ColorRGB>();
            for( int i = 0; i < datas.Count; ++i )
                if( datas[i].needPalette )
                    for( int j = 0; j < datas[i].pixels.Length; ++j )
                    {
                        ColorRGB cRGB = new ColorRGB(datas[i].pixels[j]);
                        colorSet.Add(cRGB);
                    }

            // Create colorList
            List<C3> colorList = new List<C3>(colorSet.Count);
            foreach(var item in colorSet)
            {
                C3 c3 = new C3(item);
                colorList.Add(c3);
            }

            // Iteration sorting
            Palette palette = new Palette();
            SortColorList(colorList, palette, 0, colorList.Count, MEDIAN_CUT_ITERATION_COUNT);

            // Sort palette and finalize it
            foreach(var item in palette.set)
                palette.colors.Add(item);

            // TODO: Sort palette.colors somehow

            for( int i = 0; i < palette.colors.Count; ++i )
                palette.colorToID.Add(palette.colors[i], i);

            return palette;
        }

        private static void SaveDataToSet(ImageSet set, List<ImageRawData> datas, Palette palette)
        {
            SavePalette(set, palette);
            for( int i = 0; i < set.images.Length; ++i )
                SaveData(set.images[i], datas[i], palette);
        }

        private static void SavePalette(ImageSet set, Palette palette)
        {
            set.palette = new byte[768];
            for( int i = 0; i < palette.colors.Count; ++i )
            {
                int id = i * 3;
                set.palette[id    ] = palette.colors[i].r;
                set.palette[id + 1] = palette.colors[i].g;
                set.palette[id + 2] = palette.colors[i].b;
            }
        }

        private static void SaveData(ImageSet.Image image, ImageRawData data, Palette palette)
        {
            int count = data.sizeX * data.sizeY;
            image.compressedImage = new byte[count];
            if( data.needPalette )
            {
                for( int i = 0; i < data.pixels.Length; ++i )
                {
                    ColorRGB originalColor = new ColorRGB(data.pixels[i]);
                    ColorRGB indexedColor = palette.map[originalColor];
                    int colorID = palette.colorToID[indexedColor];
                    Debug.Assert(colorID >= 0 && colorID < 256, "Wrong color index");
                    image.compressedImage[i] = (byte)colorID;
                }
            }
            else
            {
                for( int i = 0; i < data.pixels.Length; ++i )
                    image.compressedImage[i] = data.pixels[i].r;
            }
        }

        private static void SortColorList(List<C3> list, Palette palette, int startID, int count, int iterationCount)
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
                    palette.set.Add(avgCol);
                    palette.map.Add(colRGB, avgCol);
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
                IComparer<C3> comparer = new ColorComparer(mode);
                list.Sort(startID, count, comparer);

                // Split range and start process again
                bool isOdd = ( count % 2 ) > 0;
                int countL = count / 2;
                int countR = isOdd ? count - countL : countL;
                Debug.Assert(count == countL + countR, "Counts calculates wrong");
                int idL = startID;
                int idR = idL + countL;
                --iterationCount;

                SortColorList(list, palette, idL, countL, iterationCount);
                SortColorList(list, palette, idR, countR, iterationCount);
            }
        }

        private static int DefineWidestRage(int r, int g, int b)
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

        private static void CalcMinMax(ref Vector2Int minMax, int color)
        {
            minMax.x = Mathf.Min(minMax.x, color);
            minMax.y = Mathf.Max(minMax.y, color);
        }

        private static ColorRGB DefineAverageColor(List<C3> list, int startID, int count)
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

        private static ImageRawData GetRawImageData(ImageSet.Image collection)
        {
            if( null == collection || null == collection.image )
                return null;

            ImageRawData data = new ImageRawData();
            data.originalImage = collection.image;
            data.sizeX = collection.image.width;
            data.sizeY = collection.image.height;
            data.pixels = collection.image.GetPixels32(MIP_LEVEL);
            data.variableName = collection.variableName;
            data.needPalette = ( collection.type == ImageSet.Image.Type.RGB );
            return data;
            
        }

        private class Palette
        {
            public readonly HashSet<ColorRGB> set = new HashSet<ColorRGB>();
            public readonly Dictionary<ColorRGB, ColorRGB> map = new Dictionary<ColorRGB, ColorRGB>();


            public readonly List<ColorRGB> colors = new List<ColorRGB>();
            public readonly Dictionary<ColorRGB, int> colorToID = new Dictionary<ColorRGB, int>();
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

        private class ColorComparer : IComparer<C3>
        {
            public ColorComparer(int mode)
            {
                _mode = mode;
            }

            public int Compare(C3 lhs, C3 rhs)
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

        private struct C3
        {
            public C3(ColorRGB col)
            {
                cmp = new byte[3];
                cmp[0] = col.r;
                cmp[1] = col.g;
                cmp[2] = col.b;
            }

            public readonly byte[] cmp;
        }

        private const int MEDIAN_CUT_ITERATION_COUNT = 8;
        private const int MIP_LEVEL = 0;
    }
}
