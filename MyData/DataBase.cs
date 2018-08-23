using System;
using System.Collections.Generic;
using Gtk;

namespace MyData
{

    public class MyRecord {
        public SortedDictionary<string, string> value = new SortedDictionary<string, string>();
        public bool MODIFIED = false;
    }

    public class MyBase {
        public Dictionary<string, string> fields = new Dictionary<string, string>();
        public Dictionary<string, string> defaults = new Dictionary<string, string>();
        public SortedDictionary<string, MyRecord> records = new SortedDictionary<string,MyRecord>();
    }


    public class MyDataBase
    {
        public SortedDictionary<string, MyRecord> Record = new SortedDictionary<string, MyRecord>();
        public SortedDictionary<string, string> MyStructure = new SortedDictionary<string, string>();

        static void CRASH(string message){
            MessageDialog md = new MessageDialog(MainClass.win,
                            DialogFlags.DestroyWithParent, MessageType.Error,
                            ButtonsType.Close, message);
            md.Run();
            md.Destroy();
        }

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
            int linecount = 0;
            string TL;
            string Chunk = "UNKNOWN";
            int cpage = 0;
            int pagey = 0; // Not sure if this is needed, but I need to be sure
            string pagename = "";
            VBox CurrentPanel;
            foreach(string L in lines){
                linecount++;
                TL = L.Trim();
                if (TL.Length > 0 && TL.Substring(0, 1) != "#" && TL.Substring(0, 2) != "//" && TL.Substring(0, 2) != "--")
                { // No empty lines and no comment either.
                    if (TL.Substring(0, 1) == "[" && TL.Substring(TL.Length - 1, 1) == "]")
                    {

                        if (TL.ToUpper() == "[SYSTEM]")
                        {
                            Chunk = "System";
                        }
                        else if (TL.Length >= 6 && TL.ToUpper().Substring(0, 6) == "[PAGE:")
                        {
                            Chunk = "Structure";
                            cpage++;
                            pagey = 0;
                            pagename = TL.Substring(6, TL.Length - 7).Trim();
                            CurrentPanel = Field2Gui.NewPage(pagename);
                        }

                        else if (TL.ToUpper() == "[RECORDS]")
                        {
                            Chunk = "Records";
                        }
                        else if (TL.ToUpper() == "[DEFAULTS]")
                        {
                            Chunk = "Default";
                        }
                        else if (TL.Length > 7 && TL.ToUpper().Substring(0, 7) == "[ALLOW:")
                        {
                            Chunk = "Allow";
                        }
                        else
                        {
                            CRASH("Unknown Chunk definition in line #" + linecount + "\n\n" + TL);
                            return false;
                        }
                    }
                }
            }
            // loader comes here later!
            return ret;
        }

        public void Save(string filename){
            // saver comes here later!
        }

        public void DataBase(){

        }
    }
}


/* Below is the ORIGINAL BlitzMax code of the database parser.
   It has to serve to help me a bit to set this up all well in C#

For L=EachIn LF
    linecount:+1
    TL = Trim(L)
    If Trim(L) And Left(Trim(L),1)<>"#" And Left(Trim(L),2)<>"//" And Left(Trim(L),2)<>"--" ' No empty lines and no comments either!
        If Left(TL,1)="[" And Right(TL,1)="]"
            If Upper(TL)="[SYSTEM]" 
                Chunk="System"
            ElseIf Upper(Left(TL,6))="[PAGE:"
                Chunk = "Structure"
                cpage:+1
                pagey=0
                Panels[cpage] = CreatePanel(0,0,tw,th,Tabber)
                HideGadget Panels[cpage]
                AddGadgetItem tabber,Mid(TL,7,Len(TL)-7)
                pagename = Trim(Mid(TL,7,Len(TL)-7))
            ElseIf Upper(TL)="[RECORDS]"
                Chunk = "Records"
            ElseIf Upper(TL)="[DEFAULT]"
                chunk = "Default"
            ElseIf Upper(Left(TL,7))="[ALLOW:"
                chunk = "Allow"
                AllowList = New TList
                MapInsert allow,Trim(Mid(TL,8,Len(TL)-8)),allowlist
                Print "Start Allow: "+Trim(Mid(TL,8,Len(TL)-8))
            Else 
                Error "Unknown [ ] definition: "+TL 
                EndIf
        Else
            Select Chunk
                Case "System"
                    If TL.find("=")=-1 error("Invalid line: "+TL)
                    SL = TL.split("=")
                    Select Upper(Trim(SL[0]))
                        Case "OUTPUTLUAREC"
                            OutputLuaRec = Trim(SL[1])
                        Case "OUTPUTLUABASE"
                            OutputLuaBase = Trim(SL[1])
                        Case "OUTPUTPYTHONREC"
                            outputpythonrec = Trim(SL[1])
                        Case "OUTPUTPYTHONBASE"
                            outputpythonbase = Trim(SL[1])
                        Case "OUTPUTMYSQLBASE"
                            outputmysqlbase = Trim(SL[1])
                        Case "LICENSE"
                            outlicense = Trim(SL[1])    
                        Case "AUTOOUTPUT"
                            AutoOutput = Trim(Upper(SL[1]))="YES" Or Trim(Upper(SL[1]))="TRUE"
                        Case "REMOVENONEXISTENTFIELDS"
                            RemoveNonExistent = Trim(Upper(SL[1]))="YES" Or Trim(Upper(SL[1]))="TRUE"
                        Default 
                            Error"Unknown variable: "+Trim(SL[0])
                        End Select
                Case "Structure"
                    TTL = Replace(TL,"~t"," ")
                    Repeat
                    TL = TTL
                    TTL = Replace(TTL,"  "," ")
                    Until TTL=TL                    
                    If TL.find(" ")=-1 And Left(tl,6).tolower()<>"strike" error "Invalid line: "+TL+"~nline #"+Linecount
                    SL = TL.split(" ")
                    If Left(SL[0],1)<>"@" And Left(tl,6).tolower()<>"strike" And Left(tl,4).tolower()<>"info"
                        CreateLabel Lower(SL[0]),0,pagey,250,15,panels[cpage]
                        CreateLabel SL[1],250,pagey,250,15,panels[cpage]
                        If MapContains(fields,SL[1]) error "Duplicate field: "+SL[1]
                        MapInsert fields,SL[1],SL[0]
                        MapInsert fieldonpage,SL[1],pagename
                    ElseIf Len(SL)>1
                        SL[1] = Replace(SL[1],"\space"," ")
                        EndIf                                   
                    Select Lower(SL[0])
                        Case "strike"
                            CreateLabel("---",0,pagey,ClientWidth(panels[cpage]),25,panels[cpage],LABEL_SEPARATOR)
                            pagey:+25
                        Case "info"
                              CreateLabel Right(TL,Len(TL)-5),0,pagey,1000,25,panels[cpage]
                            pagey:+25
                        Case "string"
                            MapInsert recgadget,SL[1],CreateTextField(500,pagey,500,25,panels[cpage])
                            pagey:+25
                        Case "int","double"
                            MapInsert recgadget,SL[1],CreateTextField(500,pagey,250,25,panels[cpage])
                            pagey:+25
                        Case "color"
                            CreateLabel "R:",500,Pagey,50,25,panels[cpage]
                            MapInsert RecGadget,SL[1]+".Red",CreateTextField(550,pagey,50,25,Panels[cpage])
                            CreateLabel "G:",610,Pagey,50,25,panels[cpage]
                            MapInsert RecGadget,SL[1]+".Green",CreateTextField(660,pagey,50,25,Panels[cpage])
                            CreateLabel "B:",720,Pagey,50,25,panels[cpage]
                            MapInsert RecGadget,SL[1]+".Blue",CreateTextField(770,pagey,50,25,Panels[cpage])
                            MapInsert RecGadget,SL[1]+".Pick",CreateButton("Pick",880,pagey,80,25,Panels[cpage])
                            MapInsert MapColor,GadField(SL[1]+".Pick"),SL[1]
                            SetGadgetColor GadField(SL[1]+".Red")  ,255,180,180
                            SetGadgetColor GadField(SL[1]+".Green"),180,255,180
                            SetGadgetColor GadField(SL[1]+".Blue") ,180,180,255
                            pagey:+25
                        Case "bool"
                            MapInsert RecGadget,SL[1]+".Panel",CreatePanel(500,pagey,400,25,Panels[cpage])
                            MapInsert RecGadget,SL[1]+".True",CreateButton("True",0,0,200,25,gadfield(SL[1]+".Panel"),Button_radio)
                            MapInsert RecGadget,SL[1]+".False",CreateButton("False",200,0,200,25,gadfield(SL[1]+".Panel"),Button_radio)
                            pagey:+25
                        Case "mc"
                            MapInsert recgadget,SL[1],CreateComboBox(500,pagey,500,25,panels[cpage])
                            lastlist = gadfield(SL[1])
                            pagey:+25
                        Case "@noextfilter"
                            OnlyAllowExt = ""
                            OnlyAllowExtList = Null 
                        Case "@nopathfilter","@nodirfilter"
                            OnlyAllowPath = ""
                            OnlyAllowPathList = Null
                        Case "@extfilter"
                            OnlyAllowExt = Upper(Trim(Right(TL,Len(TL)-Len("@extfilter "))))
                            OnlyAllowExtList = ListFromArray(OnlyAllowExt.split(","))   
                        Case "@pathfilter"
                            OnlyAllowPath = Upper(Trim(Right(TL,Len(TL)-Len("@pathfilter "))))
                            OnlyAllowPathList = ListFromArray(OnlyAllowpath.split(",")) 
                        Case "@dirfilter"
                            OnlyAllowPath = Upper(Trim(Right(TL,Len(TL)-Len("@dirfilter "))))
                            OnlyAllowPathList = ListFromArray(OnlyAllowPath.split(","))
                        Case "@prefix"                          
                            OnlyAllowPrefix = Upper(Trim(Right(TL,Len(TL)-Len("@prefix "))))
                        Case "@db"  
                                If Not lastlist error "No list to add this item to in line #"+linecount
                            Print "Importing database: "+Trim(Right(TL,Len(TL)-4))
                            For Local l$=EachIn Listfile("Databases/"+Trim(Right(TL,Len(TL)-4)))
                                Local readrec=False
                                If Upper(l)="[RECORDS]" 
                                    readrec=True
                                ElseIf Left(l,1)="["
                                    readrec=False
                                EndIf   
                                If Prefixed(l,"Rec: ")  AddGadgetItem lastlist,Trim(Right(l,Len(l)-4))
                            Next
                        Case "@f"
                            If Not lastlist error "No list to add this item to in line #"+linecount
                            Print "Importing JCR: "+Trim(Right(TL,Len(TL)-3))
                            Local JD:TJCRDir = JCR_Dir(Trim(Right(TL,Len(TL)-3)))
                            Ok = True
                            If Not JD error "JCR could not read file: "+Trim(Right(TL,Len(TL)-3))
                            'For Local F$=EachIn OnlyAllowPathlist DebugLog "Dir Allowed: "+f; Next
                            'For Local F$=EachIn OnlyAllowextlist  DebugLog "Ext Allowed: "+F; Next
                            For Local D:TJCREntry=EachIn MapValues(JD.Entries)
                                  Rem Old and inefficient
                                If Not OnlyAllowExtList
                                    Print "Adding entry: "+D.FileName
                                    AddGadgetItem lastlist,D.FileName
                                ElseIf ListContains(OnlyAllowExtList,Upper(ExtractExt(D.FileName)   ))
                                    Print "Adding entry: "+D.FileName+" (approved by the ext filter)"
                                    AddGadgetItem lastlist,D.FileName
                                    Else
                                    'Print " Disapproved: "+D.FileName+"  ("+Upper(ExtractExt(D.FileName))+" is not in the filterlist"+")"
                                    EndIf
                                End Rem
                                Ok = True                                                                                                '               '
                                If OnlyAllowExtList  Ok = ok And ListContains(OnlyAllowExtList ,Upper(ExtractExt(D.FileName))) 'DebugLog "    Ext Check: "+D.FileName+" >>> "+Ok
                                If OnlyAllowPathList Ok = Ok And ListContains(OnlyAllowPathList,Upper(ExtractDir(D.FileName))) 'DebugLog "    Dir Check: "+D.FileName+" >>> "+Ok
                                If OnlyAllowprefix   Ok = Ok And Prefixed(D.Filename,onlyallowprefix)
                                If OK Then 
                                    AddGadgetItem lastlist,D.FileName
                                    'DebugLog "Added to list: "+D.filename
                                    Else
                                    'DebugLog "     Rejected: "+D.filename
                                    EndIf
                                Next
                        Case "@i"
                            If Not lastlist error "No list to add this item to in line #"+linecount
                            AddGadgetItem lastlist,SL[1]
                        Default error "Unknown type '"+SL[0]+"' in line #"+linecount
                        End Select
                Case "Records"
                    TL = Trim(L)
                    If Upper(Left(TL,4))="REC:"
                        If MapContains(recs,Upper(Trim(Right(TL,Len(TL)-4))))
                            Select Proceed("Duplicate record definition:~n~n"+Upper(Trim(Right(TL,Len(TL)-4)))+"~n~nShall I merge the data with the existing record?")
                                Case -1 
                                    Print "Kill program by user's request"
                                    End
                                Case 0
                                    Print "Destroying the old"
                                    TRec = New StringMap
                                    MapInsert Recs,Upper(Trim(Right(TL,Len(TL)-4))),TRec
                                Case 1
                                    Print "Merging!"
                                    TRec = StringMap(MapValueForKey(Recs,   Upper(Trim(Right(TL,Len(TL)-4)))))
                                    For Local k$=EachIn MapKeys(TRec) Print K+" = "+TRec.Value(K) Next ' debug line
                            End Select
                        Else    
                            TRec = New StringMap
                            MapInsert Recs,Upper(Trim(Right(TL,Len(TL)-4))),TRec
                        EndIf
                    ElseIf TL.Find("=")<>"="
                        If Not TRec error "Definition without starting a record first in line #"+linecount+"~n~n"+L
                        SL = TL.split("=")
                        For Local slak=0 Until Len(SL) SL[slak]=Trim(SL[slak]) Next
                        If Not MapContains(fields,sl[0]) 
                            If RemoveNonExistent 
                                Select Proceed("Field does not exist ~q"+SL[0]+"~q in line "+linecount+"~n~nRemove this Field?")
                                    Case -1 End
                                    Case  0 MapInsert TRec,SL[0],SL[1]
                                    Case  1 Print "Field "+SL[0]+" has been removed from the database!"
                                    End Select
                                Else
                                error "Field does not exist ~q"+SL[0]+"~q in line "+linecount
                                EndIf
                            Else    
                            MapInsert TRec,SL[0],SL[1]
                            EndIf
                    Else
                        error "Syntax error in "+linecount+"~n~n"+L
                        EndIf
                Case "Allow"
                    TL = Trim(L)
                    ListAddLast allowlist,TL; SortList allowlist
                    Print "= Added: "+TL 
                Case "Default"
                    TL = Trim(L)
                    If TL.Find("=")<>"="
                        SL = TL.split("=")
                        For Local slak=0 Until Len(SL) SL[slak]=Trim(SL[slak]) Next
                        If Not MapContains(fields,sl[0]) error "Field does not exist ~q"+SL[0]+"~q in line "+linecount
                        MapInsert DefaultValues,SL[0],SL[1]
                    Else
                        error "Syntax error in "+linecount+"~n~n"+L
                        EndIf   
                Default
                    error "Internal error!~n~nUnknown Chunk!~n~n"+linecount+"/"+L   
                End Select      
            EndIf
        EndIf
    Next

 */
