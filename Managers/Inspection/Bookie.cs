using System;
using System.Collections.Generic;

namespace Managers.Inspection
{
    public enum FieldType
    {
        Country,
        Name,
        DOB,        
        Sex,        
        City,       
        Job,        
        Expiration, 
        Height,
        Weight
    }
    
    [Serializable]
    public class BookieEntry
    {
        public string id;
        public string type;
        public string title;
        public string contentTemplate;
        public string[] shownFields;
    }

    [Serializable]
    public class BookieDatabase
    {
        public List<BookieEntry> entries;
    }
    
    [Serializable]
    public class DocumentFieldData
    {
        public string key;  
        public string value;
    }
}