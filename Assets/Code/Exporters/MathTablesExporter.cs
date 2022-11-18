using System.IO;
using System.Text;
using UnityEngine;

namespace msSoft.GBATools
{
    public class MathTablesExporter
    {
        public static void Export(string path, MathTablesData data)
        {
            string filenameH = path + "/" + data.filename + ".h";
            string filenameC = path + "/" + data.filename + ".c";
            StringBuilder builderH = new StringBuilder(4096);
            StringBuilder builderC = new StringBuilder(4096);

            WriteHeader(data.filename, builderH, builderC);

            ExportSinTable(data, builderH, builderC);

            ExportTools.WriteSeparator(builderH);
            ExportTools.WriteSeparator(builderC);
            builderH.AppendLine("#endif");
            File.WriteAllText(filenameH, builderH.ToString());
            File.WriteAllText(filenameC, builderC.ToString());
        }

        private static void WriteHeader(string name, StringBuilder builderH, StringBuilder builderC)
        {
            string defineName = name.ToUpper() + "_INCLUDED";
            builderH.AppendLine("#ifndef " + defineName);
            builderH.AppendLine("#define " + defineName);
            builderC.AppendLine("#include \"" + name + ".h\"");
        }

        private static void ExportSinTable(MathTablesData data, StringBuilder builderH, StringBuilder builderC)
        {
            if (!data.hasSinTableFixed && !data.hasSinTableFloat)
                return;

            ExportTools.WriteSeparator(builderH);
            ExportTools.WriteSeparator(builderC);

            float angleStep = 360.0f / data.angleTableCount;
            float[] sinTable = new float[data.angleTableCount];
            for (int i = 0; i < data.angleTableCount; ++i)
            {
                float angleRad = angleStep * i * Mathf.Deg2Rad;
                sinTable[i] = Mathf.Sin(angleRad);
            }

            builderH.AppendLine("#define " + SIN_TABLE_COUNT_NAME + " " + data.angleTableCount);

            if( data.hasSinTableFloat )
            {
                string arraySizeStr = "[" + SIN_TABLE_COUNT_NAME + "]";
                builderH.AppendLine("extern " + SIN_TABLE_FLOAT_TYPE + arraySizeStr + ";");
                builderC.AppendLine(SIN_TABLE_FLOAT_TYPE + arraySizeStr + " =");
                builderC.Append("{");
                for (int i = 0; i < data.angleTableCount; ++i )
                {
                    if (0 == i % 8)
                        builderC.Append("\n    ");
                    string floatValue = sinTable[i].ToString("0.00000000");
                    builderC.Append(floatValue + "f");

                    if (i + 1 < data.angleTableCount)
                        builderC.Append(",");
                    else
                        builderC.Append("\n");
                }
                builderC.AppendLine("};");

            }
        }

        private static readonly string SIN_TABLE_COUNT_NAME = "SIN_TABLE_COUNT";
        private static readonly string SIN_TABLE_FIXED_TYPE = "const fixed g_sinTableFixed";
        private static readonly string SIN_TABLE_FLOAT_TYPE = "const float g_sinTableFloat";
    }
}
