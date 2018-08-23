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

        static public void NewString(VBox pg,string name){
            // Gui
            var tbox = new HBox();
            tbox.Add(new Label("string"));
            tbox.Add(new Label(name));
            var ttxt = new TextView();
            var col1 = new Gdk.Color(); var suc1 = Gdk.Color.Parse("#b400ff",ref col1); 
            var col2 = new Gdk.Color(); var suc2 = Gdk.Color.Parse("#0b000f",ref col2);
            if (!suc1) tbox.Add(new Label("FG color parse failure!")); // debug only
            if (!suc2) tbox.Add(new Label("BG color parse failure!")); // debug only
            ttxt.BorderWidth = 2;
            ttxt.ModifyBg(StateType.Normal,col2);
            ttxt.ModifyFg(StateType.Normal,col1);
            MainClass.DStrings[name] = ttxt;
            tbox.Add(ttxt);
            pg.Add(tbox);
            // Database
            MainClass.Base.fields[name] = "string";
            MainClass.Base.defaults[name] = "";
        }

        static public void NewNumber(VBox pg, string numbertype, string name){
            // Gui
            var tbox = new HBox();
            tbox.Add(new Label(numbertype));
            tbox.Add(new Label(name));
            var ttxt = new TextView();
            var col1 = new Gdk.Color(); var suc1 = Gdk.Color.Parse("#00b4ff", ref col1);
            var col2 = new Gdk.Color(); var suc2 = Gdk.Color.Parse("#000b0f", ref col2);
            if (!suc1) tbox.Add(new Label("FG color parse failure!")); // debug only
            if (!suc2) tbox.Add(new Label("BG color parse failure!")); // debug only
            ttxt.BorderWidth = 2;
            ttxt.ModifyBg(StateType.Normal, col2);
            ttxt.ModifyFg(StateType.Normal, col1);
            MainClass.DStrings[name] = ttxt; // This is only for widget storage... The types only come into play when exporting.
            tbox.Add(ttxt);
            pg.Add(tbox);
            // Database
            MainClass.Base.fields[name] = numbertype;
            MainClass.Base.defaults[name] = "";
        }

        static public void NewBool(VBox pg,string name){
            var but1 = new RadioButton("True");
            var but2 = new RadioButton(but1, "False");
            var tbox = new HBox();
            tbox.Add(new Label("bool"));
            tbox.Add(new Label(name));
            tbox.Add(but1);
            tbox.Add(but2);
            pg.Add(tbox);
            MainClass.RBTbools[name] = but1;
            MainClass.RBFbools[name] = but2;
            MainClass.Base.fields[name] = "bool";
            MainClass.Base.defaults[name] = "TRUE";
        }

        static public TreeView NewMC(VBox pg,string name){
            var mmc = new TreeView();
            var tbox = new HBox();
            var tvc = new TreeViewColumn();
            var NameCell = new CellRendererText();
            tvc.PackStart(NameCell, true);
            tvc.AddAttribute(NameCell, "text", 0);
            mmc.AppendColumn(tvc);
            tvc.Title = name;
            tbox.Add(new Label("mc")); // FYI: mc = Multiple Choice
            tbox.Add(new Label(name));
            tbox.Add(mmc);
            pg.Add(tbox);
            MainClass.Base.fields[name] = "mc";
            MainClass.Base.defaults[name] = "";
            return mmc;
        }

    }
}
