#region Using directives

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

#endregion
namespace Tanjunka
{
    public class TanColorMenuItem : ToolStripMenuItem
    {
        private Color m_color;
        private string m_colorID;

        public TanColorMenuItem(Color color, string colorID)
        {
            m_color = color;
            m_colorID = colorID;
        }
        public string TanColorID { get { return m_colorID; } set { m_colorID = value; } }
        public Color TanColor    { get { return m_color; } set { m_color = value; } }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // Let base class draw its stuff first
            base.OnPaint(pe);

            // Draw code here...
            SolidBrush brush = new SolidBrush(m_color);
            Pen black = new Pen(Color.Black);

            Rectangle rect = this.ContentRectangle;
            rect.Inflate(-1,-1);

            pe.Graphics.FillRectangle(brush, rect);
            pe.Graphics.DrawRectangle(black, rect);
        }
    }

    public struct ColorInit
    {
        public string tanColorID;
        public Color  tanColor;

        public ColorInit(string cid, Color clr)
        {
            tanColorID = cid;
            tanColor   = clr;
        }
    }

    public class StyleInfo
    {
        public string label;
        public string tag;

        public StyleInfo (string l, string t)
        {
            label = l;
            tag   = t;
        }
        public override string ToString()
        {
            return label;
        }
    }
}

