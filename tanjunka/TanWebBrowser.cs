#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

#endregion

namespace Tanjunka
{
    [ClassInterfaceAttribute(ClassInterfaceType.AutoDispatch)]
    [ComVisibleAttribute(true)]
//    [ComVisible(true)]
    public class TanWebBrowser : System.Windows.Forms.WebBrowser
    {
        static private System.Windows.Forms.TextBox log;
        myVoidMethodDelegateIII genericCallbackIIIFunc;
        myVoidMethodDelegateII genericCallbackIIFunc;
        myVoidMethodDelegateI genericCallbackIFunc;
        myVoidMethodDelegateISII_S genericCallbackISII_SFunc;
        myVoidMethodDelegateISS_S genericCallbackISS_SFunc;

        public TanWebBrowser()
        {
            this.ObjectForScripting = this;
        }

        public void SetText(string name, string text)
        {
            Object[] args = new Object[2] { name, text };

            this.Document.InvokeScript("settext", args);
//            Log.Printn ("Set(" + name + ")=(" + text + ")");
        }

        public string GetText(string name)
        {
            Object[] args = new Object[1] { name };

            Object obstr = this.Document.InvokeScript("gettext", args);
            string str = obstr.ToString();

//            Log.Printn ("Get(" + name + ")=(" + str + ")");

            return str;
        }

        public void ClearSelect (string listname)
        {
            Object[] args = new Object[1] { listname, };
            this.Document.InvokeScript("clearselect", args);
        }

        public void AddOption (string listname, string name, int id, bool selected)
        {
            Object[] args = new Object[4] { listname, name, id, selected};
            this.Document.InvokeScript("addoption", args);
        }

        public void ClearList (string listname)
        {
            Object[] args = new Object[1] { listname, };
            this.Document.InvokeScript("clearlist", args);
        }

        public string GetValue (string textid)
        {
            Object[] args = new Object[1] { textid};
            return this.Document.InvokeScript("getvalue", args).ToString();
        }

        public string GetSelect (string textid)
        {
            Object[] args = new Object[1] { textid};
            return this.Document.InvokeScript("getvalue", args).ToString();
        }

        public void SetSelect (string textid, string value)
        {
            Object[] args = new Object[2] { textid, value};
            this.Document.InvokeScript("setselect", args);
        }

        public void SetCheck(string name, string text)
        {
            Object[] args = new Object[2] { name, text };

            this.Document.InvokeScript("setcheck", args);
        }

        public string GetCheck(string name)
        {
            Object[] args = new Object[1] { name };

            return this.Document.InvokeScript("getcheck", args).ToString();
        }

        public void SetCheckBool(string name, bool bChecked)
        {
            Object[] args = new Object[2] { name, bChecked ? "1" : "0" };

            this.Document.InvokeScript("setcheck", args);
        }

        public bool GetCheckBool(string name)
        {
            Object[] args = new Object[1] { name };

            return (this.Document.InvokeScript("getcheck", args).ToString().CompareTo("0") != 0);
        }

        public string GetInnerHTML(string id)
        {
            string bod;
            Object[] args = new Object[1] { id };
            Object bodOb = this.Document.InvokeScript("getInnerHTML", args);
            bod = bodOb.ToString();

            return bod;
        }

        public void SetInnerHTML(string id, string value)
        {
            Object[] args = new Object[2] { id, value };
            this.Document.InvokeScript("setInnerHTML", args);
        }

        public string GetInnerText(string id)
        {
            string bod;
            Object[] args = new Object[1] { id };
            Object bodOb = this.Document.InvokeScript("getInnerText", args);
            bod = bodOb.ToString();

            return bod;
        }

        public void SetInnerText(string id, string value)
        {
            Object[] args = new Object[2] { id, value };
            this.Document.InvokeScript("setInnerText", args);
        }

        public void DisableElement(string id, bool bDisable)
        {
            Object[] args = new Object[2] { id, bDisable ? "1" : "0"};
            this.Document.InvokeScript("disableElement", args);
        }

        public void SetCSSForm(string cssURL)
        {
            Object[] args = new Object[1] { cssURL, };
            this.Document.InvokeScript("changeCSS", args);
        }

        //-------------------------------------------------------

        public void setBox(System.Windows.Forms.TextBox tb)
        {
            log = tb;
        }

        public void logFromHTML(string str)
        {
            log.Text += str + "\r\n";
        }

        public void setGenericCallbackIII(myVoidMethodDelegateIII func)
        {
            genericCallbackIIIFunc = func;
        }

        public void genericCallbackIII(int value1, int value2, int value3)
        {
            genericCallbackIIIFunc(value1, value2, value3);
        }

        public void setGenericCallbackII(myVoidMethodDelegateII func)
        {
            genericCallbackIIFunc = func;
        }

        public void genericCallbackII(int value1, int value2)
        {
            genericCallbackIIFunc(value1, value2);
        }

        public void setGenericCallbackI(myVoidMethodDelegateI func)
        {
            genericCallbackIFunc = func;
        }

        public void genericCallbackI(int value1)
        {
            genericCallbackIFunc(value1);
        }

        public void setGenericCallbackISII_S(myVoidMethodDelegateISII_S func)
        {
            genericCallbackISII_SFunc = func;
        }

        public string genericCallbackISII_S(int value1, string value2, int value3, int value4)
        {
            return genericCallbackISII_SFunc(value1, value2, value3, value4);
        }

        public void setGenericCallbackISS_S(myVoidMethodDelegateISS_S func)
        {
            genericCallbackISS_SFunc = func;
        }

        public string genericCallbackISS_S(int value1, string value2, string value3)
        {
            return genericCallbackISS_SFunc(value1, value2, value3);
        }

        public void ClickSomething(int ndx)
        {
            Log.Printn("here-debug");
        }

        public void CheckSomething(int id, int index, int onoff)
        {
            Log.Printn("checked " + id + " : " + index + " : " + onoff);

        }

        public void EditSomething(int id, int index)
        {
            Log.Printn("clicked " + id + " : " + index);

        }

        #if false
        protected override void OnDragEnter(DragEventArgs e)
        {
            // make sure they're actually dropping files (not text or anything else)
            {
                string[] formats = e.Data.GetFormats();

                Log.Printn ("---formats---");
                foreach (string format in formats)
                {
                    Log.Printn (format);
                }
            }

            if (
                (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Bitmap, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Dib, false) == true) ||
                (e.Data.GetDataPresent("UniformResourceLocator", false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Html, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Rtf, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.UnicodeText, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.SymbolicLink, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Text, true) == true) ||
                false
                )
            {
                // allow them to continue
                // (without this, the cursor stays a "NO" symbol
                e.Effect = DragDropEffects.All;
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Dib, false) == true)
            {
                Log.Printn ("--dropped DIB--");

                if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    // loop through the string array, adding each filename to the ListBox

                    foreach (string file in files)
                    {
                        Log.Printn (file);
                    }
                }
                else
                {
                    Log.Printn ("*no filedrop info for dib");
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                // get the filenames
                // (yes, everything to the left of the "=" can be put in the
                // foreach loop in place of "files", but this is easier to understand.)

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped file--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
            else if (e.Data.GetDataPresent("UniformResourceLocator", false) == true)
            {
                Log.Printn ("--dropped URL--");
                {
                    string link = (string)e.Data.GetData(DataFormats.UnicodeText);

                    Log.Printn ("part1:" + link);
                }
                {
                    System.IO.Stream ioStream =
                    (System.IO.Stream)e.Data.GetData("FileGroupDescriptor");
                    byte[] contents = new Byte[2048];
                    ioStream.Read(contents, 0, 2048);
                    ioStream.Close();
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    //The magic number 76 is the size of that part of the
                    //FILEGROUPDESCRIPTOR structure before
                    // the filename starts - cribbed
                    //from another usenet post.
                    for (int i = 76; contents[i] != 0; i++)
                    {
                        sb.Append((char)contents[i]);
                    }
                    if (!sb.ToString(sb.Length - 4, 4).ToLower().Equals(".url"))
                    {
                        throw new Exception("filename does not end in '.url'");
                    }
                    string link = sb.ToString(0, sb.Length - 4);

                    Log.Printn ("part2:" + link);
                }

            }
            else if (e.Data.GetDataPresent(DataFormats.SymbolicLink, false) == true)
            {
                string link = (string)e.Data.GetData(DataFormats.SymbolicLink);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped link--");
                Log.Printn (link);
            }
            else if (e.Data.GetDataPresent(DataFormats.Html, false) == true)
            {
                string link = (string)e.Data.GetData(DataFormats.Html);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped html--");
                Log.Printn (link);
            }
            else if (e.Data.GetDataPresent(DataFormats.Rtf, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.Rtf);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped rtf--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.UnicodeText, false) == true)
            {
                string unitext = (string)e.Data.GetData(DataFormats.UnicodeText);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped unicode--");
                Log.Printn (unitext);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text, true) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.Text);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped text--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
        }
        #endif
    }

    public delegate void myVoidMethodDelegate();
    public delegate void myVoidMethodDelegateIII(int value1, int value2, int value3);
    public delegate void myVoidMethodDelegateII(int value1, int value2);
    public delegate void myVoidMethodDelegateI(int value1);
    public delegate string myVoidMethodDelegateISII_S(int value1, string value2, int value3, int value4);
    public delegate string myVoidMethodDelegateISS_S(int value1, string value2, string value3);
}
