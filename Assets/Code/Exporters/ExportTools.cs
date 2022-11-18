using System.Text;

namespace msSoft.GBATools
{
    public static class ExportTools
    {
        public static void WriteSeparator(StringBuilder builder)
        {
            for( int i = 0; i < 100; ++i )
                builder.Append("/");
            builder.Append("\n");
        }
    }
}
