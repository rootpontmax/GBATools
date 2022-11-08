using UnityEngine;

namespace msSoft.GBATools.Editor
{
    public class ImageSetEditorData
    {
        public class Image
        {
            public Image(string _n, Texture2D o, Texture2D i)
            {
                name = _n;
                original = o;
                indexed = i;
            }
            public readonly string name;
            public readonly Texture2D original;
            public readonly Texture2D indexed;
            public bool foldout;
        }


        public readonly string paletteName;
        public readonly Texture2D paletteTexture;
        public readonly Image[] images;
        public bool paletteFoldout;

        public ImageSetEditorData(ImageSet set)
        {
            paletteName = set.paletteName;
            paletteTexture = CreatePalette(set);
            images = CreateImages(set);
        }

        private static Texture2D CreatePalette(ImageSet set)
        {
            if( null == set || null == set.palette )
                return null;

            
            Color32[] colors = new Color32[256];
            for( int i = 0; i < 256; ++i )
            {
                int id = i * 3;
                byte r = set.palette[id    ];
                byte g = set.palette[id + 1];
                byte b = set.palette[id + 2];
                int x = i % 16;
                int y = 15 - i / 16;
                int pixelOffset = y * 16 + x;
                colors[pixelOffset] = new Color32(r, g, b, 255);
            }


            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGB24, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(colors);
            texture.Apply(false, true);

            return texture;
        }

        private Image[] CreateImages(ImageSet set)
        {
            if( null == set || null == set.images )
                return null;

            Image[] array = new Image[set.images.Length];
            for( int i = 0; i < set.images.Length; ++i )
            {
                string name = set.images[i].variableName;
                Texture2D original = set.images[i].image;
                Texture2D indexed = ( ImageSet.Image.Type.RGB == set.images[i].type ) ?
                                    CreateIndexedTexture(set.images[i].compressedImage, set.palette, set.images[i].image.width, set.images[i].image.height) :
                                    null;

                Image image = new Image(name, original, indexed);
                array[i] = image;
            }

            return array;
        }

        private Texture2D CreateIndexedTexture(byte[] compressedImage, byte[] palette, int sizeX, int sizeY)
        {
            int count = sizeX * sizeY;
            Texture2D texture = new Texture2D(sizeX, sizeY, TextureFormat.RGB24, false);
            Color32[] colors32 = new Color32[count];
            for( int i = 0; i < count; ++i )
            {
                int id = compressedImage[i] * 3;
                byte r = palette[id    ];
                byte g = palette[id + 1];
                byte b = palette[id + 2];

                colors32[i].r = r;
                colors32[i].g = g;
                colors32[i].b = b;
                colors32[i].a = 255;
            }
            texture.SetPixels32(colors32);
            texture.filterMode = FilterMode.Point;
            texture.Apply(false, true);

            return texture;
        }

    }
}
