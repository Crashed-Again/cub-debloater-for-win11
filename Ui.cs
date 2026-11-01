using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Cub
{
    // ------------------------------------------------------------------ theme
    internal static class Theme
    {
        public static float Scale = 1f;
        public static int S(int v) { return (int)Math.Round(v * Scale); }

        // the window background (also used for the title bar, so the two blend)
        public static readonly Color Glass = Color.FromArgb(13, 14, 13);

        public static readonly Color Panel2     = Color.FromArgb(25, 27, 25);   // sidebar, top bar, bottom bar
        public static readonly Color Panel      = Color.FromArgb(20, 21, 20);   // big content container
        public static readonly Color Card       = Color.FromArgb(31, 33, 30);   // one row
        public static readonly Color Hover      = Color.FromArgb(34, 36, 33);
        public static readonly Color Btn        = Color.FromArgb(40, 43, 39);
        public static readonly Color BtnHover   = Color.FromArgb(54, 58, 52);
        public static readonly Color Input      = Color.FromArgb(36, 38, 35);
        public static readonly Color Border     = Color.FromArgb(48, 52, 46);
        public static readonly Color Text       = Color.FromArgb(236, 238, 233);
        public static readonly Color Sub        = Color.FromArgb(146, 152, 142);
        public static readonly Color Accent     = Color.FromArgb(190, 214, 122);   // soft lime, like the tPay bars
        public static readonly Color AccentDim  = Color.FromArgb(104, 122, 62);
        public static readonly Color AccentText = Color.FromArgb(24, 30, 12);
        public static readonly Color Warn       = Color.FromArgb(255, 181, 71);
        public static readonly Color Good       = Color.FromArgb(124, 214, 144);
        public static readonly Color Off        = Color.FromArgb(74, 78, 71);

        // Poppins if installed, otherwise Segoe UI
        public static readonly string Family = PickFamily();
        private static string PickFamily()
        {
            try
            {
                using (var fonts = new InstalledFontCollection())
                    foreach (FontFamily f in fonts.Families)
                        if (string.Equals(f.Name, "Poppins", StringComparison.OrdinalIgnoreCase)) return "Poppins";
            }
            catch { }
            return "Segoe UI";
        }

        public static Font Body  = new Font(Family, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        public static Font Small = new Font(Family, 8.75f, FontStyle.Regular, GraphicsUnit.Point);
        public static Font Bold  = new Font(Family, 10f, FontStyle.Bold, GraphicsUnit.Point);
        public static Font Title = new Font(Family, 20f, FontStyle.Bold, GraphicsUnit.Point);
        public static Font Mono  = new Font("Consolas", 9.5f, FontStyle.Regular, GraphicsUnit.Point);

        public static string VersionText()
        {
            Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return v.Build > 0 ? v.ToString(3) : v.Major + "." + v.Minor;   // 2.0.0 shows as "2.0"
        }

        public static GraphicsPath Round(Rectangle r, int radius)
        {
            int d = Math.Max(2, radius * 2);
            d = Math.Min(d, Math.Min(r.Width, r.Height));
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // 5x7 dot-matrix letters for the NEONBEAR wordmark
        private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
        {
            { 'N', new[] { "10001", "11001", "10101", "10101", "10011", "10001", "10001" } },
            { 'E', new[] { "11111", "10000", "10000", "11110", "10000", "10000", "11111" } },
            { 'O', new[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" } },
            { 'B', new[] { "11110", "10001", "10001", "11110", "10001", "10001", "11110" } },
            { 'A', new[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" } },
            { 'R', new[] { "11110", "10001", "10001", "11110", "10100", "10010", "10001" } }
        };

        public static void DrawDots(Graphics g, string text, int x, int y, int step, int dia, Color c)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(c))
            {
                int cx = x;
                foreach (char ch in text)
                {
                    string[] rows;
                    if (Glyphs.TryGetValue(ch, out rows))
                    {
                        for (int r = 0; r < rows.Length; r++)
                            for (int col = 0; col < rows[r].Length; col++)
                                if (rows[r][col] == '1')
                                    g.FillEllipse(b, cx + col * step, y + r * step, dia, dia);
                    }
                    cx += 6 * step;
                }
            }
        }
    }

    // ------------------------------------------------------------------ dark title bar / scrollbars
    internal static class DarkMode
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string subApp, string subIdList);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int size);

        private static int Rgb(Color c) { return c.R | (c.G << 8) | (c.B << 16); }

        /// <summary>Dark title bar painted in the app's own colour, with a matching window border (Windows 11).</summary>
        public static void TitleBar(IntPtr handle, Color caption, Color text)
        {
            int v = 1;
            DwmSetWindowAttribute(handle, 20, ref v, sizeof(int));   // dark mode title bar
            DwmSetWindowAttribute(handle, 19, ref v, sizeof(int));
            int c = Rgb(caption);
            DwmSetWindowAttribute(handle, 35, ref c, sizeof(int));   // caption colour
            DwmSetWindowAttribute(handle, 34, ref c, sizeof(int));   // border colour
            int t = Rgb(text);
            DwmSetWindowAttribute(handle, 36, ref t, sizeof(int));   // caption text colour
        }

        public static void Scrollbars(Control c)
        {
            if (c.IsHandleCreated) SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
            else c.HandleCreated += (s, e) => SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
        }
    }

    // ------------------------------------------------------------------ small vector icons
    internal static class Icons
    {
        public static void Draw(Graphics g, string key, Rectangle r, Color c)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float w = r.Width, h = r.Height;
            Func<float, float, PointF> P = (x, y) => new PointF(r.X + x * w, r.Y + y * h);
            float pw = Math.Max(1.5f, w / 11f);
            using (var pen = new Pen(c, pw) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var br = new SolidBrush(c))
            {
                switch (key)
                {
                    case "Appearance":
                        g.DrawEllipse(pen, r.X + pw / 2, r.Y + pw / 2, w - pw, h - pw);
                        g.FillPie(br, r.X + pw, r.Y + pw, w - pw * 2, h - pw * 2, -90, 180);
                        break;
                    case "AI":
                        {
                            var pts = new List<PointF>();
                            for (int i = 0; i < 8; i++)
                            {
                                double a = Math.PI / 4 * i - Math.PI / 2;
                                float rad = (i % 2 == 0) ? 0.5f : 0.16f;
                                pts.Add(P(0.5f + (float)Math.Cos(a) * rad, 0.5f + (float)Math.Sin(a) * rad));
                            }
                            g.FillPolygon(br, pts.ToArray());
                        }
                        break;
                    case "Apps":
                        {
                            float s = 0.36f;
                            foreach (float fx in new[] { 0.08f, 0.56f })
                                foreach (float fy in new[] { 0.08f, 0.56f })
                                    using (var p = Theme.Round(new Rectangle((int)(r.X + fx * w), (int)(r.Y + fy * h), (int)(s * w), (int)(s * h)), 2))
                                        g.DrawPath(pen, p);
                        }
                        break;
                    case "Start menu":
                        g.DrawLine(pen, P(0.12f, 0.22f), P(0.88f, 0.22f));
                        g.DrawLine(pen, P(0.12f, 0.50f), P(0.70f, 0.50f));
                        g.DrawLine(pen, P(0.12f, 0.78f), P(0.52f, 0.78f));
                        break;
                    case "Privacy and security":
                        g.DrawPolygon(pen, new[] { P(0.5f, 0.04f), P(0.9f, 0.2f), P(0.9f, 0.55f), P(0.5f, 0.96f), P(0.1f, 0.55f), P(0.1f, 0.2f) });
                        break;
                    case "Performance":
                        g.DrawPolygon(pen, new[] { P(0.6f, 0.02f), P(0.14f, 0.58f), P(0.46f, 0.58f), P(0.38f, 0.98f), P(0.86f, 0.4f), P(0.54f, 0.4f) });
                        break;
                    case "Software":
                        g.DrawLine(pen, P(0.5f, 0.06f), P(0.5f, 0.66f));
                        g.DrawLines(pen, new[] { P(0.26f, 0.44f), P(0.5f, 0.68f), P(0.74f, 0.44f) });
                        g.DrawLine(pen, P(0.14f, 0.9f), P(0.86f, 0.9f));
                        break;
                    case "Drivers and runtimes":
                        g.DrawRectangle(pen, r.X + w * 0.26f, r.Y + h * 0.26f, w * 0.48f, h * 0.48f);
                        foreach (float t in new[] { 0.38f, 0.62f })
                        {
                            g.DrawLine(pen, P(t, 0.06f), P(t, 0.26f));
                            g.DrawLine(pen, P(t, 0.74f), P(t, 0.94f));
                            g.DrawLine(pen, P(0.06f, t), P(0.26f, t));
                            g.DrawLine(pen, P(0.74f, t), P(0.94f, t));
                        }
                        break;
                    case "Apply":
                        g.FillPolygon(br, new[] { P(0.22f, 0.1f), P(0.88f, 0.5f), P(0.22f, 0.9f) });
                        break;
                    case "Search":
                        g.DrawEllipse(pen, r.X + w * 0.08f, r.Y + h * 0.08f, w * 0.6f, h * 0.6f);
                        g.DrawLine(pen, P(0.6f, 0.6f), P(0.92f, 0.92f));
                        break;
                }
            }
        }
    }

    // ------------------------------------------------------------------ rounded card
    internal class Card : Panel
    {
        public Color Fill = Theme.Card;
        public int Radius = 12;
        public bool Outline = true;

        public Card()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Theme.Panel;   // shows in the corners, so it must match whatever the card sits on
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var p = Theme.Round(r, Theme.S(Radius)))
            {
                using (var b = new SolidBrush(Fill)) e.Graphics.FillPath(b, p);
                if (Outline) using (var pen = new Pen(Theme.Border)) e.Graphics.DrawPath(pen, p);
            }
            base.OnPaint(e);   // raises the Paint event: handlers draw ON TOP of the card
        }
    }

    // ------------------------------------------------------------------ toggle switch
    internal sealed class ToggleSwitch : Control
    {
        private bool _on;
        public event EventHandler CheckedChanged;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(Theme.S(46), Theme.S(24));
            Cursor = Cursors.Hand;
            BackColor = Theme.Card;
        }

        public bool Checked
        {
            get { return _on; }
            set
            {
                if (_on == value) return;
                _on = value;
                Invalidate();
                EventHandler h = CheckedChanged;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var p = Theme.Round(r, Height / 2))
            using (var b = new SolidBrush(_on ? Theme.Accent : Theme.Off))
                g.FillPath(b, p);
            int pad = Theme.S(3);
            int d = Height - pad * 2 - 1;
            int x = _on ? Width - d - pad - 1 : pad;
            using (var b = new SolidBrush(_on ? Theme.AccentText : Color.White))
                g.FillEllipse(b, x, pad, d, d);
        }
    }

    // ------------------------------------------------------------------ chip / button
    internal sealed class NButton : Control
    {
        public bool Primary;
        private bool _hover;

        public NButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = Theme.Bold;
            Size = new Size(Theme.S(110), Theme.S(40));
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fill, fore;
            if (Primary)
            {
                fill = !Enabled ? Color.FromArgb(58, 66, 40) : (_hover ? Color.FromArgb(208, 228, 146) : Theme.Accent);
                fore = Enabled ? Theme.AccentText : Theme.Sub;
            }
            else
            {
                fill = !Enabled ? Color.FromArgb(34, 36, 33) : (_hover ? Theme.BtnHover : Theme.Btn);
                fore = Enabled ? Theme.Text : Theme.Sub;
            }
            using (var p = Theme.Round(r, Theme.S(10)))
            {
                using (var b = new SolidBrush(fill)) g.FillPath(b, p);
                if (!Primary) using (var pen = new Pen(Theme.Border)) g.DrawPath(pen, p);
            }
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (var b = new SolidBrush(fore))
                g.DrawString(Text, Font, b, new RectangleF(0, 0, Width, Height), sf);
        }
    }

    // ------------------------------------------------------------------ sidebar nav item
    internal sealed class NavButton : Control
    {
        public string IconKey = "";
        private bool _hover;
        private bool _selected;

        public NavButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = Theme.Body;
            BackColor = Theme.Panel2;
        }

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);
            var inner = new Rectangle(Theme.S(2), Theme.S(2), Width - Theme.S(5), Height - Theme.S(5));
            if (_selected || _hover)
            {
                using (var p = Theme.Round(inner, Theme.S(12)))
                {
                    using (var b = new SolidBrush(_selected ? Color.FromArgb(40, 43, 39) : Theme.Hover)) g.FillPath(b, p);
                    if (_selected) using (var pen = new Pen(Color.FromArgb(66, 71, 62))) g.DrawPath(pen, p);
                }
            }
            Color fore = _selected ? Theme.Text : Theme.Sub;
            int ico = Theme.S(18);
            Icons.Draw(g, IconKey, new Rectangle(inner.X + Theme.S(16), inner.Y + (inner.Height - ico) / 2, ico, ico), _selected ? Theme.Accent : fore);
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            using (var b = new SolidBrush(fore))
                g.DrawString(Text, Font, b, new RectangleF(inner.X + Theme.S(46), inner.Y, inner.Width - Theme.S(52), inner.Height), sf);
        }
    }

    // ------------------------------------------------------------------ progress bar
    internal sealed class ThinProgress : Control
    {
        private double _value;

        public ThinProgress()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = Theme.S(10);
        }

        public double Value
        {
            get { return _value; }
            set { _value = Math.Max(0, Math.Min(1, value)); Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            var track = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var p = Theme.Round(track, Height / 2))
            using (var b = new SolidBrush(Color.FromArgb(40, 43, 39)))
                g.FillPath(b, p);
            int w = (int)((Width - 1) * _value);
            if (w > Height)
            {
                using (var p = Theme.Round(new Rectangle(0, 0, w, Height - 1), Height / 2))
                using (var b = new LinearGradientBrush(new Rectangle(0, 0, Math.Max(2, w), Height), Theme.AccentDim, Theme.Accent, 0f))
                    g.FillPath(b, p);
            }
        }
    }

    // ------------------------------------------------------------------ wallpaper thumbnail tile
    internal sealed class WallpaperTile : Control
    {
        public Image Thumb;
        public string Caption = "";
        private bool _selected;
        private bool _hover;

        public WallpaperTile()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = Theme.Small;
            BackColor = Theme.Card;
        }

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            int imgH = Width * 9 / 16;
            var img = new Rectangle(1, 1, Width - 3, imgH);
            using (var path = Theme.Round(img, Theme.S(10)))
            {
                if (Thumb != null)
                {
                    g.SetClip(path);
                    g.DrawImage(Thumb, img);
                    g.ResetClip();
                }
                else
                {
                    using (var b = new SolidBrush(Theme.Input)) g.FillPath(b, path);
                }
                using (var pen = new Pen(_selected ? Theme.Accent : (_hover ? Color.FromArgb(96, 102, 90) : Theme.Border), _selected ? 2.5f : 1f))
                    g.DrawPath(pen, path);
            }

            if (_selected)
            {
                int d = Theme.S(20);
                var badge = new Rectangle(img.Right - d - Theme.S(8), img.Y + Theme.S(8), d, d);
                using (var b = new SolidBrush(Theme.Accent)) g.FillEllipse(b, badge);
                using (var pen = new Pen(Theme.AccentText, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                    g.DrawLines(pen, new[]
                    {
                        new PointF(badge.X + d * 0.27f, badge.Y + d * 0.52f),
                        new PointF(badge.X + d * 0.44f, badge.Y + d * 0.70f),
                        new PointF(badge.X + d * 0.74f, badge.Y + d * 0.32f)
                    });
            }

            using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            using (var b = new SolidBrush(_selected ? Theme.Text : Theme.Sub))
                g.DrawString(Caption, Font, b, new RectangleF(2, imgH + Theme.S(8), Width - 4, Height - imgH - Theme.S(8)), sf);
        }
    }

    // ------------------------------------------------------------------ dark dropdown menu
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        private sealed class Colors : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return Theme.Card; } }
            public override Color MenuBorder { get { return Theme.Border; } }
            public override Color MenuItemBorder { get { return Theme.Border; } }
            public override Color MenuItemSelected { get { return Theme.BtnHover; } }
            public override Color MenuItemSelectedGradientBegin { get { return Theme.BtnHover; } }
            public override Color MenuItemSelectedGradientEnd { get { return Theme.BtnHover; } }
            public override Color ImageMarginGradientBegin { get { return Theme.Card; } }
            public override Color ImageMarginGradientMiddle { get { return Theme.Card; } }
            public override Color ImageMarginGradientEnd { get { return Theme.Card; } }
        }

        public DarkMenuRenderer() : base(new Colors()) { RoundedEdges = false; }
    }

    // ------------------------------------------------------------------ rounded dropdown (replaces the white system ComboBox)
    internal sealed class NSelect : Control
    {
        public readonly List<object> Items = new List<object>();
        private int _index = -1;
        private bool _hover;
        public event EventHandler SelectionChangeCommitted;

        public NSelect()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = Theme.Body;
            Size = new Size(Theme.S(240), Theme.S(40));
            BackColor = Theme.Card;
        }

        public int SelectedIndex
        {
            get { return _index; }
            set { _index = (value >= 0 && value < Items.Count) ? value : -1; Invalidate(); }
        }

        public object SelectedItem
        {
            get { return (_index >= 0 && _index < Items.Count) ? Items[_index] : null; }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            var menu = new ContextMenuStrip
            {
                Renderer = new DarkMenuRenderer(), BackColor = Theme.Card, ForeColor = Theme.Text, Font = Font,
                ShowImageMargin = false, ShowCheckMargin = false, MinimumSize = new Size(Width, 0)
            };
            for (int i = 0; i < Items.Count; i++)
            {
                int index = i;
                var item = new ToolStripMenuItem((i == _index ? "\u2022  " : "     ") + Items[i])
                {
                    ForeColor = i == _index ? Theme.Accent : Theme.Text, BackColor = Theme.Card,
                    Padding = new Padding(Theme.S(4), Theme.S(5), Theme.S(4), Theme.S(5))
                };
                item.Click += (s, ev) =>
                {
                    _index = index;
                    Invalidate();
                    EventHandler h = SelectionChangeCommitted;
                    if (h != null) h(this, EventArgs.Empty);
                };
                menu.Items.Add(item);
            }
            menu.Show(this, new Point(0, Height + Theme.S(4)));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var p = Theme.Round(r, Theme.S(10)))
            {
                using (var b = new SolidBrush(_hover ? Theme.BtnHover : Theme.Input)) g.FillPath(b, p);
                using (var pen = new Pen(Theme.Border)) g.DrawPath(pen, p);
            }
            string text = SelectedItem == null ? "" : SelectedItem.ToString();
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            using (var b = new SolidBrush(Theme.Text))
                g.DrawString(text, Font, b, new RectangleF(Theme.S(14), 0, Width - Theme.S(44), Height), sf);

            // chevron
            float cx = Width - Theme.S(22), cy = Height / 2f, d = Theme.S(4);
            using (var pen = new Pen(Theme.Sub, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                g.DrawLines(pen, new[] { new PointF(cx - d, cy - d / 2f), new PointF(cx, cy + d / 2f), new PointF(cx + d, cy - d / 2f) });
        }
    }

    // ------------------------------------------------------------------ rounded text box
    internal sealed class NInput : Card
    {
        public readonly TextBox Box;

        public NInput()
        {
            Fill = Theme.Input;
            Radius = 10;
            BackColor = Theme.Card;
            Height = Theme.S(38);
            Box = new TextBox { BorderStyle = BorderStyle.None, BackColor = Theme.Input, ForeColor = Theme.Text, Font = Theme.Body };
            Controls.Add(Box);
            SizeChanged += (s, e) =>
            {
                Box.Left = Theme.S(14);
                Box.Width = Math.Max(Theme.S(40), Width - Theme.S(28));
                Box.Top = (Height - Box.Height) / 2;
            };
        }

        public override string Text
        {
            get { return Box.Text; }
            set { Box.Text = value; }
        }

        public string PlaceholderText
        {
            get { return Box.PlaceholderText; }
            set { Box.PlaceholderText = value; }
        }

        public void Clear() { Box.Clear(); }
    }
}
