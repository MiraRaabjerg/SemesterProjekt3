using System;
using System.Linq;
using Microsoft.Maui.Graphics;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.PresentationMaui
{
    /// <summary>
    /// Enkel graf:
    ///  - Sort kurve af Samples (resp. frekvens)
    ///  - Orange linje for tærskel (hvis sat)
    ///  - Egen akse + grid, ingen scroll
    /// </summary>
    public sealed class SimpleChartDrawable : IDrawable
    {
        private readonly NightViewModel _vm;

        public SimpleChartDrawable(NightViewModel vm)
        {
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        }

        public void Draw(ICanvas canvas, RectF dirty)
        {
            try
            {
                canvas.SaveState();
                canvas.Antialias = true;

                // -------- Margener, så labels ikke klippes --------
                float ml = 70, mr = 28, mt = 16, mb = 56;
                float left = dirty.Left + ml;
                float right = dirty.Right - mr;
                float top = dirty.Top + mt;
                float bottom = dirty.Bottom - mb;

                // Baggrund
                canvas.FillColor = Colors.White;
                canvas.FillRectangle(dirty);

                var samples = _vm.Samples?.ToList() ?? new();
                if (samples.Count < 2 || left >= right || top >= bottom)
                {
                    // Ingen data -> vis tekst
                    canvas.FontColor = Colors.Gray;
                    canvas.DrawString(
                        "Ingen data",
                        left + 8,
                        (top + bottom) / 2,
                        HorizontalAlignment.Left);
                    canvas.RestoreState();
                    return;
                }

                // -------- Y-akse: min/max + lidt padding --------
                double rawMin = samples.Min(s => (double)s.Frequency);
                double rawMax = samples.Max(s => (double)s.Frequency);

                if (Math.Abs(rawMax - rawMin) < 0.001)
                {
                    // næsten flad linje -> giv lidt spænd
                    rawMin -= 1;
                    rawMax += 1;
                }

                double padY = Math.Max(0.5, (rawMax - rawMin) * 0.2);
                double yMin = rawMin - padY;
                double yMax = rawMax + padY;
                if (yMax <= yMin) yMax = yMin + 1;   // sikkerhed

                // -------- X/Y mapping-funktioner --------
                float X(int idx)
                {
                    if (samples.Count == 1) return left;
                    float t = (float)idx / (samples.Count - 1);
                    return left + t * (right - left);
                }

                float Y(float v)
                {
                    float t = (float)((yMax - v) / (yMax - yMin));
                    return top + t * (bottom - top);
                }

                // -------- Vandrette grid-linjer --------
                canvas.StrokeColor = Color.FromArgb("#E5E5E5");
                canvas.StrokeSize = 1;
                int yGrid = 4;
                for (int i = 0; i <= yGrid; i++)
                {
                    float yy = top + (bottom - top) * i / yGrid;
                    canvas.DrawLine(left, yy, right, yy);
                }

                // -------- Tærskel-linje (orange) --------
                if (_vm.Threshold.HasValue)
                {
                    float thrY = Y((float)_vm.Threshold.Value);
                    canvas.StrokeColor = Colors.Orange;
                    canvas.StrokeSize = 2;
                    canvas.DrawLine(left, thrY, right, thrY);

                    canvas.FontColor = Colors.Orange;
                    canvas.FontSize = 11;
                    canvas.DrawString(
                        $"Tærskel {_vm.Threshold.Value:0.0}",
                        right - 110,
                        thrY - 14,
                        HorizontalAlignment.Left);
                }

                // -------- Sort polyline for samples --------
                canvas.StrokeColor = Colors.Black;
                canvas.StrokeSize = 2.5f;

                for (int i = 1; i < samples.Count; i++)
                {
                    var a = samples[i - 1];
                    var b = samples[i];

                    canvas.DrawLine(
                        X(i - 1), Y((float)a.Frequency),
                        X(i), Y((float)b.Frequency));
                }

                // -------- Y-labels --------
                canvas.FontColor = Colors.Gray;
                canvas.FontSize = 11;
                for (int i = 0; i <= yGrid; i++)
                {
                    double val = yMin + (yMax - yMin) * i / yGrid;
                    float yy = top + (bottom - top) * i / yGrid;
                    canvas.DrawString(
                        val.ToString("0.0"),
                        left - 56,
                        yy - 8,
                        HorizontalAlignment.Left);
                }

                // -------- X-labels (sample-indeks) --------
                int xTicks = 6;
                for (int i = 0; i <= xTicks; i++)
                {
                    int idx =
                        samples.Count == 1
                            ? 0
                            : (int)Math.Round((samples.Count - 1) * (double)i / xTicks);

                    float xx = X(idx);
                    canvas.DrawLine(xx, bottom, xx, bottom + 4);
                    canvas.DrawString(
                        idx.ToString(),
                        xx - 10,
                        bottom + 8,
                        HorizontalAlignment.Left);
                }

                // -------- Aksentitler --------
                canvas.FontColor = Colors.Black;
                canvas.FontSize = 12;
                canvas.DrawString(
                    "Tid (sample indeks)",
                    (left + right) / 2,
                    bottom + 26,
                    HorizontalAlignment.Center);

                canvas.SaveState();
                canvas.Translate(left - 58, (top + bottom) / 2);
                canvas.Rotate(-90);
                canvas.DrawString(
                    "Frekvens",
                    -40,
                    -8,
                    HorizontalAlignment.Left);
                canvas.RestoreState();

                canvas.RestoreState();
            }
            catch (Exception ex)
            {
                // Sikkerhed: hellere undlade graf end at crashe appen
                canvas.RestoreState();
                System.Diagnostics.Debug.WriteLine("[Chart] " + ex);
            }
        }
    }
}


