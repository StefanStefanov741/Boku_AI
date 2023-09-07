using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class HexagonalButton : Control
{
    public string tag;

    public HexagonalButton(string t) {
        this.tag = t;
        this.TabStop = false;
    }

    public void clicked() {
        MessageBox.Show("Clicked: "+tag);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        using (Graphics g = e.Graphics)
        {
            //Define the points for the hexagon
            Point[] hexagonPoints = new Point[6];
            float angle = 360.0f / 6;

            for (int i = 0; i < 6; i++)
            {
                float x = (float)(Width / 2 + Width / 2 * Math.Cos(Math.PI * angle * i / 180));
                float y = (float)(Height / 2 + Height / 2 * Math.Sin(Math.PI * angle * i / 180));
                hexagonPoints[i] = new Point((int)x, (int)y);
            }

            //Fill the hexagon
            using (Brush brush = new SolidBrush(Color.LightBlue))
            {
                g.FillPolygon(brush, hexagonPoints);
            }

            //Draw outline
            using (Pen pen = new Pen(Color.SlateGray, 3))
            {
                g.DrawPolygon(pen, hexagonPoints);
            }

            //Add Tag as text
            string buttonText = tag;
            using (Font font = new Font("Arial", 14, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.LimeGreen))
            {
                SizeF textSize = g.MeasureString(buttonText, font);
                PointF textLocation = new PointF((Width - textSize.Width) / 2, (Height - textSize.Height) / 2);
                g.DrawString(buttonText, font, textBrush, textLocation);
            }
        }
    }
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // Recreate the region to match the new size
        using (GraphicsPath path = new GraphicsPath())
        {
            Point[] hexagonPoints = new Point[6];
            float angle = 360.0f / 6;

            for (int i = 0; i < 6; i++)
            {
                float x = (float)(Width / 2 + Width / 2 * Math.Cos(Math.PI * angle * i / 180));
                float y = (float)(Height / 2 + Height / 2 * Math.Sin(Math.PI * angle * i / 180));
                hexagonPoints[i] = new Point((int)x, (int)y);
            }

            path.AddPolygon(hexagonPoints);

            // Set the region to the hexagon shape
            this.Region = new Region(path);
        }
    }
}
