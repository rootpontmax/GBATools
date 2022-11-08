namespace msSoft.GBATools
{
    public class GraphicsConverter
    {
        public static void Convert(GraphicsData data)
        {
            if( null == data )
                return;

            if( null != data.imageSets )
                for( int i = 0; i < data.imageSets.Length; ++i )
                    ImageSetConverter.Convert(data.imageSets[i]);

            

            //string path = EditorUtility.OpenFolderPanel("Choose folder to save template", "", "");
            //string fullPath = path + "/" + data.filename + ".h";
            //Debug.LogError(fullPath);
            
            //TextAsset textAsset = 
        }
    }
}
