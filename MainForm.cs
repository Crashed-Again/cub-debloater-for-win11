using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace Cub
{
    internal sealed class BrowserChoice
    {
        public string Name;
        public string Id;
        public BrowserChoice(string name, string id) { Name = name; Id = id; }
        public override string ToString() { return Name; }
    }

    internal sealed class PinDef
    {
        public string Key; public string Title; public string AppId; public bool Default;
        public PinDef(string key, string title, string appId, bool def) { Key = key; Title = title; AppId = appId; Default = def; }
    }

    internal sealed class AppDef
    {
        public string Title; public string Patterns; public string Desc; public bool Default;
        public AppDef(string title, string patterns, string desc, bool def) { Title = title; Patterns = patterns; Desc = desc; Default = def; }
    }

    internal sealed class RuntimeDef
    {
        public string Key; public string Title; public string Desc; public string[] Ids; public bool Default;
        public RuntimeDef(string key, string title, string desc, string[] ids, bool def) { Key = key; Title = title; Desc = desc; Ids = ids; Default = def; }
    }

    internal sealed class RowInfo
    {
        public Control Card; public string Page; public string Text;
        public RowInfo(Control card, string page, string text) { Card = card; Page = page; Text = text; }
    }

    internal sealed class Profile
    {
        public Dictionary<string, bool> Toggles { get; set; } = new Dictionary<string, bool>();
        public List<string> CustomApps { get; set; } = new List<string>();
        public string Browser { get; set; } = "";
        public string Wallpaper { get; set; } = "";          // custom image path (empty = use a built-in one)
        public string WallpaperId { get; set; } = "";        // built-in wallpaper id
        public string Extras { get; set; } = "";
    }

    internal sealed class MainForm : Form
    {
        private static int S(int v) { return Theme.S(v); }

        // =========================================================== data
        private static readonly AppDef[] Apps =
        {
            new AppDef("Camera", "Microsoft.WindowsCamera", "", true),
            new AppDef("LinkedIn", "*LinkedIn*", "", true),
            new AppDef("WhatsApp", "*WhatsApp*", "", true),
            new AppDef("Xbox and Game Bar", "Microsoft.GamingApp;Microsoft.XboxApp;Microsoft.XboxGamingOverlay;Microsoft.XboxGameOverlay;Microsoft.XboxIdentityProvider;Microsoft.XboxSpeechToTextOverlay;Microsoft.Xbox.TCUI", "All Xbox components", true),
            new AppDef("Clipchamp", "Clipchamp.Clipchamp", "", true),
            new AppDef("News", "Microsoft.BingNews", "", true),
            new AppDef("Weather", "Microsoft.BingWeather", "", true),
            new AppDef("Bing Search", "Microsoft.BingSearch", "", true),
            new AppDef("Solitaire Collection", "Microsoft.MicrosoftSolitaireCollection", "", true),
            new AppDef("Microsoft Teams", "MSTeams;MicrosoftTeams", "", true),
            new AppDef("Microsoft To Do", "Microsoft.Todos", "", true),
            new AppDef("Skype", "Microsoft.SkypeApp", "", true),
            new AppDef("People", "Microsoft.People", "", true),
            new AppDef("Feedback Hub", "Microsoft.WindowsFeedbackHub", "", true),
            new AppDef("Get Help and Tips", "Microsoft.GetHelp;Microsoft.Getstarted", "", true),
            new AppDef("Mixed Reality and 3D", "Microsoft.MixedReality.Portal;Microsoft.Microsoft3DViewer;Microsoft.Print3D", "", true),
            new AppDef("Microsoft 365 hub", "Microsoft.MicrosoftOfficeHub", "", true),
            new AppDef("OneNote", "Microsoft.Office.OneNote", "", true),
            new AppDef("New Outlook", "Microsoft.OutlookForWindows", "", true),
            new AppDef("Dev Home", "Microsoft.Windows.DevHome", "", true),
            new AppDef("Power Automate", "Microsoft.PowerAutomateDesktop", "", true),
            new AppDef("Phone Link", "Microsoft.YourPhone;MicrosoftWindows.CrossDevice", "", true),
            new AppDef("Maps", "Microsoft.WindowsMaps", "", true),
            new AppDef("Media Player and Movies", "Microsoft.ZuneMusic;Microsoft.ZuneVideo", "", true),
            new AppDef("Sound Recorder", "Microsoft.WindowsSoundRecorder", "", true),
            new AppDef("Sticky Notes", "Microsoft.MicrosoftStickyNotes", "", true),
            new AppDef("Quick Assist", "MicrosoftCorporationII.QuickAssist", "", true),
            new AppDef("Microsoft Family", "MicrosoftCorporationII.MicrosoftFamily", "", true),
            new AppDef("Mail and Calendar", "Microsoft.WindowsCommunicationsApps", "", true),
            new AppDef("Wallet", "Microsoft.Wallet", "", true),
            new AppDef("Sponsored apps", "*Spotify*;*TikTok*;*Instagram*;*Facebook*;*Disney*;*Netflix*;*PrimeVideo*;*CandyCrush*;*Twitter*;*Duolingo*;*AdobeExpress*;*Hulu*", "Spotify, TikTok, Instagram, Netflix, Disney+, Candy Crush and friends", true),
            // off by default - tick them if you want them gone too
            new AppDef("Photos", "Microsoft.Windows.Photos", "Off by default", false),
            new AppDef("Paint", "Microsoft.Paint", "Off by default", false),
            new AppDef("Snipping Tool", "Microsoft.ScreenSketch", "Off by default", false),
            new AppDef("Windows Terminal", "Microsoft.WindowsTerminal", "Off by default", false)
        };

        private static readonly PinDef[] Pins =
        {
            new PinDef("calc", "Calculator", "Microsoft.WindowsCalculator_8wekyb3d8bbwe!App", true),
            new PinDef("clock", "Clock", "Microsoft.WindowsAlarms_8wekyb3d8bbwe!App", true),
            new PinDef("notepad", "Notepad", "Microsoft.WindowsNotepad_8wekyb3d8bbwe!App", true),
            new PinDef("paint", "Paint", "Microsoft.Paint_8wekyb3d8bbwe!App", false),
            new PinDef("photos", "Photos", "Microsoft.Windows.Photos_8wekyb3d8bbwe!App", false),
            new PinDef("snip", "Snipping Tool", "Microsoft.ScreenSketch_8wekyb3d8bbwe!App", false),
            new PinDef("store", "Microsoft Store", "Microsoft.WindowsStore_8wekyb3d8bbwe!App", false),
            new PinDef("term", "Windows Terminal", "Microsoft.WindowsTerminal_8wekyb3d8bbwe!App", false),
            new PinDef("settings", "Settings", "windows.immersivecontrolpanel_cw5n1h2txyewy!microsoft.windows.immersivecontrolpanel", false)
        };

        private static readonly RuntimeDef[] Runtimes =
        {
            new RuntimeDef("vc2015", "Visual C++ 2015-2022 (x64 and x86)", "Needed by most modern apps and games", new[] { "Microsoft.VCRedist.2015+.x64", "Microsoft.VCRedist.2015+.x86" }, true),
            new RuntimeDef("vc2013", "Visual C++ 2013 (x64 and x86)", "Needed by many older apps and games", new[] { "Microsoft.VCRedist.2013.x64", "Microsoft.VCRedist.2013.x86" }, true),
            new RuntimeDef("vc2012", "Visual C++ 2012 (x64 and x86)", "Very old programs only", new[] { "Microsoft.VCRedist.2012.x64", "Microsoft.VCRedist.2012.x86" }, false),
            new RuntimeDef("vc2010", "Visual C++ 2010 (x64 and x86)", "Very old programs only", new[] { "Microsoft.VCRedist.2010.x64", "Microsoft.VCRedist.2010.x86" }, false),
            new RuntimeDef("net8", ".NET Desktop Runtime 8", "For apps built on .NET 8", new[] { "Microsoft.DotNet.DesktopRuntime.8" }, true),
            new RuntimeDef("net10", ".NET Desktop Runtime 10", "For apps built on .NET 10", new[] { "Microsoft.DotNet.DesktopRuntime.10" }, true),
            new RuntimeDef("directx", "DirectX End-User Runtime", "Legacy DirectX files that older games ask for", new[] { "Microsoft.DirectX" }, true),
            new RuntimeDef("webview2", "WebView2 Runtime", "Used by many modern desktop apps", new[] { "Microsoft.EdgeWebView2Runtime" }, true)
        };

        private static readonly BrowserChoice[] Browsers =
        {
            new BrowserChoice("Don't change my browser", ""),
            new BrowserChoice("Brave", "Brave.Brave"),
            new BrowserChoice("Firefox", "Mozilla.Firefox"),
            new BrowserChoice("Google Chrome", "Google.Chrome"),
            new BrowserChoice("Vivaldi", "Vivaldi.Vivaldi"),
            new BrowserChoice("Opera", "Opera.Opera"),
            new BrowserChoice("Keep Edge (just update it)", "Microsoft.Edge")
        };

        private static readonly string[] PageNames =
        {
            "Appearance", "AI", "Apps", "Start menu", "Privacy and security", "Performance", "Software", "Drivers and runtimes", "Apply"
        };

        // =========================================================== state
        private readonly Dictionary<string, ToggleSwitch> _t = new Dictionary<string, ToggleSwitch>();
        private readonly Dictionary<string, bool> _def = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> _cat = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _appPat = new Dictionary<string, string>();    // toggle key -> app patterns
        private readonly Dictionary<string, string[]> _rtIds = new Dictionary<string, string[]>(); // toggle key -> winget ids
        private readonly Dictionary<string, Control> _pages = new Dictionary<string, Control>();
        private readonly List<NavButton> _nav = new List<NavButton>();
        private readonly List<RowInfo> _rows = new List<RowInfo>();
        private readonly List<string> _customApps = new List<string>();
        private readonly List<WallpaperTile> _tiles = new List<WallpaperTile>();

        private BackdropPanel _root;
        private TPanel _main;
        private Card _content, _side, _top, _bottom;
        private FlowLayoutPanel _appsFlow, _wpStrip;
        private NSelect _browser, _preset;
        private NInput _extras, _customBox, _searchBox;
        private TextBox _search;
        private ListView _results;
        private NButton _searchBtn, _apply, _restart;
        private Label _searchStatus, _wpLabel, _status;
        private RichTextBox _log;
        private ThinProgress _prog;
        private Rectangle _searchRect, _chipRect;

        private string _wallpaperId = WallpaperCatalog.DefaultId;
        private string _wallpaper = "";     // custom image path
        private string _buildPage = "";
        private string _current = "Appearance";
        private bool _running;

        private static readonly string SettingsDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cub");
        private static readonly string LastProfile = Path.Combine(SettingsDir, "last-profile.json");

        // =========================================================== window
        public MainForm()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                Theme.Scale = g.DpiX / 96f;

            Text = "Cub by NeonBear";
            try
            {
                using (Stream ico = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("icon.ico"))
                    if (ico != null) Icon = new Icon(ico);
            }
            catch { }
            BackColor = Theme.Glass;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(1200), S(800));
            MinimumSize = new Size(S(1000), S(680));
            HandleCreated += (s, e) =>
            {
                DarkMode.TitleBar(Handle, Theme.Glass, Theme.Text);
            };

            BuildShell();

            BuildAppearance();
            BuildAi();
            BuildApps();
            BuildStart();
            BuildPrivacy();
            BuildPerformance();
            BuildSoftware();
            BuildDrivers();
            BuildApplyPage();

            ShowPage("Appearance");
            LoadProfileFile(LastProfile, true);
            UpdateWallpaperUi();
            ApplyLook();
        }

        /// <summary>Lets a little of the desktop show through the whole window.</summary>
        private void ApplyLook()
        {
            Opacity = On("ui_translucent") ? 0.96 : 1.0;
            if (_root != null)
            {
                _root.ShowImage = On("ui_blur");
                _root.Invalidate(true);
            }
        }

        /// <summary>The window background is a blurred copy of the wallpaper that is currently selected.</summary>
        private void UpdateBackdrop()
        {
            if (_root == null) return;
            try
            {
                if (!string.IsNullOrEmpty(_wallpaper) && File.Exists(_wallpaper))
                {
                    using (Image full = Image.FromFile(_wallpaper)) _root.SetSource(full);
                }
                else
                {
                    using (Image thumb = WallpaperCatalog.LoadThumb(_wallpaperId)) _root.SetSource(thumb);
                }
            }
            catch { _root.SetSource(null); }
        }

        // =========================================================== shell: sidebar, top bar, bottom bar
        private void BuildShell()
        {
            _root = new BackdropPanel { Dock = DockStyle.Fill, Padding = new Padding(S(14)) };
            Controls.Add(_root);

            // dock order: Fill first, then the edges (the one added last sits outermost)
            _main = new TPanel { Dock = DockStyle.Fill };
            _root.Controls.Add(_main);
            _root.Controls.Add(new TPanel { Dock = DockStyle.Left, Width = S(14) });
            BuildSidebar();

            _content = new Card
            {
                Dock = DockStyle.Fill, Fill = Theme.Panel, BackColor = Color.Transparent, Radius = 18,
                Padding = new Padding(S(26), S(20), S(16), S(16))
            };
            _main.Controls.Add(_content);
            _main.Controls.Add(new TPanel { Dock = DockStyle.Top, Height = S(12) });
            _main.Controls.Add(new TPanel { Dock = DockStyle.Bottom, Height = S(12) });
            BuildTopBar();
            BuildBottomBar();
        }

        private void BuildSidebar()
        {
            _side = new Card { Dock = DockStyle.Left, Width = S(248), Fill = Theme.Panel2, FillAlpha = 170, BorderColor = Color.FromArgb(48, 255, 255, 255), BackColor = Color.Transparent, Radius = 18 };
            _side.Resize += (s, e) => _side.Invalidate();
            _side.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                try
                {
                    using (var ico = new Icon(Icon, new Size(S(46), S(46))))
                        g.DrawIcon(ico, new Rectangle(S(20), S(22), S(46), S(46)));
                }
                catch { }
                using (var big = new Font(Theme.Family, 18f, FontStyle.Bold, GraphicsUnit.Point))
                using (var white = new SolidBrush(Theme.Text))
                using (var grey = new SolidBrush(Theme.Sub))
                {
                    g.DrawString("Cub", big, white, S(76), S(18));
                    g.DrawString("by NeonBear", Theme.Small, grey, S(78), S(50));
                }

                // bottom promo card, like the "Upgrade" card in tPay (only when there is room)
                int cardH = S(112);
                int top = _side.Height - cardH - S(16);
                if (top > S(104) + PageNames.Length * S(50) + S(10))
                {
                    var rc = new Rectangle(S(14), top, _side.Width - S(29), cardH);
                    using (var p = Theme.Round(rc, S(14)))
                    {
                        using (var b = new SolidBrush(Theme.Card)) g.FillPath(b, p);
                        using (var pen = new Pen(Theme.Border)) g.DrawPath(pen, p);
                    }
                    Theme.DrawDots(g, "NEONBEAR", rc.X + S(16), rc.Y + S(16), S(3), S(2), Theme.Sub);
                    using (var white = new SolidBrush(Theme.Text))
                    using (var grey = new SolidBrush(Theme.Sub))
                    {
                        g.DrawString("Cub " + Theme.VersionText(), Theme.Bold, white, rc.X + S(14), rc.Y + S(50));
                        g.DrawString("by NeonBear", Theme.Small, grey, rc.X + S(16), rc.Y + S(74));
                    }
                }
            };

            int y = S(96);
            foreach (string name in PageNames)
            {
                var nb = new NavButton
                {
                    Text = name, IconKey = name, Left = S(12), Top = y, Width = S(248) - S(25), Height = S(46),
                    Tag = name, BackColor = Color.Transparent
                };
                string captured = name;
                nb.Click += (s, e) => ShowPage(captured);
                _side.Controls.Add(nb);
                _nav.Add(nb);
                y += S(50);
            }
            _root.Controls.Add(_side);
        }

        private void BuildTopBar()
        {
            _top = new Card { Dock = DockStyle.Top, Height = S(72), Fill = Theme.Panel2, FillAlpha = 170, BorderColor = Color.FromArgb(48, 255, 255, 255), BackColor = Color.Transparent, Radius = 18 };
            _search = new TextBox
            {
                BorderStyle = BorderStyle.None, BackColor = Theme.Input, ForeColor = Theme.Text, Font = Theme.Body,
                PlaceholderText = "Search every setting..."
            };
            _search.TextChanged += (s, e) => OnSearch();
            _top.Controls.Add(_search);

            _top.SizeChanged += (s, e) =>
            {
                int h = S(46);
                int y = (_top.Height - h) / 2;
                int chipW = S(250);
                _chipRect = new Rectangle(_top.Width - S(18) - chipW, y, chipW, h);
                _searchRect = new Rectangle(S(18), y, Math.Max(S(120), _chipRect.X - S(14) - S(18)), h);
                _search.Left = _searchRect.X + S(46);
                _search.Width = Math.Max(S(60), _searchRect.Width - S(46) - S(16));
                _search.Top = _searchRect.Y + (_searchRect.Height - _search.Height) / 2;
                _top.Invalidate();
            };
            _top.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                using (var p = Theme.Round(_searchRect, S(14)))
                {
                    using (var b = new SolidBrush(Theme.Input)) g.FillPath(b, p);
                    using (var pen = new Pen(Theme.Border)) g.DrawPath(pen, p);
                }
                int ico = S(18);
                Icons.Draw(g, "Search", new Rectangle(_searchRect.X + S(16), _searchRect.Y + (_searchRect.Height - ico) / 2, ico, ico), Theme.Sub);

                using (var p = Theme.Round(_chipRect, S(14)))
                {
                    using (var b = new SolidBrush(Theme.Input)) g.FillPath(b, p);
                    using (var pen = new Pen(Theme.Border)) g.DrawPath(pen, p);
                }
                try
                {
                    using (var i = new Icon(Icon, new Size(S(32), S(32))))
                        g.DrawIcon(i, new Rectangle(_chipRect.X + S(10), _chipRect.Y + (_chipRect.Height - S(32)) / 2, S(32), S(32)));
                }
                catch { }
                using (var white = new SolidBrush(Theme.Text))
                using (var grey = new SolidBrush(Theme.Sub))
                {
                    g.DrawString("Cub", Theme.Bold, white, _chipRect.X + S(52), _chipRect.Y + S(3));
                    g.DrawString("by NeonBear", Theme.Small, grey, _chipRect.X + S(53), _chipRect.Y + S(22));
                    using (var sf = new StringFormat { Alignment = StringAlignment.Far })
                        g.DrawString("v" + Theme.VersionText(), Theme.Small, grey, new RectangleF(_chipRect.Right - S(70), _chipRect.Y + S(13), S(58), S(20)), sf);
                }
            };
            _main.Controls.Add(_top);
        }

        private void BuildBottomBar()
        {
            _bottom = new Card { Dock = DockStyle.Bottom, Height = S(78), Fill = Theme.Panel2, FillAlpha = 170, BorderColor = Color.FromArgb(48, 255, 255, 255), BackColor = Color.Transparent, Radius = 18 };

            var lbl = new Label { Text = "Preset", ForeColor = Theme.Sub, BackColor = Color.Transparent, AutoSize = true, Left = S(24), Top = S(29), Font = Theme.Body };
            _bottom.Controls.Add(lbl);

            _preset = new NSelect { Left = S(84), Top = S(19), Width = S(260), Height = S(40), BackColor = Color.Transparent };
            _preset.Items.AddRange(new object[]
            {
                "Recommended (my setup)", "Light touch (look + AI only)", "Aggressive (everything on)", "Nothing (clear all)"
            });
            _preset.SelectedIndex = 0;
            _preset.SelectionChangeCommitted += (s, e) => ApplyPreset(_preset.SelectedIndex);
            _bottom.Controls.Add(_preset);

            var save = new NButton { Text = "Save profile", Left = S(360), Top = S(19), Width = S(124), Height = S(40), BackColor = Color.Transparent };
            save.Click += (s, e) => SaveProfileDialog();
            _bottom.Controls.Add(save);

            var load = new NButton { Text = "Load profile", Left = S(492), Top = S(19), Width = S(124), Height = S(40), BackColor = Color.Transparent };
            load.Click += (s, e) => LoadProfileDialog();
            _bottom.Controls.Add(load);

            _apply = new NButton { Text = "Apply changes", Primary = true, Width = S(200), Height = S(46), Top = S(16), BackColor = Color.Transparent };
            _apply.Click += (s, e) => OnApply();
            _bottom.Controls.Add(_apply);

            _bottom.SizeChanged += (s, e) => { _apply.Left = _bottom.Width - _apply.Width - S(24); };
            _apply.Left = _bottom.Width - _apply.Width - S(24);
            _main.Controls.Add(_bottom);
        }

        // =========================================================== page building helpers
        private FlowLayoutPanel NewPage(string name, string title, string sub)
        {
            _buildPage = name;
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoScroll = true, BackColor = Theme.Panel, Visible = false
            };
            flow.SizeChanged += (s, e) => FitWidth(flow);
            DarkMode.Scrollbars(flow);

            flow.Controls.Add(new Label
            {
                Text = title, Font = Theme.Title, ForeColor = Theme.Text, BackColor = Theme.Panel,
                AutoSize = false, Height = S(48), Margin = new Padding(0)
            });
            flow.Controls.Add(new Label
            {
                Text = sub, Font = Theme.Body, ForeColor = Theme.Sub, BackColor = Theme.Panel,
                AutoSize = false, Height = S(30), Margin = new Padding(0, 0, 0, S(8))
            });

            _content.Controls.Add(flow);
            _pages[name] = flow;
            return flow;
        }

        private void FitWidth(FlowLayoutPanel flow)
        {
            int w = flow.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - S(6);
            if (w < S(200)) return;
            foreach (Control c in flow.Controls) c.Width = w;
        }

        private void SubHeading(FlowLayoutPanel page, string text)
        {
            page.Controls.Add(new Label
            {
                Text = text, Font = Theme.Bold, ForeColor = Theme.Text, BackColor = Theme.Panel,
                AutoSize = false, Height = S(32), Margin = new Padding(0, S(10), 0, S(4))
            });
        }

        private void Register(Control card, string text)
        {
            _rows.Add(new RowInfo(card, _buildPage, (text ?? "").ToLowerInvariant()));
        }

        private Card NewRow(int height)
        {
            return new Card { Height = height, Fill = Theme.Card, BackColor = Theme.Panel, Radius = 12, Margin = new Padding(0, 0, 0, S(8)) };
        }

        /// <summary>One row = one setting on one line: title, description, toggle on the right.</summary>
        private ToggleSwitch AddToggle(FlowLayoutPanel page, string key, string category, string title, string desc,
                                       bool def, bool warn = false, int height = 0)
        {
            bool compact = string.IsNullOrEmpty(desc);
            int h = height > 0 ? height : (compact ? S(52) : S(74));
            Card card = NewRow(h);

            var t = new Label
            {
                Text = title, Font = Theme.Bold, ForeColor = Theme.Text, BackColor = Theme.Card,
                AutoSize = false, AutoEllipsis = true, Left = S(20), Height = S(24)
            };
            t.Top = compact ? (h - t.Height) / 2 : S(13);
            card.Controls.Add(t);

            Label d = null;
            if (!compact)
            {
                d = new Label
                {
                    Text = desc, Font = Theme.Small, ForeColor = warn ? Theme.Warn : Theme.Sub, BackColor = Theme.Card,
                    AutoSize = false, AutoEllipsis = true, Left = S(20), Top = S(40), Height = S(22)
                };
                card.Controls.Add(d);
            }

            var sw = new ToggleSwitch { Checked = def };
            card.Controls.Add(sw);

            card.SizeChanged += (s, e) =>
            {
                sw.Left = card.Width - sw.Width - S(22);
                sw.Top = (card.Height - sw.Height) / 2;
                int w = Math.Max(S(60), sw.Left - S(38));
                t.Width = w;
                if (d != null) d.Width = w;
            };

            page.Controls.Add(card);
            _t[key] = sw;
            _def[key] = def;
            _cat[key] = category;
            Register(card, title + " " + desc);
            return sw;
        }

        private Label CardLabel(string text, Font font, Color color, int left, int top, int height, int width)
        {
            return new Label
            {
                Text = text, Font = font, ForeColor = color, BackColor = Theme.Card, AutoSize = false, AutoEllipsis = true,
                Left = left, Top = top, Height = height, Width = width
            };
        }

        private NInput Input()
        {
            return new NInput();
        }

        // =========================================================== pages
        private void BuildAppearance()
        {
            var p = NewPage("Appearance", "Appearance", "How Windows looks and feels. Flip anything off to leave it alone.");
            AddToggle(p, "dark", "Appearance", "Dark mode", "Apps and Windows itself use the dark theme.", true);
            AddToggle(p, "left", "Appearance", "Taskbar icons on the left", "Windows 10 style alignment. Off = keep centered.", true);
            AddToggle(p, "thispc", "Appearance", "File Explorer opens to This PC", "Instead of Home / Quick access.", true);
            AddToggle(p, "ui_blur", "UI", "Blurred glass look", "Frosted sidebar and bars over a blurred copy of your wallpaper (follows the wallpaper you pick).", true)
                .CheckedChanged += (s, e) => ApplyLook();
            AddToggle(p, "ui_translucent", "UI", "Slightly transparent window", "Lets a little of your real desktop show through Cub itself.", true)
                .CheckedChanged += (s, e) => ApplyLook();
            AddToggle(p, "wallpaper", "Appearance", "Set wallpaper", "Applies the wallpaper you pick below.", true);

            // wallpaper picker: one scrolling line of thumbnails
            var card = new Card { Height = S(262), Fill = Theme.Card, BackColor = Theme.Panel, Radius = 12, Margin = new Padding(0, 0, 0, S(8)) };
            var cap = CardLabel("Wallpaper", Theme.Bold, Theme.Text, S(20), S(14), S(24), S(300));
            _wpLabel = CardLabel("", Theme.Small, Theme.Sub, S(20), S(40), S(22), S(400));
            var choose = new NButton { Text = "Use my own image...", Width = S(190), Height = S(36), BackColor = Theme.Card };
            choose.Click += (s, e) => ChooseWallpaper();

            _wpStrip = new FlowLayoutPanel
            {
                Left = S(14), Top = S(72), Height = S(176), FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
                AutoScroll = true, BackColor = Theme.Card, Padding = new Padding(S(6), S(2), 0, 0)
            };
            DarkMode.Scrollbars(_wpStrip);
            foreach (WallpaperDef def in WallpaperCatalog.All)
            {
                var tile = new WallpaperTile
                {
                    Width = S(200), Height = S(142), Tag = def.Id, BackColor = Theme.Card,
                    Caption = def.Name + (def.Id == WallpaperCatalog.DefaultId ? " (default)" : ""),
                    Thumb = WallpaperCatalog.LoadThumb(def.Id), Margin = new Padding(0, 0, S(14), 0)
                };
                tile.Click += (s, e) =>
                {
                    _wallpaperId = (string)((Control)s).Tag;
                    _wallpaper = "";
                    UpdateWallpaperUi();
                };
                _tiles.Add(tile);
                _wpStrip.Controls.Add(tile);
            }

            card.Controls.AddRange(new Control[] { cap, _wpLabel, choose, _wpStrip });
            card.SizeChanged += (s, e) =>
            {
                choose.Left = card.Width - choose.Width - S(20);
                choose.Top = S(14);
                _wpStrip.Width = Math.Max(S(200), card.Width - S(28));
                _wpLabel.Width = Math.Max(S(60), choose.Left - S(40));
            };
            p.Controls.Add(card);
            Register(card, "wallpaper background image picture desktop");

            AddToggle(p, "mouse", "Appearance", "Mouse acceleration off", "Turns off \"Enhance pointer precision\" so the cursor moves 1:1.", true);
            AddToggle(p, "sticky", "Appearance", "Disable Sticky Keys shortcut", "No more popup when you tap Shift five times (also Toggle and Filter Keys).", true);
            AddToggle(p, "fileext", "Appearance", "Show file extensions", "See .txt, .exe and so on in File Explorer.", true);
            AddToggle(p, "classicmenu", "Appearance", "Classic right-click menu", "Skip \"Show more options\" on Windows 11.", true);
        }

        private void BuildAi()
        {
            var p = NewPage("AI", "AI", "Get rid of the AI add-ons nobody asked for.");
            AddToggle(p, "copilot", "AI", "Remove Copilot", "Uninstalls the app, hides the taskbar button and sets the policy that switches it off.", true);
            AddToggle(p, "recall", "AI", "Disable Recall and Click to Do", "Stops Windows taking AI snapshots of your screen and analyzing them.", true);
            AddToggle(p, "websearch", "AI", "No web results in Start search", "Search only shows your own apps and files.", true);
        }

        private void BuildApps()
        {
            var p = NewPage("Apps", "Apps", "Pick exactly which built-in apps get removed. Calculator, Clock, Notepad and the Store are never touched.");
            _appsFlow = p;
            AddToggle(p, "removeapps", "Apps", "Remove the apps ticked below", "Master switch. Off = no app is removed, whatever is ticked.", true);
            AddToggle(p, "suggest", "Apps", "Block suggested apps and ads", "Stops Windows reinstalling sponsored apps and showing tips and promotions.", true);

            var card = NewRow(S(88));
            var cap = CardLabel("Add your own app to remove", Theme.Bold, Theme.Text, S(20), S(10), S(24), S(400));
            _customBox = Input();
            _customBox.Left = S(20); _customBox.Top = S(42);
            var add = new NButton { Text = "Add", Width = S(90), Height = S(36), BackColor = Theme.Card };
            card.Controls.AddRange(new Control[] { cap, _customBox, add });
            card.SizeChanged += (s, e) =>
            {
                add.Left = card.Width - add.Width - S(22);
                add.Top = S(42);
                _customBox.Width = Math.Max(S(80), add.Left - S(20) - S(14));
            };
            Action addCustom = () =>
            {
                string v = _customBox.Text.Trim();
                if (v.Length == 0) return;
                AddCustomApp(v, true);
                _customBox.Clear();
            };
            add.Click += (s, e) => addCustom();
            _customBox.Box.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; addCustom(); }
            };
            p.Controls.Add(card);
            Register(card, "add your own app custom package remove");
            new ToolTip().SetToolTip(_customBox.Box, "A package name, e.g. Microsoft.BingNews or *Spotify* (wildcards work). Run Get-AppxPackage in PowerShell to list names.");

            SubHeading(p, "Built-in apps");
            foreach (AppDef a in Apps)
            {
                string key = "app:" + a.Title;
                AddToggle(p, key, "Apps", a.Title, a.Desc, a.Default, false, string.IsNullOrEmpty(a.Desc) ? S(52) : S(66));
                _appPat[key] = a.Patterns;
            }
        }

        private void AddCustomApp(string pattern, bool on)
        {
            string key = "custom:" + pattern;
            if (_t.ContainsKey(key)) { _t[key].Checked = on; return; }
            string saved = _buildPage;
            _buildPage = "Apps";
            AddToggle(_appsFlow, key, "Apps", pattern, "Custom package", on, false, S(66));
            _buildPage = saved;
            _appPat[key] = pattern;
            _customApps.Add(pattern);
            FitWidth(_appsFlow);
        }

        private void BuildStart()
        {
            var p = NewPage("Start menu", "Start menu", "Clean it up and choose what stays pinned.");
            AddToggle(p, "hiderec", "Start", "Hide the Recommended section", "No recent files or suggestions in Start. May need Windows Pro or newer builds.", true);
            AddToggle(p, "grid", "Start", "All apps in grid view", "Instead of the category or list view.", true);
            AddToggle(p, "pins", "Start", "Replace pinned apps with my list", "Master switch for the list below. Off = keep your current pins.", true);
            SubHeading(p, "Pinned apps");
            foreach (PinDef pin in Pins)
                AddToggle(p, "pin:" + pin.Key, "Start", pin.Title, "", pin.Default);
        }

        private void BuildPrivacy()
        {
            var p = NewPage("Privacy and security", "Privacy and security", "Some of these are one-way trips, so they are flagged.");
            AddToggle(p, "telemetry", "Privacy", "Disable telemetry", "Stops the tracking service and tasks. Home and Pro still send a minimal level - a Windows limit.", true);
            AddToggle(p, "location", "Privacy", "Disable location tracking", "Turns off location services for the whole PC.", true);
            AddToggle(p, "gamebar", "Privacy", "Disable Xbox Game Bar (ms-gamingoverlay popup)", "Turns Game Bar off and silences the \"you need an app to open ms-gamingoverlay\" popup.", true);
            AddToggle(p, "sac", "Privacy", "Disable Smart App Control", "Caution: can not be turned back on without resetting Windows.", true, true);
            AddToggle(p, "bitlocker", "Privacy", "Disable BitLocker / device encryption", "Caution: DECRYPTS your drives (slow, runs in the background).", true, true);
            AddToggle(p, "hibernate", "Privacy", "Disable hibernation", "Frees disk space. Also turns off Fast Startup.", true, true);
        }

        private void BuildPerformance()
        {
            var p = NewPage("Performance", "Performance", "Speed over battery life.");
            AddToggle(p, "ultimate", "Performance", "Ultimate Performance power plan", "Creates and activates it. Some laptops do not offer this plan.", true);
        }

        private void BuildSoftware()
        {
            var p = NewPage("Software", "Software", "Installs and updates through winget. The browser step always runs last.");
            AddToggle(p, "quicklook", "Software", "QuickLook", "Select a file and press Space to preview it, like on macOS.", true);

            // browser picker
            var card = NewRow(S(84));
            var cap = CardLabel("Browser", Theme.Bold, Theme.Text, S(20), S(13), S(24), S(300));
            var sub = CardLabel("Installs (or updates) the one you pick. Pick Keep Edge to only update it.", Theme.Small, Theme.Sub, S(20), S(42), S(22), S(400));
            _browser = new NSelect { Width = S(250), Height = S(40), BackColor = Theme.Card };
            _browser.Items.AddRange(Browsers);
            _browser.SelectedIndex = Array.FindIndex(Browsers, b => b.Id == "Google.Chrome");   // Google Chrome by default
            card.Controls.AddRange(new Control[] { cap, sub, _browser });
            card.SizeChanged += (s, e) =>
            {
                _browser.Left = card.Width - _browser.Width - S(22);
                _browser.Top = (card.Height - _browser.Height) / 2;
                sub.Width = Math.Max(S(60), _browser.Left - S(38));
            };
            p.Controls.Add(card);
            Register(card, "browser chrome firefox brave edge vivaldi opera");

            AddToggle(p, "browserupdate", "Software", "Update the browser if it is already installed",
                "Optional. Off = install only when missing and never run an update check.", true);
            AddToggle(p, "removeedge", "Software", "Remove Edge after the new browser installs",
                "Only runs if a different browser was picked AND installed successfully. WebView2 is kept.", true);

            // winget search
            SubHeading(p, "Find and install apps");
            var sc = NewRow(S(342));
            var scap = CardLabel("Search winget", Theme.Bold, Theme.Text, S(20), S(12), S(24), S(300));
            _searchBox = Input();
            _searchBox.Left = S(20); _searchBox.Top = S(44);
            _searchBtn = new NButton { Text = "Search", Width = S(110), Height = S(36), BackColor = Theme.Card };
            var resultsCard = new Card { Left = S(20), Top = S(92), Height = S(194), Fill = Color.FromArgb(24, 25, 23), BackColor = Theme.Card, Radius = 10, Padding = new Padding(S(10), S(8), S(6), S(8)) };
            _results = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details, FullRowSelect = true, MultiSelect = true, HeaderStyle = ColumnHeaderStyle.None,
                BackColor = Color.FromArgb(24, 25, 23), ForeColor = Theme.Text, BorderStyle = BorderStyle.None,
                Font = Theme.Body, HideSelection = false
            };
            _results.Columns.Add("Name", S(260));
            _results.Columns.Add("Id", S(260));
            _results.Columns.Add("Version", S(100));
            DarkMode.Scrollbars(_results);
            resultsCard.Controls.Add(_results);
            _searchStatus = CardLabel("Type a name (e.g. vlc, discord, 7zip) and press Search.", Theme.Small, Theme.Sub, S(20), S(300), S(26), S(400));
            var addSel = new NButton { Text = "Add selected", Width = S(140), Height = S(36), BackColor = Theme.Card };
            sc.Controls.AddRange(new Control[] { scap, _searchBox, _searchBtn, resultsCard, _searchStatus, addSel });
            sc.SizeChanged += (s, e) =>
            {
                _searchBtn.Left = sc.Width - _searchBtn.Width - S(22);
                _searchBtn.Top = S(45);
                _searchBox.Width = Math.Max(S(80), _searchBtn.Left - S(20) - S(14));
                resultsCard.Width = Math.Max(S(200), sc.Width - S(42));
                addSel.Left = sc.Width - addSel.Width - S(22);
                addSel.Top = S(296);
                _searchStatus.Width = Math.Max(S(60), addSel.Left - S(40));
                int w = resultsCard.Width - S(16) - SystemInformation.VerticalScrollBarWidth - S(4);
                _results.Columns[0].Width = (int)(w * 0.42);
                _results.Columns[1].Width = (int)(w * 0.40);
                _results.Columns[2].Width = (int)(w * 0.18);
            };
            _searchBtn.Click += (s, e) => DoSearch();
            _searchBox.Box.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DoSearch(); }
            };
            addSel.Click += (s, e) => AddSelectedResults();
            _results.DoubleClick += (s, e) => AddSelectedResults();
            p.Controls.Add(sc);
            Register(sc, "search winget find install apps");

            // the editable install list
            var ex = NewRow(S(112));
            var excap = CardLabel("Apps that will be installed", Theme.Bold, Theme.Text, S(20), S(10), S(24), S(400));
            var exsub = CardLabel("winget package IDs, separated by commas. Search results land here; you can also type or delete IDs.", Theme.Small, Theme.Sub, S(20), S(37), S(22), S(400));
            _extras = Input();
            _extras.Left = S(20); _extras.Top = S(64);
            ex.Controls.AddRange(new Control[] { excap, exsub, _extras });
            ex.SizeChanged += (s, e) =>
            {
                _extras.Width = Math.Max(S(80), ex.Width - S(42));
                exsub.Width = Math.Max(S(60), ex.Width - S(42));
            };
            p.Controls.Add(ex);
            Register(ex, "apps that will be installed list winget ids");
        }

        private void BuildDrivers()
        {
            var p = NewPage("Drivers and runtimes", "Drivers and runtimes", "Fill in what Windows is missing. Only missing runtimes are installed.");
            AddToggle(p, "runtimes", "Runtimes", "Install missing runtimes", "Checks each runtime below and installs only the ones that are not there yet.", true);
            SubHeading(p, "Runtimes to check");
            foreach (RuntimeDef r in Runtimes)
            {
                string key = "rt:" + r.Key;
                AddToggle(p, key, "Runtimes", r.Title, r.Desc, r.Default, false, S(66));
                _rtIds[key] = r.Ids;
            }
            SubHeading(p, "Drivers");
            AddToggle(p, "drivers", "Drivers", "Install and update drivers", "Downloads missing and newer drivers from Windows Update. Can take a while and may need a restart.", true);
        }

        private void BuildApplyPage()
        {
            _buildPage = "Apply";
            var page = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Visible = false };

            // dock order: Fill first, then the top items from bottom to top
            var logCard = new Card { Dock = DockStyle.Fill, Fill = Color.FromArgb(14, 15, 14), BackColor = Theme.Panel, Radius = 12, Padding = new Padding(S(16)) };
            _log = new RichTextBox
            {
                Dock = DockStyle.Fill, BackColor = Color.FromArgb(14, 15, 14), ForeColor = Theme.Text, Font = Theme.Mono,
                BorderStyle = BorderStyle.None, ReadOnly = true, DetectUrls = false
            };
            DarkMode.Scrollbars(_log);
            logCard.Controls.Add(_log);
            page.Controls.Add(logCard);

            page.Controls.Add(new Panel { Dock = DockStyle.Top, Height = S(12), BackColor = Theme.Panel });

            var statusRow = new Panel { Dock = DockStyle.Top, Height = S(44), BackColor = Theme.Panel };
            _status = new Label { Text = "Ready. Choose your options, then press Apply changes.", ForeColor = Theme.Sub, BackColor = Theme.Panel, AutoSize = false, Left = 0, Top = S(10), Height = S(24) };
            _restart = new NButton { Text = "Restart PC", Width = S(130), Height = S(36), Top = S(4), BackColor = Theme.Panel, Visible = false };
            _restart.Click += (s, e) =>
            {
                if (MessageBox.Show("Restart now?", "Cub", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Process.Start(new ProcessStartInfo("shutdown.exe", "/r /t 5") { UseShellExecute = false, CreateNoWindow = true });
            };
            statusRow.Controls.Add(_status);
            statusRow.Controls.Add(_restart);
            statusRow.SizeChanged += (s, e) =>
            {
                _restart.Left = statusRow.Width - _restart.Width - S(8);
                _status.Width = Math.Max(S(100), _restart.Left - S(10));
            };
            page.Controls.Add(statusRow);

            _prog = new ThinProgress { Dock = DockStyle.Top, BackColor = Theme.Panel, Height = S(10) };
            page.Controls.Add(_prog);

            var restoreHost = new Panel { Dock = DockStyle.Top, Height = S(66), BackColor = Theme.Panel };
            var rflow = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Panel, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            restoreHost.Controls.Add(rflow);
            AddToggle(rflow, "restore", "Run", "Create a restore point first", "", true);
            rflow.SizeChanged += (s, e) => { foreach (Control c in rflow.Controls) c.Width = rflow.ClientSize.Width - S(6); };
            page.Controls.Add(restoreHost);

            page.Controls.Add(new Label { Text = "Apply", Font = Theme.Title, ForeColor = Theme.Text, BackColor = Theme.Panel, AutoSize = false, Dock = DockStyle.Top, Height = S(52) });

            _content.Controls.Add(page);
            _pages["Apply"] = page;
        }

        // =========================================================== navigation + search
        private void ShowPage(string name)
        {
            _current = name;
            foreach (var kv in _pages) kv.Value.Visible = kv.Key == name;
            foreach (NavButton nb in _nav) nb.Selected = (string)nb.Tag == name;
        }

        private void OnSearch()
        {
            string q = _search.Text.Trim().ToLowerInvariant();
            var counts = new Dictionary<string, int>();
            foreach (RowInfo r in _rows)
            {
                bool vis = q.Length == 0 || r.Text.Contains(q);
                r.Card.Visible = vis;
                if (vis) counts[r.Page] = (counts.ContainsKey(r.Page) ? counts[r.Page] : 0) + 1;
            }
            if (q.Length > 0 && !(counts.ContainsKey(_current) && counts[_current] > 0))
            {
                foreach (string n in PageNames)
                    if (counts.ContainsKey(n) && counts[n] > 0) { ShowPage(n); break; }
            }
        }

        // =========================================================== wallpaper
        private void UpdateWallpaperUi()
        {
            UpdateBackdrop();
            foreach (WallpaperTile t in _tiles)
                t.Selected = string.IsNullOrEmpty(_wallpaper) && (string)t.Tag == _wallpaperId;
            if (_wpLabel != null)
                _wpLabel.Text = string.IsNullOrEmpty(_wallpaper)
                    ? "Selected: " + WallpaperCatalog.NameOf(_wallpaperId)
                    : "Using your own image: " + _wallpaper;
        }

        private void ChooseWallpaper()
        {
            using (var dlg = new OpenFileDialog { Title = "Pick a wallpaper", Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp" })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _wallpaper = dlg.FileName;
                    if (_t.ContainsKey("wallpaper")) _t["wallpaper"].Checked = true;
                    UpdateWallpaperUi();
                }
            }
        }

        // =========================================================== winget search
        private void DoSearch()
        {
            string q = _searchBox.Text.Trim();
            if (q.Length == 0) return;
            _searchBtn.Enabled = false;
            _searchStatus.ForeColor = Theme.Sub;
            _searchStatus.Text = "Searching for \"" + q + "\"...";
            _results.Items.Clear();

            var t = new Thread(() =>
            {
                string error;
                List<WingetHit> hits = WingetSearch.Run(q, out error);
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        _searchBtn.Enabled = true;
                        if (hits == null || hits.Count == 0)
                        {
                            _searchStatus.ForeColor = Theme.Warn;
                            _searchStatus.Text = error ?? "No results.";
                            return;
                        }
                        _results.BeginUpdate();
                        foreach (WingetHit h in hits)
                        {
                            var item = new ListViewItem(h.Name);
                            item.SubItems.Add(h.Id);
                            item.SubItems.Add(h.Version);
                            _results.Items.Add(item);
                        }
                        _results.EndUpdate();
                        _searchStatus.Text = hits.Count + " result(s). Double-click or select and press Add selected.";
                    }));
                }
                catch (InvalidOperationException) { }
            });
            t.IsBackground = true;
            t.Start();
        }

        private void AddSelectedResults()
        {
            var have = new HashSet<string>(
                (_extras.Text ?? "").Split(new[] { ',', ';', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries),
                StringComparer.OrdinalIgnoreCase);
            int added = 0;
            foreach (ListViewItem item in _results.SelectedItems)
            {
                string id = item.SubItems[1].Text;
                if (have.Add(id))
                {
                    _extras.Text = string.IsNullOrWhiteSpace(_extras.Text) ? id : _extras.Text.TrimEnd().TrimEnd(',') + ", " + id;
                    added++;
                }
            }
            _searchStatus.ForeColor = added > 0 ? Theme.Good : Theme.Sub;
            _searchStatus.Text = added > 0 ? "Added " + added + " app(s) to the install list." : "Select a result first (or it is already in the list).";
        }

        // =========================================================== presets & profiles
        private void ApplyPreset(int idx)
        {
            foreach (string key in _t.Keys.ToList())
            {
                if (key == "restore" || key == "ui_translucent" || key == "ui_blur") continue;
                bool v;
                switch (idx)
                {
                    case 0: v = _def[key]; break;
                    case 1: v = (_cat[key] == "Appearance" || _cat[key] == "AI") && _def[key]; break;
                    case 2: v = true; break;
                    default: v = false; break;
                }
                _t[key].Checked = v;
            }
        }

        private Profile CaptureProfile()
        {
            var pr = new Profile();
            foreach (var kv in _t) pr.Toggles[kv.Key] = kv.Value.Checked;
            pr.CustomApps = new List<string>(_customApps);
            pr.Browser = ((BrowserChoice)_browser.SelectedItem).Id;
            pr.Wallpaper = _wallpaper;
            pr.WallpaperId = _wallpaperId;
            pr.Extras = _extras.Text;
            return pr;
        }

        private void RestoreProfile(Profile pr)
        {
            foreach (string c in pr.CustomApps) AddCustomApp(c, true);
            foreach (var kv in pr.Toggles)
                if (_t.ContainsKey(kv.Key)) _t[kv.Key].Checked = kv.Value;
            for (int i = 0; i < _browser.Items.Count; i++)
                if (((BrowserChoice)_browser.Items[i]).Id == pr.Browser) { _browser.SelectedIndex = i; break; }
            _wallpaper = pr.Wallpaper ?? "";
            if (!string.IsNullOrEmpty(pr.WallpaperId) && WallpaperCatalog.All.Any(w => w.Id == pr.WallpaperId))
                _wallpaperId = pr.WallpaperId;
            _extras.Text = pr.Extras ?? "";
            UpdateWallpaperUi();
        }

        private void SaveProfileFile(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonSerializer.Serialize(CaptureProfile(), new JsonSerializerOptions { WriteIndented = true }));
        }

        private void LoadProfileFile(string path, bool silent)
        {
            try
            {
                if (!File.Exists(path)) return;
                var pr = JsonSerializer.Deserialize<Profile>(File.ReadAllText(path));
                if (pr != null) RestoreProfile(pr);
            }
            catch (Exception e)
            {
                if (!silent) MessageBox.Show("Could not load that profile: " + e.Message, "Cub");
            }
        }

        private void SaveProfileDialog()
        {
            using (var dlg = new SaveFileDialog { Title = "Save profile", Filter = "Cub profile|*.json", FileName = "cub-profile.json" })
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try { SaveProfileFile(dlg.FileName); }
                    catch (Exception e) { MessageBox.Show("Could not save: " + e.Message, "Cub"); }
                }
        }

        private void LoadProfileDialog()
        {
            using (var dlg = new OpenFileDialog { Title = "Load profile", Filter = "Cub profile|*.json" })
                if (dlg.ShowDialog(this) == DialogResult.OK) LoadProfileFile(dlg.FileName, false);
        }

        // =========================================================== apply
        private bool On(string key) { return _t.ContainsKey(key) && _t[key].Checked; }

        private Options BuildOptions()
        {
            var o = new Options();
            foreach (var kv in _t) if (kv.Value.Checked) o.On.Add(kv.Key);

            foreach (var kv in _appPat)
                if (_t[kv.Key].Checked)
                    o.AppPatterns.AddRange(kv.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            foreach (var kv in _rtIds)
                if (_t[kv.Key].Checked)
                    o.RuntimeIds.AddRange(kv.Value);

            foreach (PinDef pin in Pins)
                if (On("pin:" + pin.Key)) o.Pins.Add(pin.AppId);

            var b = (BrowserChoice)_browser.SelectedItem;
            o.BrowserId = b.Id;
            o.BrowserName = b.Name;
            o.WallpaperPath = _wallpaper;
            o.WallpaperId = _wallpaperId;
            o.ExtraIds = _extras.Text;
            return o;
        }

        private void OnApply()
        {
            if (_running) return;
            Options o = BuildOptions();

            var warn = new StringBuilder();
            if (o.Has("bitlocker")) warn.AppendLine(" - BitLocker drives will be DECRYPTED");
            if (o.Has("sac")) warn.AppendLine(" - Smart App Control will be turned off for good");
            if (o.Has("hibernate")) warn.AppendLine(" - Hibernation and Fast Startup will be turned off");
            if (o.Has("removeapps") && o.AppPatterns.Count > 0) warn.AppendLine(" - " + o.AppPatterns.Count + " app pattern(s) will be uninstalled");
            if (o.Has("drivers")) warn.AppendLine(" - Drivers will be downloaded and installed from Windows Update (can take a while)");
            if (o.Has("runtimes") && o.RuntimeIds.Count > 0) warn.AppendLine(" - Missing runtimes will be installed");
            if (o.Has("removeedge") && !string.IsNullOrEmpty(o.BrowserId) && o.BrowserId != "Microsoft.Edge")
                warn.AppendLine(" - Microsoft Edge will be removed after " + o.BrowserName + " installs");

            string msg = "Ready to apply your selection.";
            if (warn.Length > 0) msg += "\n\nThis includes:\n" + warn;
            msg += "\nContinue?";
            if (MessageBox.Show(this, msg, "Cub", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try { SaveProfileFile(LastProfile); } catch { }

            _log.Clear();
            _restart.Visible = false;
            _prog.Value = 0;
            _running = true;
            _apply.Enabled = false;
            _search.Clear();
            ShowPage("Apply");

            var worker = new Thread(() =>
            {
                try
                {
                    new Engine(o, AppendLog, SetProgress).Run();
                    AppendLog("All done. Restart your PC to finish everything.", LogKind.Ok);
                    if (!string.IsNullOrEmpty(o.BrowserId)) AppendLog("Set your default browser in Settings > Apps > Default apps.", LogKind.Info);
                }
                catch (Exception ex)
                {
                    AppendLog("Unexpected error: " + ex.Message, LogKind.Warn);
                }
                finally
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            _running = false;
                            _apply.Enabled = true;
                            _restart.Visible = true;
                            _status.Text = "Finished. A restart is recommended.";
                        }));
                    }
                    catch (InvalidOperationException) { }
                }
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void SetProgress(double v, string text)
        {
            if (IsDisposed) return;
            try { BeginInvoke(new Action(() => { _prog.Value = v; _status.Text = text; })); }
            catch (InvalidOperationException) { }
        }

        private void AppendLog(string text, LogKind kind)
        {
            if (IsDisposed) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    Color c; string prefix;
                    switch (kind)
                    {
                        case LogKind.Step: c = Theme.Accent; prefix = "\n> "; break;
                        case LogKind.Ok:   c = Theme.Good;   prefix = "   + "; break;
                        case LogKind.Warn: c = Theme.Warn;   prefix = "   ! "; break;
                        default:           c = Theme.Sub;    prefix = "     "; break;
                    }
                    _log.SelectionStart = _log.TextLength;
                    _log.SelectionLength = 0;
                    _log.SelectionColor = c;
                    _log.AppendText(prefix + text + "\n");
                    _log.SelectionStart = _log.TextLength;
                    _log.ScrollToCaret();
                }));
            }
            catch (InvalidOperationException) { }
        }
    }
}
