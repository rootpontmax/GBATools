using System;
using UnityEngine;

namespace msSoft.GBATools
{
    [CreateAssetMenu(menuName = "GBATools/Assets/Images Collection")]
    public class ImagesCollection : ScriptableObject
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
            public Type type;
        }

        public Image[] images;
    }
}
