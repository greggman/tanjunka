#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

#endregion

namespace Tanjunka
{
    // splash / about page
    partial class Splash : Form
    {
        static Splash ms_frmSplash = null;
        static Thread ms_oThread = null;

        // Fade in and out.
        private double m_dblOpacityIncrement = .34;
        private double m_dblOpacityDecrement = .34;
        private const int TIMER_INTERVAL = 50;
        private int m_iActualTicks = 0;

        public Splash()
        {
            InitializeComponent();
            this.Opacity = .00;
            timer1.Interval = TIMER_INTERVAL;
            timer1.Start();
        }

        // ************* Static Methods *************** //

        // A static method to create the thread and
        // launch the SplashScreen.
        static public void ShowSplashScreen()
        {
            // Make sure it is only launched once.
            if ( ms_frmSplash != null )
                return;
            ms_oThread = new Thread( new ThreadStart(Splash.ShowForm));
            ms_oThread.IsBackground = true;
            ms_oThread.SetApartmentState(ApartmentState.STA);
            ms_oThread.Start();
        }

        // A property returning the splash screen instance
        static public Splash SplashForm
        {
            get
            {
                return ms_frmSplash;
            }
        }

        // A private entry point for the thread.
        static private void ShowForm()
        {
            ms_frmSplash = new Splash();
            Application.Run(ms_frmSplash);
        }

        // A static method to close the SplashScreen
        static public void CloseForm()
        {
            if ( ms_frmSplash != null && ms_frmSplash.IsDisposed == false )
            {
                // Make it start going away.
                ms_frmSplash.m_dblOpacityIncrement = -ms_frmSplash.m_dblOpacityDecrement;
            }
            ms_oThread = null;  // we do not need these any more.
            ms_frmSplash = null;
        }

        //********* Event Handlers ************

        // Tick Event handler for the Timer control.
        // Handle fade in and fade out.  Also
        // handle the smoothed progress bar.
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if ( m_dblOpacityIncrement > 0 )
            {
                m_iActualTicks++;
                if ( this.Opacity < 1 )
                    this.Opacity += m_dblOpacityIncrement;
            } else
            {
                if ( this.Opacity > 0 )
                    this.Opacity += m_dblOpacityIncrement;
                else
                {
                    this.Close();
                }
            }
        }
    }
}