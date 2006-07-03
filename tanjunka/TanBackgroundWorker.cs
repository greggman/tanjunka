#region Using directives

using System;
using System.Threading;
using System.Collections;
using System.ComponentModel;

#endregion

namespace Tanjunka
{
    public class TanBackgroundWorker : BackgroundWorker
    {
        public class ProgressInfo
        {
            public enum Cmd
            {
                SetRange,
                UpdateProgress,
                UpdateMessage,
                Print,
            }

            public Cmd m_cmd;
            public string m_msg;
            public int m_value;

            public ProgressInfo(Cmd cmd, string msg)
            {
                m_cmd = cmd;
                m_msg = msg;
            }
            public ProgressInfo(Cmd cmd, int value)
            {
                m_cmd   = cmd;
                m_value = value;
            }
        };

        public DoWorkEventArgs e;

        public void UpdateMessage(string str)
        {
            ReportProgress(0, new ProgressInfo(ProgressInfo.Cmd.UpdateMessage, str));
        }
        public void UpdateProgress(int value)
        {
            ReportProgress(0, new ProgressInfo(ProgressInfo.Cmd.UpdateProgress, value));
        }
        public void Printn(string str)
        {
            ReportProgress(0, new ProgressInfo(ProgressInfo.Cmd.Print, str));
        }
        public void SetRange(int value)
        {
            ReportProgress(0, new ProgressInfo(ProgressInfo.Cmd.SetRange, value));
        }
    }
}
