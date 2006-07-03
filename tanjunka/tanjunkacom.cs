#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

using CookComputing.XmlRpc;

#endregion

#if false
namespace Tanjunka
{

    [Guid("D3A7DA91-90EA-4b12-A251-E09CED7B51E4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _tanjunkaCom
    {
        [DispId(1)]
        void logFromHTML(string str);
        [DispId(2)]
        void genericCallbackII(int value1, int value2);
        [DispId(3)]
        void genericCallbackI(int value1);
        [DispId(4)]
        void genericCallbackIII(int value1, int value2, int value3);
        [DispId(5)]
        string genericCallbackISII_S(int value1, string value2, int value3, int value4);
        [DispId(6)]
        string genericCallbackISS_S(int value1, string value2, string value3);
    }

    [Guid("F41D446E-C77E-433f-A02A-23C5616A3A96")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Tanjunka.com")]
    public class tanjunkaCom : _tanjunkaCom
    {
        static private System.Windows.Forms.TextBox log;
        myVoidMethodDelegateIII genericCallbackIIIFunc;
        myVoidMethodDelegateII genericCallbackIIFunc;
        myVoidMethodDelegateI genericCallbackIFunc;
        myVoidMethodDelegateISII_S genericCallbackISII_SFunc;
        myVoidMethodDelegateISS_S genericCallbackISS_SFunc;

        public tanjunkaCom() { }

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

    }

// Declares a delegate for a method that takes in an int and returns a String.
   public delegate void myVoidMethodDelegate();
   public delegate void myVoidMethodDelegateIII(int value1, int value2, int value3);
   public delegate void myVoidMethodDelegateII(int value1, int value2);
   public delegate void myVoidMethodDelegateI(int value1);
   public delegate string myVoidMethodDelegateISII_S(int value1, string value2, int value3, int value4);
   public delegate string myVoidMethodDelegateISS_S(int value1, string value2, string value3);


}
#endif
