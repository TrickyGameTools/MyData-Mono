using System;
using System.Collections.Generic;
using Gtk;

namespace MyData
{

    public class MyRecord {
        public SortedDictionary<string, string> value = new SortedDictionary<string, string>();
        public bool MODIFIED = false;
    }


    public class MyDataBase
    {
        public SortedDictionary<string, MyRecord> Record = new SortedDictionary<string, MyRecord>();
        public SortedDictionary<string, string> MyStructure = new SortedDictionary<string, string>();

        public static bool Load(string filename){
            bool ret = true;
            string[] lines;
            try{
                lines = System.IO.File.ReadAllLines(filename);
            } catch {
                MessageDialog md = new MessageDialog(MainClass.win,
                                DialogFlags.DestroyWithParent, MessageType.Error,
                                ButtonsType.Close, "Error loading file");
                md.Run();
                md.Destroy();
                return false;
            }
            // loader comes here later!
            return ret;
        }

        public void Save(string filename){
            // saver comes here later!
        }

        public DataBase(){

        }
    }
}
