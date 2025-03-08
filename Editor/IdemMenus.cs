using Idem.Configuration;
using UnityEditor;

namespace Idem.Editor
{
    public static class IdemMenus
    {
        [MenuItem("Idem/Configuration")]
        public static void OpenConfiguration()
        {
            var existing = IdemConfigProvider.Default.GetAsset();
            if (existing == null)
            {
                IdemConfigProvider.Default.CreateDefault();
                existing = IdemConfigProvider.Default.GetAsset();
            }

            Selection.activeObject = existing;
        }
    }
}