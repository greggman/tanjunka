#region Using directives

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;

#endregion

namespace Tanjunka
{
    public class Log
    {
        // ---------------------------- log ------------------------------------------
        static System.Windows.Forms.TextBox log;
        static TanBackgroundWorker m_bgw;
        static StringBuilder _preLog;
        static bool s_bInitDebug = false;

        static Log()
        {
            if (!s_bInitDebug)
            {
                s_bInitDebug = true;

                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
                Debug.AutoFlush = true;
                Debug.WriteLine("-------------start--------------------");
            }
            m_bgw = null;
            _preLog = new StringBuilder();
        }

        static public void Init(TextBox log_)
        {
            log = log_;
            log.Text = _preLog.ToString();
            m_bgw = null;
        }

        static public void SetBackgroundWorker(TanBackgroundWorker bgw)
        {
            m_bgw = bgw;
        }

        static public void Clear()
        {
            if (m_bgw == null)
            {
                RealClear();
            }
        }

        static public void Print(string str)
        {
            Debug.Write(str);
            if (m_bgw == null)
            {
                RealPrint(str);
            }
        }

        static public void Printn(string str)
        {
            Debug.WriteLine(str);
            if (m_bgw == null)
            {
                RealPrintn(str);
            }
            else
            {
                m_bgw.ReportProgress(0, new TanBackgroundWorker.ProgressInfo(TanBackgroundWorker.ProgressInfo.Cmd.Print, str));
            }
        }

        // -----------------------------------------------------

        static public void RealClear()
        {
            if (log != null)
            {
                log.Text = "";
            }
        }

        static public void RealPrint(string str)
        {
            if (log != null)
            {
                log.Text += str;
            }
            else
            {
                _preLog.Append(str);
            }
        }

        static public void RealPrintn(string str)
        {
            if (log != null)
            {
                log.Text += str + "\r\n";
            }
            else
            {
                _preLog.Append(str);
            }
        }
    }
}
