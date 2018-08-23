using System;
using Gtk;
namespace MyData
{
    public class Field2Gui
    {
        static public VBox NewPage(string pagename){
            VBox ret = new VBox();
            MainClass.Panels.Add(ret);
            MainClass.Pages.AppendPage(ret, new Label(pagename));
            return ret;
        }
    }
}
