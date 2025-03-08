using UnityEngine;

namespace Idem.Configuration
{
    public class IdemConfiguration : ScriptableObject
    {
        [SerializeField] private IdemConfig config;

        public IdemConfig Config => config;
    }
}