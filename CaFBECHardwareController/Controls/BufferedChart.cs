using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

public class BufferedChart : Chart
{
    public BufferedChart()
    {
        // Enable true double buffering
        this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer, true);

        this.UpdateStyles();
    }
}