using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace msSoft.GBATools
{
    public static class GraphicsDataExporter
    {
        public static void Export(string path, GraphicsData data)
        {
            if( null == data )
                return;
            
            string filenameH = path + "/" + data.filename + ".h";
            string filenameC = path + "/" + data.filename + ".c";

            StringBuilder builderH = new StringBuilder(4096);
            StringBuilder builderC = new StringBuilder(4096);
            WriteIncludeHeader(builderH, builderC, data.filename);

            if( null != data.imageSets )
                for( int i = 0; i < data.imageSets.Length; ++i )
                    ExportImageSet(builderH, builderC, data.imageSets[i]);

            // Close preprocessor and save
            WriteSeparator(builderH);
            WriteSeparator(builderC);
            builderH.AppendLine("#endif");
            File.WriteAllText(filenameH, builderH.ToString());
            File.WriteAllText(filenameC, builderC.ToString());
        }

        private static void WriteIncludeHeader(StringBuilder builderH, StringBuilder builderC, string name)
        {
            string defineName = name.ToUpper() + "_INCLUDED";
            builderH.AppendLine("#ifndef " + defineName);
            builderH.AppendLine("#define " + defineName);
            builderH.AppendLine("\n#include <stdint.h>\n");
            builderC.AppendLine("#include \"" + name + ".h\"");
        }

        private static void ExportImageSet(StringBuilder builderH, StringBuilder builderC, ImageSet set)
        {
            if( null == set )
                return;

            WritePalette(builderH, builderC, set);
            for( int i = 0; i < set.images.Length; ++i )
                WriteImage(builderH, builderC, set.images[i]);
        }

        private static void WritePalette(StringBuilder builderH, StringBuilder builderC, ImageSet set)
        {
            WriteSeparator(builderH);
            WriteSeparator(builderC);
            builderH.AppendLine(EXTERN_TYPE + PALETTE_TYPE + set.paletteName + "[256];");
            builderC.AppendLine(PALETTE_TYPE + set.paletteName + "[256] =");
            builderC.Append("{");
            for( int i = 0; i < 256; ++i )
            {
                if( 0 == i % 16 )
                    builderC.Append("\n    ");

                int id = i * 3;
                int r = set.palette[id    ];
                int g = set.palette[id + 1];
                int b = set.palette[id + 2];
                r >>= 3;
                g >>= 3;
                b >>= 3;
                int color = r | ( g << 5 ) | ( b << 10 );
                Debug.Assert(color >= 0 && color < 32768, "Wrong color value");

                ushort byteToStore = Convert.ToUInt16(color);
                builderC.Append("0x" + byteToStore.ToString("x4"));
                if( i < 255 )
                    builderC.Append(",");
                else
                    builderC.Append("\n");
            }
            builderC.AppendLine("};");
        }

        private static void WriteImage(StringBuilder builderH, StringBuilder builderC, ImageSet.Image image)
        {
            WriteSeparator(builderH);
            WriteSeparator(builderC);

            int sizeX = image.image.width;
            int sizeY = image.image.height;
            int count = sizeX * sizeY;

            builderH.AppendLine(DEFINE_TYPE + image.variableName + "_SIZE_X " + sizeX);
            builderH.AppendLine(DEFINE_TYPE + image.variableName + "_SIZE_Y " + sizeY);
            builderH.AppendLine(DEFINE_TYPE + image.variableName + "_SIZE " + count);
            builderH.AppendLine(EXTERN_TYPE + IMAGE_TYPE + image.variableName + "[" + count + "];");
            builderC.AppendLine(IMAGE_TYPE + image.variableName + "[" + count + "] =");
            builderC.Append("{");

            for( int i = 0; i < image.compressedImage.Length; ++i )
            {
                if( 0 == i % 16 )
                    builderC.Append("\n    ");

                builderC.Append("0x" + image.compressedImage[i].ToString("x2"));

                if( i + 1 <= image.compressedImage.Length )
                    builderC.Append(",");
                else
                    builderC.Append("\n");
            }
            builderC.AppendLine("};");
        }

        private static void WriteSeparator(StringBuilder builder)
        {
            for( int i = 0; i < 100; ++i )
                builder.Append("/");
            builder.Append("\n");
        }


        private static readonly string EXTERN_TYPE = "extern ";
        private static readonly string PALETTE_TYPE = "const uint16_t ";
        private static readonly string IMAGE_TYPE = "const uint8_t ";
        private static readonly string DEFINE_TYPE = "#define ";


    }
}
