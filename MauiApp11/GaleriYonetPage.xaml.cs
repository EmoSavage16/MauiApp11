using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace MauiApp11
{
    public partial class GaleriYonetPage : ContentPage
    {
        private string connectionString = "server=127.0.0.1;database=bemaotoveritabani;user=root;password=1234;";
        private List<ArabaItem> arabaListesi = new();

        public GaleriYonetPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAraclarAsync();
        }

        private async Task LoadAraclarAsync()
        {
            arabaListesi.Clear();

            try
            {
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT id, marka, model, yil, yakit, fiyat FROM arac_ilanlari", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    arabaListesi.Add(new ArabaItem
                    {
                        Id = reader.GetInt32(0),
                        Marka = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Model = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Yil = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        Yakit = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Fiyat = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5)
                    });
                }

                // UI güncellemesi ana iş parçacığında yapılmalı
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    AracListView.ItemsSource = null;
                    AracListView.ItemsSource = arabaListesi;
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Araçlar yüklenirken hata oluştu:\n{ex.Message}", "Tamam");
            }
        }

        private async void OnYeniAracEkleClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GaleriArabaEklePage());
        }

        private async void OnAracDuzenleClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ArabaItem secilenAraba)
            {
                await Navigation.PushAsync(new GaleriDuzenlemePage(secilenAraba.Id));
            }
        }

        private async void OnAracSilClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ArabaItem secilenAraba)
            {
                bool onay = await DisplayAlert("Onay", $"{secilenAraba.Marka} {secilenAraba.Model} adlı aracı silmek istiyor musunuz?", "Evet", "Hayır");
                if (!onay)
                    return;

                try
                {
                    using var conn = new MySqlConnection(connectionString);
                    await conn.OpenAsync();

                    // Transaction açalım ki hepsi birlikte silinsin
                    using var transaction = await conn.BeginTransactionAsync();

                    // Önce fotoğrafları sil
                    var deleteFotosCmd = new MySqlCommand("DELETE FROM arac_ilanlari_foto WHERE ilan_no = @ilan_no", conn, transaction);
                    deleteFotosCmd.Parameters.AddWithValue("@ilan_no", secilenAraba.IlanNo);
                    await deleteFotosCmd.ExecuteNonQueryAsync();

                    // Sonra aracı sil
                    var deleteCmd = new MySqlCommand("DELETE FROM arac_ilanlari WHERE id = @id", conn, transaction);
                    deleteCmd.Parameters.AddWithValue("@id", secilenAraba.Id);
                    await deleteCmd.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();

                    await DisplayAlert("Başarılı", "Araç ve fotoğrafları başarıyla silindi.", "Tamam");
                    await LoadAraclarAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Hata", $"Silme işlemi sırasında hata oluştu:\n{ex.Message}", "Tamam");
                }
            }
        }


        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var secilenAraba = e.SelectedItem as ArabaItem;
            if (secilenAraba != null)
            {
                DisplayAlert("Seçilen Araç", $"{secilenAraba.Marka} {secilenAraba.Model}", "Tamam");
            }
            AracListView.SelectedItem = null; // Seçimi temizle
        }
    }

    public class ArabaItem
    {
        public int Id { get; set; }
        public string Marka { get; set; }
        public long IlanNo { get; set; }
        public string Model { get; set; }
        public int Yil { get; set; }
        public string Yakit { get; set; }
        public decimal Fiyat { get; set; }
    }
}
