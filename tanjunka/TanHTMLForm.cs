#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Tanjunka
{
    // form for editing moveable type service options
    partial class TanHTMLForm : Form
    {
        private string m_formFilename;
        private PersistWindowState m_windowState;

        public TanHTMLForm(string formFilename, string title)
        {
            m_formFilename = formFilename;
            InitializeComponent();
            m_windowState = new PersistWindowState(this);
            this.Text = title;
        }

        private void TanHTMLForm_Load(object sender, EventArgs e)
        {
            this.webBrowser1.setGenericCallbackI(new myVoidMethodDelegateI(ClickSomething));
            this.webBrowser1.setGenericCallbackII(new myVoidMethodDelegateII(EditSomething));

            Util.SetFormHTML(this.webBrowser1, m_formFilename);
        }

        protected void SetText(string name, string text)
        {
            this.webBrowser1.SetText(name, text);
        }

        protected string GetText(string name)
        {
            return this.webBrowser1.GetText(name);
        }

        protected void ClearSelect (string listname)
        {
            this.webBrowser1.ClearSelect(listname);
        }

        protected void AddOption (string listname, string name, int id, bool selected)
        {
            this.webBrowser1.AddOption(listname, name, id, selected);
        }

        protected string GetValue (string textid)
        {
            return this.webBrowser1.GetValue(textid);
        }

        protected void SetCheck(string name, string text)
        {
            this.webBrowser1.SetCheck(name, text);
        }

        protected string GetCheck(string name)
        {
            return this.webBrowser1.GetCheck(name);
        }

        protected void SetInnerText(string name, string text)
        {
            this.webBrowser1.SetInnerText(name, text);
        }

        protected void ClearItems (string name)
        {
            Object[] args = new Object[1] { name, };
            this.webBrowser1.Document.InvokeScript("clearlist", args);
        }

        protected void AddItemSSS(bool bChecked, string s1, string s2, string s3)
        {
            Object[] args = new Object[4] { bChecked, s1, s2, s3, };
            this.webBrowser1.Document.InvokeScript("additem", args);
        }


        protected virtual void GetSetAll (bool bGetFromForm)
        {
        }

        protected virtual void ClickSomething (int id)
        {
        }

        protected virtual void EditSomething (int id, int index)
        {
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            GetSetAll (false);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            GetSetAll (true);

            this.DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}

