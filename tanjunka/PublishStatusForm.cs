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
    // shows the status (progress bar) for posting
    partial class PublishStatusForm : Form  //, IProgressCallback
    {
        private String titleRoot = "Posting";
        private BackgroundWorker m_bgw;

        public PublishStatusForm(BackgroundWorker bgw)
        {
            m_bgw = bgw;
            InitializeComponent();
        }

        private void UpdateStatusText()
        {
            Text = titleRoot + String.Format( " - {0}% complete", (progressBar.Value * 100 ) / (progressBar.Maximum - progressBar.Minimum) );
        }

        public void SetMessage( String text )
        {
            if (cancelButton.Enabled)
            {
                label.Text = text;
            }
        }

        public void SetRange ( int max )
        {
            progressBar.Minimum = 0;
            progressBar.Maximum = max;
        }

        public void SetProgress( int val )
        {
            progressBar.Value = val;
//            progressBar.Invalidate();
//            UpdateStatusText();
//            this.Refresh();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            m_bgw.CancelAsync();
            SetMessage ("Canceling...");
            cancelButton.Enabled = false;
        }
    }
}