using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;

namespace MauiApp11
{
    public partial class AracDetayPage : ContentPage
    {
        private const string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";
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

        public AracDetayPage(Arac secilenArac)
        {
            InitializeComponent();

            MarkaLabel.Text = $"Marka: {secilenArac.Marka}";
            ModelLabel.Text = $"Model: {secilenArac.Model}";
            YilLabel.Text = $"Yıl: {secilenArac.Yil}";
            YakitLabel.Text = $"Yakıt: {secilenArac.Yakit}";
            VitesLabel.Text = $"Vites: {secilenArac.Vites}";
            KasaLabel.Text = $"Kasa Tipi: {secilenArac.KasaTipi}";
            MotorGucuLabel.Text = $"Motor Gücü: {secilenArac.MotorGucu}";
            MotorHacmiLabel.Text = $"Motor Hacmi: {secilenArac.MotorHacmi} cc";
            RenkLabel.Text = $"Renk: {secilenArac.Renk}";
            KMLabel.Text = $"KM: {secilenArac.KM:N0}";
            FiyatLabel.Text = $"Fiyat: ₺{secilenArac.Fiyat:N0}";

            // Fotoğrafları yükle
            LoadAracFotolar(secilenArac.IlanNo);
        }

        private async void LoadAracFotolar(long ilanNo)
        {
            
            try
            {
                var fotolar = new List<ImageSource>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT foto FROM arac_ilanlari_foto WHERE ilan_no = @ilanNo";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ilanNo", ilanNo);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    byte[] imageBytes = (byte[])reader["foto"];
                                    var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                                    fotolar.Add(imageSource);
                                }
                            }
                        }
                    }
                }

                // Eğer hiç foto yoksa placeholder
                if (fotolar.Count == 0)
                {
                    fotolar.Add("noimage.png");
                }

                FotoCarousel.ItemsSource = fotolar;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Fotoğraflar yüklenirken hata: {ex.Message}", "Tamam");
            }
        }
    }
}
