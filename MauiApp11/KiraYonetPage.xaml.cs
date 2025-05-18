using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace MauiApp11
{
    public partial class KiraYonetPage : ContentPage
    {
        private string connectionString = "server=127.0.0.1;database=bemaotoveritabani;user=root;password=1234;";

        public KiraYonetPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadKiralamaVerileri();
        }

        public class KiralamaItem
        {
            public int KiralamaId { get; set; }
            public string Baslik { get; set; }
            public string Aciklama { get; set; }
            public string KiralikMarka { get; set; }
            public string KiralikModel { get; set; }
            public int Yil { get; set; }
            public string Vites { get; set; }
            public decimal GunlukFiyat { get; set; }
            public decimal AylikFiyat { get; set; }
            public DateTime BaslangicTarihi { get; set; }
            public DateTime BitisTarihi { get; set; }
        }

        private void LoadKiralamaVerileri()
        {
            var kiralamaListesi = new List<KiralamaItem>();

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var cmd = new MySqlCommand("SELECT kiralama_id, baslik, aciklama, kiralik_marka, kiralik_model, vites, yil, gunluk_fiyat, aylik_fiyat, baslangic_tarihi, bitis_tarihi FROM kiralama", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    kiralamaListesi.Add(new KiralamaItem
                    {
                        KiralamaId = reader.GetInt32(0),
                        Baslik = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Aciklama = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        KiralikMarka = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        KiralikModel = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Vites = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        Yil = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        GunlukFiyat = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                        AylikFiyat = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                        BaslangicTarihi = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                        BitisTarihi = reader.IsDBNull(10) ? DateTime.MinValue : reader.GetDateTime(10)
                    });
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Hata", $"Veriler yüklenirken hata oluştu: {ex.Message}", "Tamam");
            }

            KiralamaListView.ItemsSource = kiralamaListesi;
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedItem = e.SelectedItem as KiralamaItem;
            if (selectedItem != null)
            {
                DisplayAlert("Seçilen Kiralama", selectedItem.Baslik, "Tamam");
            }

            KiralamaListView.SelectedItem = null;
        }

        private async void OnKiralamaDuzenleClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var selectedKiralama = (KiralamaItem)button.BindingContext;

            await Navigation.PushAsync(new KiralamaDuzenlePage(selectedKiralama.KiralamaId));
        }

        private async void OnYeniKiralikArabaEkleClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KiralikArabaEklePage());
        }

        private async void OnKiralamaSilClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var selectedKiralama = (KiralamaItem)button.BindingContext;

            bool onay = await DisplayAlert("Onay", $"{selectedKiralama.Baslik} adlı kiralamayı silmek istiyor musunuz?", "Evet", "Hayır");
            if (!onay)
                return;

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var deleteCmd = new MySqlCommand("DELETE FROM kiralama WHERE kiralama_id = @kiralama_id", conn);
                deleteCmd.Parameters.AddWithValue("@kiralama_id", selectedKiralama.KiralamaId);

                int affectedRows = deleteCmd.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    await DisplayAlert("Başarılı", "Kiralama silindi.", "Tamam");
                    LoadKiralamaVerileri();
                }
                else
                {
                    await DisplayAlert("Hata", "Kiralama silinemedi. Lütfen tekrar deneyin.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
