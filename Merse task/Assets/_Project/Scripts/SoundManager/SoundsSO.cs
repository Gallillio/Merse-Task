using UnityEngine;

namespace SoundManager
{
    [CreateAssetMenu(menuName = "Sound SO", fileName = "Sounds SO")]
    public class SoundsSO : ScriptableObject
    {
        public SoundList[] sounds;
    }
}