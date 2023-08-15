using System.Collections.Generic;

namespace Utilities
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [System.Serializable]
        public class SerializableDictionaryObject
        {
            public TKey Key;
            public TValue Value;
        }

        public List<SerializableDictionaryObject> Dictionary = new ();
    }
}