using System;
using System.IO;
using UnityEditor;

namespace CZCIT.SolarGlare.Editor
{
    public static class SolarGlareExport
    {
        [MenuItem("Tools/CZCIT Solar Glare/Export Package")]
        public static void Export()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string output = Path.Combine(desktop, "CZCIT-SolarGlare-26.11.17.unitypackage");
            AssetDatabase.ExportPackage("Assets/CZCIT Solar Glare", output, ExportPackageOptions.Recurse);
        }
    }
}
