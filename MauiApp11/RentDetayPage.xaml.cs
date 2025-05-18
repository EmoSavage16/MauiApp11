using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MauiApp11
{
    public partial class RentDetayPage : ContentPage
    {
        private RentModel _seciliRent;
        private const string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";

        public RentDetayPage(RentModel seciliRent)
        {
            InitializeComponent();
            _seciliRent = seciliRent;
            BilgileriGoster();
            LoadFotos();
        }

        private void BilgileriGoster()
        {
            BaslikLabel.Text = _seciliRent.Baslik;
            AciklamaLabel.Text = _seciliRent.Aciklama;
            VitesLabel.Text = _seciliRent.Vites;
            YilLabel.Text = _seciliRent.Yil.ToString();
            GunlukFiyatLabel.Text = $"₺{_seciliRent.GunlukFiyat:N0}";
            AylikFiyatLabel.Text = $"₺{_seciliRent.AylikFiyat:N0}";
            BaslangicLabel.Text = _seciliRent.BaslangicTarihi.ToString("yyyy-MM-dd");
            BitisLabel.Text = _seciliRent.BitisTarihi.ToString("yyyy-MM-dd");
            TarihAraligiLabel.Text = _seciliRent.TarihAraligi;
        }

        private async void LoadFotos()
        {
            try
            {
                var fotolar = await LoadKiralamaFotolarAsync(_seciliRent.KiralamaId);
                FotoCarousel.ItemsSource = fotolar;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Fotoğraflar yüklenirken hata oluştu: {ex.Message}", "Tamam");
            }
        }
        private void OnOncekiClicked(object sender, EventArgs e)
        {
            if (FotoCarousel.ItemsSource == null) return;

            int yeniIndex = FotoCarousel.Position - 1;
            if (yeniIndex >= 0)
                FotoCarousel.Position = yeniIndex;
        }

        private void OnSonrakiClicked(object sender, EventArgs e)
        {
            if (FotoCarousel.ItemsSource == null) return;

            int yeniIndex = FotoCarousel.Position + 1;
            int maxIndex = ((List<ImageSource>)FotoCarousel.ItemsSource).Count - 1;

            if (yeniIndex <= maxIndex)
                FotoCarousel.Position = yeniIndex;
        }
    
        private async Task<List<ImageSource>> LoadKiralamaFotolarAsync(int kiralamaId)
        {
            var fotolar = new List<ImageSource>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT foto_data FROM kiralama_foto WHERE kiralama_id = @kiralamaId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@kiralamaId", kiralamaId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(0))
                {
                    byte[] imageBytes = (byte[])reader["foto_data"];
                    fotolar.Add(ImageSource.FromStream(() => new MemoryStream(imageBytes)));
                }
            }

            if (fotolar.Count == 0)
                fotolar.Add("noimage.png"); // Projende placeholder görsel olduğundan emin ol

            return fotolar;
        }
    }
}
