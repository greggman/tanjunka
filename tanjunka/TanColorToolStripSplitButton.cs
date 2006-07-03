#region Using directives

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

#endregion
namespace Tanjunka
{
    public class TanColorToolStripSplitButton : ToolStripSplitButton
    {
        private Color m_color;
        private string m_colorID;

        public string TanColorID { get { return m_colorID; } set { m_colorID = value; } }
        public Color  TanColor   { get { return m_color;   } set { m_color   = value; } }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // Let base class draw its stuff first
            base.OnPaint(pe);

            // Draw code here...
            SolidBrush brush = new SolidBrush(m_color);

            Rectangle rect = new Rectangle(2, 15, 18, 4);

            pe.Graphics.FillRectangle(brush, rect);
        }
    }
}

