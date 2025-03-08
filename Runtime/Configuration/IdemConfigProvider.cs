using UnityEditor;
using UnityEngine;

namespace Idem.Configuration
{
    public class IdemConfigProvider
    {
        private const string ResourceFolder = "Idem";
        private static readonly string _resourcePath = $"{ResourceFolder}/{nameof(IdemConfiguration)}";

        public static readonly IdemConfigProvider Default = new();

        public virtual IdemConfig GetConfig(bool silent = false)
        {
            var loaded = GetAsset();
            if (loaded == null)
            {
                Debug.LogWarning("[Idem] Configuration not found. Please create a configuration file.");
                return null;
            }

            return loaded.Config;
        }


#if UNITY_EDITOR
        public void CreateDefault()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            if (!AssetDatabase.IsValidFolder($"Assets/Resources/{ResourceFolder}"))
                AssetDatabase.CreateFolder("Assets/Resources", ResourceFolder);


            var config = ScriptableObject.CreateInstance<IdemConfiguration>();
            AssetDatabase.CreateAsset(config, $"Assets/Resources/{_resourcePath}.asset");
            AssetDatabase.SaveAssets();
        }
#endif

        public IdemConfiguration GetAsset()
        {
            return Resources.Load<IdemConfiguration>(_resourcePath);
        }
    }
}