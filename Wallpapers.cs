using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Cub
{
    internal sealed class WallpaperDef
    {
        public string Id;
        public string Name;
        public WallpaperDef(string id, string name) { Id = id; Name = name; }
    }

    /// <summary>The built-in wallpapers. Images are embedded in the exe as "wp-<id>.jpg" (full size) and "th-<id>.jpg" (thumbnail).</summary>
    internal static class WallpaperCatalog
    {
        public const string DefaultId = "midnight-bloom";

        public static readonly WallpaperDef[] All =
        {
            new WallpaperDef("dusk-valley", "Dusk Valley"),
            new WallpaperDef("moonlit-hills", "Moonlit Hills"),
            new WallpaperDef("night-valley", "Night Valley"),
            new WallpaperDef("cobalt-bloom", "Cobalt Bloom"),
            new WallpaperDef("emerald-bloom", "Emerald Bloom"),
            new WallpaperDef("crimson-bloom", "Crimson Bloom"),
            new WallpaperDef("coral-dark-bloom", "Coral Dark Bloom"),
            new WallpaperDef("midnight-bloom", "Midnight Bloom"),
            new WallpaperDef("amber-dark-bloom", "Amber Dark Bloom"),
            new WallpaperDef("azure-dark-bloom", "Azure Dark Bloom"),
            new WallpaperDef("frost-bloom", "Frost Bloom"),
        };

        public static string NameOf(string id)
        {
            foreach (WallpaperDef d in All) if (d.Id == id) return d.Name;
            return id;
        }

        public static Stream OpenFull(string id)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("wp-" + id + ".jpg");
        }

        public static Image LoadThumb(string id)
        {
            try
            {
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("th-" + id + ".jpg"))
                {
                    if (s == null) return null;
                    var ms = new MemoryStream();
                    s.CopyTo(ms);
                    ms.Position = 0;
                    return new Bitmap(ms);   // the bitmap keeps using the stream, so it is not disposed
                }
            }
            catch { return null; }
        }
    }
}
