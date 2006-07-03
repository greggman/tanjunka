#region Using directives

using System;
using System.Collections;
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
    // let's you choose a supported service type
    // example:
    //
    //   Movabletype
    //   Flickr
    //   Blogger
    //   WordPress
    //
    [ComVisible(true)]
    public partial class ServiceTypeForm : Form
    {
        private string selectedService;
        private Dictionary<string, BlogPhotoServiceCreator> m_services;
        private PersistWindowState m_windowState;

        public ServiceTypeForm(Dictionary<string, BlogPhotoServiceCreator> services)
        {
            m_services = services;
            InitializeComponent();
            m_windowState = new PersistWindowState(this);
        }

        public string GetSelectedService ()
        {
            return selectedService;
        }

        private void ServiceTypeForm_Load(object sender, EventArgs e)
        {
            this.webBrowser1.ObjectForScripting = this;
            Util.SetFormHTML(this.webBrowser1, "tan-servicetypeform.html");
        }

        private void ClearSelect ()
        {
            Object[] args = new Object[1] { "selectlist", };
            this.webBrowser1.Document.InvokeScript("clearselect", args);
        }

        private void AddOption (string name, string id)
        {
            Object[] args = new Object[3] { "selectlist", name, id, };
            this.webBrowser1.Document.InvokeScript("addoption", args);
        }

        private string GetValue(string name)
        {
            Object[] args = new Object[1] { name };

            Object obstr = this.webBrowser1.Document.InvokeScript("gettext", args);
            string str = obstr.ToString();

            Log.Printn("Get(" + name + ")=(" + str + ")");

            return str;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ClearSelect();

            // TODO: change this to whatever is CLS complient
            Dictionary<string, BlogPhotoServiceCreator>.Enumerator de = m_services.GetEnumerator();
            while (de.MoveNext())
            {
                AddOption (de.Current.Key, de.Current.Key);
            }

            #if false
            foreach (Diction4aryEntry<string,BlogPhotoServiceCreator> de in m_services)
            {
                string name = (string)de.Key;

                AddOption (name, name);
            }
            #endif

            #if false
            // add post sets
            AddOption ("movabletype", "movabletype");
            AddOption ("flickr",      "f--lickr");
            AddOption ("Blogger",     "Blogger");
            #endif
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            selectedService = GetValue("selectlist");

            this.DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
        }
    }
}