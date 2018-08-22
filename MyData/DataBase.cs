using System;
using System.Collections.Generic;

namespace MyData
{

    public class MyRecord {
        public SortedDictionary<string, string> value = new SortedDictionary<string, string>();
        public bool MODIFIED = false;
    }


    public class DataBase
    {
        public SortedDictionary<string, MyRecord> Record = new SortedDictionary<string, MyRecord>();
        public SortedDictionary<string, string> MyStructure = new SortedDictionary<string, string>();

        public void Load(string filename){
            // loader comes here later!
        }

        public void Save(string filename){
            // saver comes here later!
        }

        public DataBase()
        {

        }
    }
}
