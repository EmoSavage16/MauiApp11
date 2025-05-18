using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace MauiApp11
{
    public class TemizlikIslem
    {
        public int TemizlikId { get; set; }
        public string IslemAdi { get; set; }
        public string AracTuru { get; set; }
        public decimal Fiyat { get; set; }
        public int Sure { get; set; }
        public string Kategori { get; set; }
        public string Durum { get; set; }
        public string Plaka { get; set; }
    }

    public partial class Temizlik : ContentPage
    {
        public ObservableCollection<TemizlikIslem> TemizlikListesi { get; set; } = new();
        public ObservableCollection<string> AracTurleri { get; set; } = new()
        {
            "SUV", "Sedan", "Hatchback", "MPV", "Crossover",  // İç Dış Yıkama eklendi
        };

        public string Plaka { get; set; }
        public string IslemAdi { get; set; }
        public string AracTuru { get; set; }
        public string KategoriSecim { get; set; }
        public decimal GosterFiyat { get; set; }
        public int GosterSure { get; set; }

        private string connectionString = "server=localhost;database=bemaotoveritabani;uid=root;pwd=1234";

        public Temizlik()
        {
            InitializeComponent();
            BindingContext = this;
            LoadTemizlikFromDatabase();
        }

        // Kategori değiştiğinde fiyat ve süreyi güncelle
        private void OnKategoriChanged(object sender, EventArgs e)
        {
            if (KategoriSecim != null)
            {
                GosterFiyat = HesaplaFiyat(KategoriSecim, AracTuru);
                GosterSure = HesaplaSure(KategoriSecim);
            }
        }

        private Command _ekleCommand;
        public Command EkleCommand => _ekleCommand ??= new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Plaka) || string.IsNullOrWhiteSpace(IslemAdi) ||
                string.IsNullOrWhiteSpace(AracTuru) || string.IsNullOrWhiteSpace(KategoriSecim))
            {
                await DisplayAlert("Hata", "Lütfen tüm alanları doldurun.", "Tamam");
                return;
            }

            decimal fiyat = HesaplaFiyat(KategoriSecim, AracTuru);
            int sure = HesaplaSure(KategoriSecim);

            TemizlikIslem yeni = new()
            {
                IslemAdi = IslemAdi,
                AracTuru = AracTuru,
                Fiyat = fiyat,
                Sure = sure,
                Kategori = KategoriSecim,
                Durum = "Beklemede",
                Plaka = Plaka  // Plaka bilgisini ekliyoruz
            };

            AddTemizlikToDatabase(yeni);
            TemizlikListesi.Add(yeni);

            // Form temizle
            Plaka = IslemAdi = AracTuru = KategoriSecim = null;
            GosterFiyat = 0;
            GosterSure = 0;
            OnPropertyChanged(nameof(Plaka));
            OnPropertyChanged(nameof(IslemAdi));
            OnPropertyChanged(nameof(AracTuru));
            OnPropertyChanged(nameof(KategoriSecim));
            OnPropertyChanged(nameof(GosterFiyat));
            OnPropertyChanged(nameof(GosterSure));
        });

        private decimal HesaplaFiyat(string kategori, string aracTuru)
        {
            if (kategori == "Dış Temizlik")
                return aracTuru == "SUV" ? 250 : 200;
            if (kategori == "İç Temizlik")
                return aracTuru == "SUV" ? 300 : 250;
            if (kategori == "Cam Filmi")
                return 500;
            if (kategori == "PPF")
                return 600;
            if (kategori == "İç Dış Yıkama") // İç Dış Yıkama fiyatı
                return aracTuru == "SUV" ? 350 : 300;
            return 0;
        }

        private int HesaplaSure(string kategori)
        {
            return kategori switch
            {
                "Dış Temizlik" => 30,
                "İç Temizlik" => 45,
                "Cam Filmi" => 60,
                "PPF" => 90,
                "İç Dış Yıkama" => 50,  // İç Dış Yıkama süresi
                _ => 30
            };
        }

        private void AddTemizlikToDatabase(TemizlikIslem islem)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "INSERT INTO temizlik (islem_adi, arac_turu, fiyat, sure, kategori, durum, plaka) " +
                               "VALUES (@IslemAdi, @AracTuru, @Fiyat, @Sure, @Kategori, @Durum, @Plaka)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IslemAdi", islem.IslemAdi);
                cmd.Parameters.AddWithValue("@AracTuru", islem.AracTuru);
                cmd.Parameters.AddWithValue("@Fiyat", islem.Fiyat);
                cmd.Parameters.AddWithValue("@Sure", islem.Sure);
                cmd.Parameters.AddWithValue("@Kategori", islem.Kategori);
                cmd.Parameters.AddWithValue("@Durum", islem.Durum);
                cmd.Parameters.AddWithValue("@Plaka", islem.Plaka);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                DisplayAlert("Hata", $"Veritabanı hatası: {ex.Message}", "Tamam");
            }
        }

        private void LoadTemizlikFromDatabase()
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "SELECT islem_adi, arac_turu, fiyat, sure, kategori, durum, plaka FROM temizlik ORDER BY temizlik_id DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                TemizlikListesi.Clear();
                while (reader.Read())
                {
                    TemizlikListesi.Add(new TemizlikIslem
                    {
                        IslemAdi = reader.GetString(0),
                        AracTuru = reader.GetString(1),
                        Fiyat = reader.GetDecimal(2),
                        Sure = reader.GetInt32(3),
                        Kategori = reader.GetString(4),
                        Durum = reader.GetString(5),
                        Plaka = reader.IsDBNull(6) ? string.Empty : reader.GetString(6) // Null kontrolü
                    });
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Hata", $"Veritabanı hatası: {ex.Message}", "Tamam");
            }
        }
    }
}