using System;
using System.Collections.Generic;

namespace MyData
{

    public class MyRecord {
        public SortedDictionary<string, string> value = new SortedDictionary<string, string>(); 
    }


    public class DataBase
    {
        public SortedDictionary<string, MyRecord> Record = new SortedDictionary<string, MyRecord>();
        public SortedDictionary<string, string> MyStructure = new SortedDictionary<string, string>();
        public DataBase()
        {

        }
    }
}
