using System;
using UnityEngine;

namespace msSoft.GBATools
{
    [CreateAssetMenu(menuName = "GBATools/Assets/Image Set")]
    public class ImageSet : ScriptableObject
    {
        [Serializable]
        public class Image
        {
            public enum Type
            {
                RGB,
                Byte
            }
            public string variableName;
            public Texture2D image;
            [HideInInspector] public byte[] compressedImage;
            public Type type;
        }

        public string paletteName;
        public Image[] images;

        [HideInInspector] public byte[] palette;
    }
}
