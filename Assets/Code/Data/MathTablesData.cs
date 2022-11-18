using UnityEngine;

namespace msSoft.GBATools
{
    [CreateAssetMenu(menuName = "GBATools/Assets/Math Tables")]
    public class MathTablesData : ScriptableObject
    {
        public string filename = "MathTables";
        public int fractionalBitCount;
        public int angleTableCount;
        public bool hasSinTableFloat;
        public bool hasSinTableFixed;

        public void OnValidate()
        {
            fractionalBitCount = Mathf.Clamp(fractionalBitCount,0,32);
            angleTableCount = Mathf.Max(0, angleTableCount);            
        }
    }
}
