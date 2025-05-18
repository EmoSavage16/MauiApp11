using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace MauiApp11
{
    public static class SessionManager
    {
        private const string KullaniciIdKey = "KullaniciId";

        public static int? KullaniciId
        {
            get
            {
                if (Preferences.ContainsKey(KullaniciIdKey))
                {
                    return Preferences.Get(KullaniciIdKey, 0);
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                    Preferences.Set(KullaniciIdKey, value.Value);
            }
        }

        public static void OturumuKapat()
        {
            if (Preferences.ContainsKey(KullaniciIdKey))
            {
                Preferences.Remove(KullaniciIdKey);
            }
        }

        public static bool OturumAktif => KullaniciId != null;
    }
}
