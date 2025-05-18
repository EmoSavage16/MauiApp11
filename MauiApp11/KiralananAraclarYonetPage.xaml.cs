using System;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using Microsoft.Maui.Controls;

namespace MauiApp11
{
    public partial class KiralananAraclarYonetPage : ContentPage
    {
        // Model sınıfı bu dosyanın üst kısmında olmalı
        public class KiralananAracModel
        {
            public int Id { get; set; }
            public int KullaniciId { get; set; }
            public int KiralamaId { get; set; }
            public DateTime KiralamaTarihi { get; set; }
            public DateTime TeslimTarihi { get; set; }
            public decimal ToplamFiyat { get; set; }
        }

        private ObservableCollection<KiralananAracModel> kiralananAraclar;

        public KiralananAraclarYonetPage()
        {
            InitializeComponent();
            YukleKiralananAraclar();
        }

        private void YukleKiralananAraclar()
        {
            kiralananAraclar = new ObservableCollection<KiralananAracModel>();

            using (var connection = new MySqlConnection("server=localhost;database=bemaotoveritabani;user=root;password=1234"))
            {
                connection.Open();
                var query = "SELECT * FROM kiralanan_araclar";
                var cmd = new MySqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    kiralananAraclar.Add(new KiralananAracModel
                    {
                        Id = reader.GetInt32("id"),
                        KullaniciId = reader.GetInt32("kullanici_id"),
                        KiralamaId = reader.GetInt32("kiralama_id"),
                        KiralamaTarihi = reader.GetDateTime("kiralama_tarihi"),
                        TeslimTarihi = reader.GetDateTime("teslim_tarihi"),
                        ToplamFiyat = reader.GetDecimal("toplam_fiyat")
                    });
                }
            }

            AraclarCollectionView.ItemsSource = kiralananAraclar;
        }

        private async void IptalEt_Clicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.BindingContext is KiralananAracModel secilen)
            {
                bool onay = await DisplayAlert("İptal", "Bu kiralamayı iptal etmek istiyor musunuz?", "Evet", "Hayır");
                if (!onay) return;

                using (var connection = new MySqlConnection("server=localhost;database=bemaotoveritabani;user=root;password=1234"))
                {
                    connection.Open();
                    var query = "DELETE FROM kiralanan_araclar WHERE id = @id";
                    var cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", secilen.Id);
                    await cmd.ExecuteNonQueryAsync();
                }

                kiralananAraclar.Remove(secilen);
                await DisplayAlert("Başarılı", "Kiralama iptal edildi.", "Tamam");
            }
        }
    }
}
