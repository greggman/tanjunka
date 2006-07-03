#region Using directives

using System;
using System.Windows.Forms;

#endregion

namespace Tanjunka
{
    public class TanTabPage : TabPage
    {
        private myEntryMethodDelegate UpdateEntryFunc = null;
        private myEntryMethodDelegate UpdatePageFunc = null;
        private myIntStrStrBoolMethodDelegate ExecCommandFunc = null;
        private myStr_StrMethodDelegate QueryCommandValueFunc = null;
        private myVoid_StrMethodDelegate InsertHTMLFunc = null;
        private myUICmdMethodDelegate UICommandFunc = null;

        public enum UICommandID
        {
            Unknown,
            Undo,
            Redo,
            SpellCheckStart,
            SpellCheckEnd,
        }

        public TanTabPage()
        {
            this.TabStop = false;
        }

        public void SetUpdateEntryFunc (myEntryMethodDelegate func)
        {
            UpdateEntryFunc = func;
        }
        public void SetUpdatePageFunc (myEntryMethodDelegate func)
        {
            UpdatePageFunc = func;
        }
        public void SetExecCommandFunc (myIntStrStrBoolMethodDelegate func)
        {
            ExecCommandFunc = func;
        }
        public void SetUICommandFunc (myUICmdMethodDelegate func)
        {
            UICommandFunc = func;
        }
        public void SetQueryCommandValueFunc (myStr_StrMethodDelegate func)
        {
            QueryCommandValueFunc = func;
        }
        public void SetInsertHTMLFunc (myVoid_StrMethodDelegate func)
        {
            InsertHTMLFunc = func;
        }

        public void UpdateEntry(EntryInfo entry)
        {
            if (UpdateEntryFunc != null)
            {
                UpdateEntryFunc(entry);
            }
        }
        public void UpdatePage(EntryInfo entry)
        {
            if (UpdatePageFunc != null)
            {
                UpdatePageFunc(entry);
            }
        }
        public void ExecCommand(int type, string cmd, string value)
        {
            if (ExecCommandFunc != null)
            {
                ExecCommandFunc(type, cmd, value, false);
            }
        }
        public void ExecCommand(int type, string cmd, string value, bool flag)
        {
            if (ExecCommandFunc != null)
            {
                ExecCommandFunc(type, cmd, value, flag);
            }
        }
        public void UICommand(UICommandID cmd)
        {
            if (UICommandFunc != null)
            {
                UICommandFunc(cmd);
            }
        }
        public string QueryCommandValue(string cmd)
        {
            if (QueryCommandValueFunc != null)
            {
                return QueryCommandValueFunc(cmd);
            }
            return "";
        }
        public void InsertHTML(string value)
        {
            if (InsertHTMLFunc != null)
            {
                InsertHTMLFunc(value);
            }
        }
    }

    public delegate void myEntryMethodDelegate(EntryInfo entry);
    public delegate void myIntStrStrBoolMethodDelegate(int i1, string str1, string str2, bool b);
    public delegate string myStr_StrMethodDelegate(string str1);
    public delegate void myVoid_StrMethodDelegate(string str1);
    public delegate void myUICmdMethodDelegate(TanTabPage.UICommandID cmd);
}

