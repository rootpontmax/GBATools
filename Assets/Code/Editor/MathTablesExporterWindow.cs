using UnityEditor;
using UnityEngine;

namespace msSoft.GBATools.Editor
{
    public class MathTablesExporterWindow : EditorWindow
    {
        [MenuItem("GBATools/Editors/Math Tables Exporter Exporter")]
        private static void Init()
        {
            MathTablesExporterWindow window = EditorWindow.GetWindow<MathTablesExporterWindow>("Math Tables Exporter");
            window.Show();
        }

        private void OnGUI()
        {
            _data = EditorGUILayout.ObjectField("Math Tables Data", _data, typeof(MathTablesData), false) as MathTablesData;
            if (null == _data)
                return;

            if (GUILayout.Button("Export"))
            {
                string path = EditorUtility.OpenFolderPanel("Choose folder to save Math Tables", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    MathTablesExporter.Export(path, _data);
                }
            }
        }

        private MathTablesData _data;
        
    }
}
