#region Using directives

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;

#endregion

namespace Tanjunka
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                foreach (string arg in args)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(arg);
                }

                if (sb.Length > 0)
                {
                    MessageBox.Show(sb.ToString());
                }
                Splash.ShowSplashScreen();
                System.Diagnostics.Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
                Application.EnableVisualStyles();
                // Application.EnableRTLMirroring(); // 2.0 beta 1
                Application.Run(new BodyForm(sb.ToString()));
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("done");
        }
    }
}