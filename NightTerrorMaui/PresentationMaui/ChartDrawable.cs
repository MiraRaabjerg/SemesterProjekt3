using System;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace NightTerrorMaui.PresentationMaui
{
    /// Enkel og robust graf:
    /// - Sort polyline af Samples (Frequency over tid)
    /// - Orange tærskellinje (median eller fallback)
    /// - Tilpasser hele tidsrummet til den tilgængelige bredde (ingen horisontal scroll)
    public sealed class SimpleChartDrawable : IDrawable
    {
        private readonly NightViewModel _vm;
        public SimpleChartDrawable(NightViewModel vm) => _vm = vm;

        public void Draw(ICanvas canvas, RectF dirty)
        {
            try
            {
                canvas.SaveState();
                canvas.Antialias = true;

                // Lidt store marginer så labels ikke klippes
                float ml = 70, mr = 28, mt = 16, mb = 56;
                float left = dirty.Left + ml, right = dirty.Right - mr;
                float top = dirty.Top + mt, bottom = dirty.Bottom - mb;

                // Baggrund
                canvas.FillColor = Colors.White;
                canvas.FillRectangle(dirty);

                var samples = _vm?.Samples?.ToList() ?? [];
                if (samples.Count < 2 || left >= right || top >= bottom)
                {
                    canvas.FontColor = Colors.Gray;
                    canvas.DrawString("Ingen data", left + 8, (top + bottom) / 2, HorizontalAlignment.Left);
                    canvas.RestoreState();
                    return;
                }

                // ----- Tidsakse (fit hele tidsrummet) -----
                var tMin = samples.Min(s => s.Time);
                var tMax = samples.Max(s => s.Time);
                if (tMax <= tMin) tMax = tMin.AddSeconds(1); // guard

                // Lidt padding i enderne
                var padT = TimeSpan.FromSeconds(Math.Max(5, (tMax - tMin).TotalSeconds * 0.05));
                tMin -= padT; tMax += padT;

                double totalSec = Math.Max(1.0, (tMax - tMin).TotalSeconds);
                float X(DateTime t) => left + (float)((t - tMin).TotalSeconds / totalSec) * (right - left);

                // ----- Frekvensakse (fit min/max + padding) -----
                double rawMin = samples.Min(s => s.Frequency);
                double rawMax = samples.Max(s => s.Frequency);
                if (Math.Abs(rawMax - rawMin) < 0.001) { rawMin -= 1; rawMax += 1; } // guard hvis flad linje

                double padY = Math.Max(0.5, (rawMax - rawMin) * 0.2);
                double yMin = rawMin - padY;
                double yMax = rawMax + padY;
                if (yMax <= yMin) yMax = yMin + 1; // guard

                float Y(double f) => top + (float)((yMax - f) / (yMax - yMin)) * (bottom - top);

                // ----- Grid (vandrette linjer) -----
                canvas.StrokeColor = Color.FromArgb("#E5E5E5");
                canvas.StrokeSize = 1;
                int yGrid = 4;
                for (int i = 0; i <= yGrid; i++)
                {
                    float yy = top + (bottom - top) * i / yGrid;
                    canvas.DrawLine(left, yy, right, yy);
                }

                // ----- Tærskel (orange) -----
                double threshold;
                var orderedVals = samples.Select(s => s.Frequency).OrderBy(v => v).ToList();
                threshold = orderedVals[orderedVals.Count / 2];
                threshold = Math.Round(threshold, 1);

                float yThr = Y(threshold);
                canvas.StrokeColor = Colors.Orange;
                canvas.StrokeSize = 2;
                canvas.DrawLine(left, yThr, right, yThr);
                canvas.FontColor = Colors.Orange;
                canvas.FontSize = 11;
                canvas.DrawString($"Tærskel {threshold:0.0}", right - 110, yThr - 14, HorizontalAlignment.Left);

                // ----- Sort polyline (samples) -----
                var ordered = samples.OrderBy(s => s.Time).ToList();
                canvas.StrokeColor = Colors.Black;
                canvas.StrokeSize = 2.5f;

                for (int i = 1; i < ordered.Count; i++)
                {
                    var a = ordered[i - 1];
                    var b = ordered[i];
                    canvas.DrawLine(X(a.Time), Y(a.Frequency), X(b.Time), Y(b.Frequency));
                }

                // ----- Y-etiketter -----
                canvas.FontColor = Colors.Gray;
                canvas.FontSize = 11;
                for (int i = 0; i <= yGrid; i++)
                {
                    double val = yMin + (yMax - yMin) * i / yGrid;
                    float yy = top + (bottom - top) * i / yGrid;
                    canvas.DrawString(val.ToString("0.0"), left - 56, yy - 8, HorizontalAlignment.Left);
                }

                // ----- X-etiketter (maks 6 ticks) -----
                int xTicks = 6;
                for (int i = 0; i <= xTicks; i++)
                {
                    double f = (double)i / xTicks;
                    var t = tMin.AddSeconds(totalSec * f);
                    float x = X(t);
                    canvas.DrawLine(x, bottom, x, bottom + 4);
                    canvas.DrawString(t.ToString("HH:mm"), x - 16, bottom + 8, HorizontalAlignment.Left);
                }

                // Aksetitler
                canvas.FontColor = Colors.Black;
                canvas.FontSize = 12;
                canvas.DrawString("Tid", (left + right) / 2 - 10, bottom + 26, HorizontalAlignment.Left);

                canvas.SaveState();
                canvas.Translate(left - 58, (top + bottom) / 2);
                canvas.Rotate(-90);
                canvas.DrawString("Frekvens (bpm)", -40, -8, HorizontalAlignment.Left);
                canvas.RestoreState();

                canvas.RestoreState();
            }
            catch (Exception ex)
            {
                // Som sikkerhed: vis fejl i grafen i stedet for at crashe app’en
                canvas.RestoreState();
#if DEBUG
                System.Diagnostics.Debug.WriteLine("[Chart] " + ex);
#endif
            }
        }
    }
}
