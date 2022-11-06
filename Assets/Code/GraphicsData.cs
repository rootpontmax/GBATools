using UnityEngine;

namespace msSoft.GBATools
{
    [CreateAssetMenu(menuName = "GBATools/Assets/Graphics Data")]
    public class GraphicsData : ScriptableObject
    {
        public string filename;
        public ImagesCollection[] imagesCollections;
    }
}
