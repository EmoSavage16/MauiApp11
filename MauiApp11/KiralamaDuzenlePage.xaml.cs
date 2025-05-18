using MySql.Data.MySqlClient;
using System;
using Microsoft.Maui.Controls;

namespace MauiApp11
{
    public partial class KiralamaDuzenlePage : ContentPage
    {
        private string connectionString = "server=127.0.0.1;database=bemaotoveritabani;user=root;password=1234;";
        private int _kiralamaId;

        public KiralamaDuzenlePage(int kiralamaId)
        {
            InitializeComponent();
            _kiralamaId = kiralamaId;
            LoadKiralamaVerisi();
        }

        private void LoadKiralamaVerisi()
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var cmd = new MySqlCommand(@"
                    SELECT * FROM kiralama WHERE kiralama_id = @kiralama_id", conn);
                cmd.Parameters.AddWithValue("@kiralama_id", _kiralamaId);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    BaslikEntry.Text = reader.GetString("baslik");
                    AciklamaEntry.Text = reader.GetString("aciklama");
                    MarkaEntry.Text = reader.GetString("kiralik_marka");
                    ModelEntry.Text = reader.GetString("kiralik_model");
                    YilEntry.Text = reader.GetInt32("yil").ToString();
                    VitesEntry.Text = reader.GetString("vites");
                    GunlukFiyatEntry.Text = reader.GetDecimal("gunluk_fiyat").ToString();
                    AylikFiyatEntry.Text = reader.GetDecimal("aylik_fiyat").ToString();

                    // Tarihler
                    if (!reader.IsDBNull(reader.GetOrdinal("baslangic_tarihi")))
                        BaslangicTarihiPicker.Date = reader.GetDateTime("baslangic_tarihi");
                    else
                        BaslangicTarihiPicker.Date = DateTime.Today;

                    if (!reader.IsDBNull(reader.GetOrdinal("bitis_tarihi")))
                        BitisTarihiPicker.Date = reader.GetDateTime("bitis_tarihi");
                    else
                        BitisTarihiPicker.Date = DateTime.Today.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Hata", $"Veri yüklenirken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        private async void OnDüzenleClicked(object sender, EventArgs e)
        {
            try
            {
                var baslik = BaslikEntry.Text;
                var aciklama = AciklamaEntry.Text;
                var marka = MarkaEntry.Text;
                var model = ModelEntry.Text;
                var yil = int.Parse(YilEntry.Text);
                var vites = VitesEntry.Text;
                var gunlukFiyat = decimal.Parse(GunlukFiyatEntry.Text);
                var aylikFiyat = decimal.Parse(AylikFiyatEntry.Text);

                var baslangicTarihi = BaslangicTarihiPicker.Date;
                var bitisTarihi = BitisTarihiPicker.Date;

                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                var cmd = new MySqlCommand(@"
                    UPDATE kiralama SET 
                        baslik = @baslik,
                        aciklama = @aciklama,
                        kiralik_marka = @marka,
                        kiralik_model = @model,
                        yil = @yil,
                        vites = @vites,
                        gunluk_fiyat = @gunluk_fiyat,
                        aylik_fiyat = @aylik_fiyat,
                        baslangic_tarihi = @baslangic_tarihi,
                        bitis_tarihi = @bitis_tarihi
                    WHERE kiralama_id = @kiralama_id", conn);

                cmd.Parameters.AddWithValue("@baslik", baslik);
                cmd.Parameters.AddWithValue("@aciklama", aciklama);
                cmd.Parameters.AddWithValue("@marka", marka);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@yil", yil);
                cmd.Parameters.AddWithValue("@vites", vites);
                cmd.Parameters.AddWithValue("@gunluk_fiyat", gunlukFiyat);
                cmd.Parameters.AddWithValue("@aylik_fiyat", aylikFiyat);
                cmd.Parameters.AddWithValue("@baslangic_tarihi", baslangicTarihi);
                cmd.Parameters.AddWithValue("@bitis_tarihi", bitisTarihi);
                cmd.Parameters.AddWithValue("@kiralama_id", _kiralamaId);

                cmd.ExecuteNonQuery();

                await DisplayAlert("Başarılı", "Kiralama bilgileri başarıyla güncellendi.", "Tamam");

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
