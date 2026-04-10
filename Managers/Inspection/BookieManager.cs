using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Managers.Inspection
{
    public static class BookieManager
    {
        private static Dictionary<string, BookieEntry> _documentDictionary = new Dictionary<string, BookieEntry>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBookie()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("JSONs/Bookie");


            if (jsonFile != null)
            {
                BookieDatabase database = JsonUtility.FromJson<BookieDatabase>(jsonFile.text);

                _documentDictionary.Clear();
                foreach (var entry in database.entries)
                {
                    _documentDictionary.Add(entry.id, entry);
                }
            }
        }

        public static BookieEntry GetEntry(string id)
        {
            if (_documentDictionary.TryGetValue(id, out BookieEntry entry))
            {
                return entry;
            }
            
            Debug.LogError($"Belge ID'si bulunamadı: {id}");
            return null;
        }
    }
}