using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace MauiApp11
{
    public static class Durumlar
    {
        public static List<string> DurumSecenekleri { get; } = new()
        {
            "Beklemede", "İşlemde", "Tamamlandı"
        };
    }
    public partial class TemizlemeYonet : ContentPage
    {
        public ObservableCollection<TemizlikIslem> TemizlikListesi { get; set; } = new();
        private string connectionString = "server=localhost;database=bemaotoveritabani;uid=root;pwd=1234";

        public TemizlemeYonet()
        {
            InitializeComponent();
            BindingContext = this;
            LoadTemizlikFromDatabase();
        }

        // Durumu Picker ile güncelle
        public async void OnDurumGuncellePicker(object sender, EventArgs e)
        {
            var button = sender as Button;
            var islem = button?.BindingContext as TemizlikIslem;

            if (islem != null)
            {
                try
                {
                    using var connection = new MySqlConnection(connectionString);
                    connection.Open();

                    string query = "UPDATE temizlik SET durum = @Durum WHERE temizlik_id = @TemizlikId";

                    using var cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Durum", islem.Durum);
                    cmd.Parameters.AddWithValue("@TemizlikId", islem.TemizlikId);
                    cmd.ExecuteNonQuery();

                    await DisplayAlert("Başarılı", "Durum başarıyla güncellendi.", "Tamam");
                    LoadTemizlikFromDatabase();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Hata", $"Veritabanı hatası: {ex.Message}", "Tamam");
                }
            }
        }

        // İşlemi sil
        public async void OnIslemSil(object sender, EventArgs e)
        {
            var button = sender as Button;
            var islem = button?.BindingContext as TemizlikIslem;

            if (islem != null)
            {
                try
                {
                    using var connection = new MySqlConnection(connectionString);
                    connection.Open();

                    string query = "DELETE FROM temizlik WHERE temizlik_id = @TemizlikId";

                    using var cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@TemizlikId", islem.TemizlikId);
                    cmd.ExecuteNonQuery();

                    LoadTemizlikFromDatabase();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Hata", $"Veritabanı hatası: {ex.Message}", "Tamam");
                }
            }
        }

        // Veritabanından işlemleri yükle
        private void LoadTemizlikFromDatabase()
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string query = "SELECT temizlik_id, islem_adi, arac_turu, fiyat, sure, kategori, durum, plaka FROM temizlik ORDER BY durum DESC, temizlik_id DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                TemizlikListesi.Clear();
                while (reader.Read())
                {
                    TemizlikListesi.Add(new TemizlikIslem
                    {
                        TemizlikId = reader.GetInt32(0),
                        IslemAdi = reader.GetString(1),
                        AracTuru = reader.GetString(2),
                        Fiyat = reader.GetDecimal(3),
                        Sure = reader.GetInt32(4),
                        Kategori = reader.GetString(5),
                        Durum = reader.GetString(6),
                        Plaka = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
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