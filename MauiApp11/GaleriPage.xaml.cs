using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Data;

namespace MauiApp11
{
    public partial class GaleriPage : ContentPage
    {
        private bool isFilterVisible = false;
        private List<Arac> tumAraclar = new List<Arac>();

        public GaleriPage()
        {
            InitializeComponent();
            LoadAracData();
        }

        private async void LoadAracData()
        {
            string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";
            tumAraclar.Clear();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM arac_ilanlari";
                    var command = new MySqlCommand(query, connection);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var arac = new Arac
                            {
                                Id = reader.GetInt32("id"),
                                IlanNo = reader.GetInt64("ilan_no"),
                                IlanTarihi = reader.GetDateTime("ilan_tarihi"),
                                Marka = reader.GetString("marka"),
                                Model = reader.GetString("model"),
                                Yil = reader.IsDBNull("yil") ? 0 : reader.GetInt32("yil"),
                                Yakit = reader.IsDBNull("yakit") ? "" : reader.GetString("yakit"),
                                Vites = reader.IsDBNull("vites") ? "" : reader.GetString("vites"),
                                KM = reader.IsDBNull("km") ? 0 : reader.GetInt32("km"),
                                KasaTipi = reader.IsDBNull("kasa_tipi") ? "" : reader.GetString("kasa_tipi"),
                                MotorGucu = reader.IsDBNull("motor_gucu") ? "" : reader.GetString("motor_gucu"),
                                MotorHacmi = reader.IsDBNull("motor_hacmi") ? 0 : reader.GetInt32("motor_hacmi"),
                                Renk = reader.IsDBNull("renk") ? "" : reader.GetString("renk"),
                                Fiyat = reader.GetDecimal("fiyat")
                            };
                            tumAraclar.Add(arac);
                        }
                    }
                }

                AracListView.ItemsSource = tumAraclar;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Veri alınırken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        private void ToggleFilterPanel(object sender, EventArgs e)
        {
            isFilterVisible = !isFilterVisible;
            FilterPanel.IsVisible = isFilterVisible;
            FilterColumn.Width = isFilterVisible ? new GridLength(2, GridUnitType.Star) : new GridLength(0);
        }

        private void OnFiltreleClicked(object sender, EventArgs e)
        {
            string marka = MarkaEntry.Text?.ToLower() ?? "";
            string model = ModelEntry.Text?.ToLower() ?? "";
            string yakit = YakitEntry.Text?.ToLower() ?? "";
            string vites = VitesEntry.Text?.ToLower() ?? "";
            string kasa = KasaEntry.Text?.ToLower() ?? "";
            int.TryParse(YilEntry.Text, out int yil);
            int.TryParse(MinKMEntry.Text, out int minKM);
            int.TryParse(MaxKMEntry.Text, out int maxKM);
            decimal.TryParse(MinFiyatEntry.Text, out decimal minFiyat);
            decimal.TryParse(MaxFiyatEntry.Text, out decimal maxFiyat);

            var filtrelenmis = tumAraclar.FindAll(a =>
                (string.IsNullOrWhiteSpace(marka) || a.Marka.ToLower().Contains(marka)) &&
                (string.IsNullOrWhiteSpace(model) || a.Model.ToLower().Contains(model)) &&
                (string.IsNullOrWhiteSpace(yakit) || a.Yakit.ToLower().Contains(yakit)) &&
                (string.IsNullOrWhiteSpace(vites) || a.Vites.ToLower().Contains(vites)) &&
                (string.IsNullOrWhiteSpace(kasa) || a.KasaTipi.ToLower().Contains(kasa)) &&
                (yil == 0 || a.Yil == yil) &&
                (minFiyat == 0 || a.Fiyat >= minFiyat) &&
                (maxFiyat == 0 || a.Fiyat <= maxFiyat) &&
                (minKM == 0 || a.KM >= minKM) &&
                (maxKM == 0 || a.KM <= maxKM)
            );

            AracListView.ItemsSource = filtrelenmis;
        }

        private void OnTemizleClicked(object sender, EventArgs e)
        {
            MarkaEntry.Text = "";
            ModelEntry.Text = "";
            YakitEntry.Text = "";
            VitesEntry.Text = "";
            KasaEntry.Text = "";
            YilEntry.Text = "";
            MinFiyatEntry.Text = "";
            MaxFiyatEntry.Text = "";
            MinKMEntry.Text = "";
            MaxKMEntry.Text = "";
            AracListView.ItemsSource = tumAraclar;
        }

        private async void AracListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Arac secilenArac)
            {
                await Navigation.PushAsync(new AracDetayPage(secilenArac));
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }

    public class Arac
    {
        public int Id { get; set; }
        public long IlanNo { get; set; }
        public DateTime IlanTarihi { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public int Yil { get; set; }
        public string Yakit { get; set; }
        public string Vites { get; set; }
        public int KM { get; set; }
        public string KasaTipi { get; set; }
        public string MotorGucu { get; set; }
        public int MotorHacmi { get; set; }
        public string Renk { get; set; }
        public decimal Fiyat { get; set; }
    }
}
