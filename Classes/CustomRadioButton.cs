namespace RedisPlugin.Classes
{
    public class CustomRadioButton : RadioButton
    {
        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Clear background
            g.Clear(this.BackColor);

            // Define dimensions
            int circleDiameter = 16;
            int padding = 4;
            int textOffsetX = circleDiameter + padding;
            int centerY = this.Height / 2;

            // Define colors
            Color borderColor = this.Enabled ? Color.DodgerBlue : Color.LightGray;
            Color fillColor = this.Checked ? Color.DodgerBlue : Color.Transparent;
            Color innerCircleColor = this.Enabled ? Color.White : Color.Gainsboro;
            Color textColor = this.Enabled ? this.ForeColor : Color.Gray;

            // Optional: subtle hover or focus glow
            if (this.Focused)
            {
                using (Pen glowPen = new Pen(Color.CornflowerBlue, 3))
                {
                    glowPen.Color = Color.FromArgb(80, glowPen.Color);
                    Rectangle glowRect = new Rectangle(0, centerY - circleDiameter / 2, circleDiameter, circleDiameter);
                    g.DrawEllipse(glowPen, glowRect);
                }
            }

            // Draw outer circle
            Rectangle outerCircle = new Rectangle(0, centerY - circleDiameter / 2, circleDiameter, circleDiameter);
            using (Pen pen = new Pen(borderColor, 2))
            {
                g.DrawEllipse(pen, outerCircle);
            }

            // Draw inner filled circle if Checked
            if (this.Checked)
            {
                int innerDiameter = 8;
                Rectangle innerCircle = new Rectangle(
                    outerCircle.X + (circleDiameter - innerDiameter) / 2,
                    outerCircle.Y + (circleDiameter - innerDiameter) / 2,
                    innerDiameter,
                    innerDiameter);
                using (Brush brush = new SolidBrush(fillColor))
                {
                    g.FillEllipse(brush, innerCircle);
                }

                this.Text = "Connected" ; // Unicode filled circle
            }
            else
            {
                this.Text = "Disconnected" ; // Unicode empty circle
            }

                // Draw text with vertical alignment
                Size textSize = TextRenderer.MeasureText(this.Text, this.Font);
            Point textLocation = new Point(textOffsetX, centerY - textSize.Height / 2);
            TextRenderer.DrawText(g, this.Text, this.Font, textLocation, textColor);
        }

    }
}
