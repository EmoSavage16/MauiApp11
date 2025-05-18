using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Linq;

namespace MauiApp11
{
    public class RentModel
    {
        public int KiralamaId { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public string Aciklama { get; set; }
        public string Vites { get; set; }
        public int Yil { get; set; }
        public decimal GunlukFiyat { get; set; }
        public decimal AylikFiyat { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }

        public string Baslik => $"{Marka} {Model}";
        public string TarihAraligi => $"{BaslangicTarihi:yyyy-MM-dd} - {BitisTarihi:yyyy-MM-dd}";
    }

    public partial class RentPage : ContentPage
    {
        public ObservableCollection<RentModel> RentList { get; set; } = new();
        private ObservableCollection<RentModel> _allRentList = new();

        public RentPage()
        {
            InitializeComponent();
            LoadRentData();
            BindingContext = this;
        }

        // Veritabanından kiralama verilerini yükler
        private async void LoadRentData()
        {
            string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string query = @"
    SELECT 
        kiralama_id,
        kiralik_marka AS Marka,
        kiralik_model AS Model,
        aciklama AS Aciklama,
        vites AS Vites,
        yil AS Yil,
        gunluk_fiyat AS GunlukFiyat,
        aylik_fiyat AS AylikFiyat,
        baslangic_tarihi,
        bitis_tarihi
    FROM kiralama;
";

                using var command = new MySqlCommand(query, connection);
                int kullaniciId = Preferences.Get("kullanici_id", 0); // kullanıcı ID 0 ise oturum açılmamış olabilir
                command.Parameters.AddWithValue("@kullaniciId", kullaniciId);


                using var reader = await command.ExecuteReaderAsync();

                _allRentList.Clear();

                while (await reader.ReadAsync())
                {
                    _allRentList.Add(new RentModel
                    {
                        KiralamaId = reader["kiralama_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["kiralama_id"]),
                        Marka = reader["Marka"] == DBNull.Value ? "" : reader["Marka"].ToString(),
                        Model = reader["Model"] == DBNull.Value ? "" : reader["Model"].ToString(),
                        Aciklama = reader["Aciklama"] == DBNull.Value ? "" : reader["Aciklama"].ToString(),
                        Vites = reader["Vites"] == DBNull.Value ? "" : reader["Vites"].ToString(),
                        Yil = reader["Yil"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Yil"]),
                        GunlukFiyat = reader["GunlukFiyat"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["GunlukFiyat"]),
                        AylikFiyat = reader["AylikFiyat"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["AylikFiyat"]),
                        BaslangicTarihi = reader["baslangic_tarihi"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["baslangic_tarihi"]),
                        BitisTarihi = reader["bitis_tarihi"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["bitis_tarihi"])
                    });
                }

                RentList.Clear();
                foreach (var item in _allRentList)
                    RentList.Add(item);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async void ToggleFilterPanel(object sender, EventArgs e)
        {
            if (FilterPanel.IsVisible)
            {
                await AnimateFilterWidth(300, 0);
                FilterPanel.IsVisible = false;
            }
            else
            {
                FilterPanel.IsVisible = true;
                await AnimateFilterWidth(0, 300);
            }
        }

        private async Task AnimateFilterWidth(double from, double to)
        {
            const uint animationDuration = 250; // ms

            var animation = new Animation(d => FilterColumn.Width = new GridLength(d), from, to);
            animation.Commit(this, "FilterAnim", length: animationDuration, easing: Easing.CubicInOut);

            await Task.Delay((int)animationDuration);
        }



        // Filtreleme işlemi
        private void FiltreleButton_Clicked(object sender, EventArgs e)
        {
            var marka = MarkaEntry.Text?.ToLower().Trim();
            var model = ModelEntry.Text?.ToLower().Trim();
            var vites = VitesPicker.SelectedItem?.ToString()?.ToLower().Trim(); var yilText = YilEntry.Text?.Trim();
            var minFiyatText = MinFiyatEntry.Text?.Trim();
            var maxFiyatText = MaxFiyatEntry.Text?.Trim();

            int.TryParse(yilText, out int yilFilter);
            decimal.TryParse(minFiyatText, out decimal minFiyat);
            decimal.TryParse(maxFiyatText, out decimal maxFiyat);

            var filtrelenmisListe = _allRentList.Where(item =>
                (string.IsNullOrEmpty(marka) || item.Marka.ToLower().Contains(marka)) &&
                (string.IsNullOrEmpty(model) || item.Model.ToLower().Contains(model)) &&
                (string.IsNullOrEmpty(vites) || item.Vites.ToLower().Contains(vites)) &&
                (yilFilter == 0 || item.Yil == yilFilter) &&
                (minFiyat == 0 || item.GunlukFiyat >= minFiyat) &&
                (maxFiyat == 0 || item.GunlukFiyat <= maxFiyat)
            ).ToList();

            RentList.Clear();
            foreach (var item in filtrelenmisListe)
                RentList.Add(item);
        }





        // Kiralama tarihleri çakışma kontrolü (veritabanı)
        private async Task<bool> KiralamaTarihleriniKontrolEt(RentModel arac, DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"
        SELECT COUNT(*) FROM kiralanan_araclar
        WHERE kiralama_id = @kiralamaId
          AND NOT (teslim_tarihi < @baslangicTarihi OR kiralama_tarihi > @bitisTarihi)
    ";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@kiralamaId", arac.KiralamaId);
            command.Parameters.AddWithValue("@baslangicTarihi", baslangicTarihi);
            command.Parameters.AddWithValue("@bitisTarihi", bitisTarihi);

            try
            {
                var result = await command.ExecuteScalarAsync();

                int count = 0;
                if (result != null && result != DBNull.Value)
                {
                    count = Convert.ToInt32(result);
                }

                return count == 0; // 0 ise çakışma yok, kiralama mümkün
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"ExecuteScalarAsync hatası: {ex.Message}", "Tamam");
                return false;
            }
        }


        // Kiralama işlemini veritabanına ekler
        private async Task KiralamaEkle(RentModel arac, DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";
            decimal toplamFiyat = HesaplaToplamFiyat(baslangicTarihi, bitisTarihi, arac.GunlukFiyat);

            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                // 1. Müsaitlik kontrolü
                string kontrolQuery = @"
            SELECT COUNT(*) FROM kiralanan_araclar
            WHERE kiralama_id = @kiralamaId
              AND NOT (teslim_tarihi < @baslangicTarihi OR kiralama_tarihi > @bitisTarihi)
        ";

                using (var kontrolCommand = new MySqlCommand(kontrolQuery, connection))
                {
                    kontrolCommand.Parameters.AddWithValue("@kiralamaId", arac.KiralamaId);
                    kontrolCommand.Parameters.AddWithValue("@baslangicTarihi", baslangicTarihi);
                    kontrolCommand.Parameters.AddWithValue("@bitisTarihi", bitisTarihi);

                    var count = Convert.ToInt32(await kontrolCommand.ExecuteScalarAsync());
                    if (count > 0)
                    {
                        await Application.Current.MainPage.DisplayAlert("Uyarı", "Bu araç seçilen tarihlerde zaten kiralanmış.", "Tamam");
                        return;
                    }
                }

                // 2. Kullanıcı ID kontrolü
                int? aktifKullaniciId = SessionManager.KullaniciId;
                if (!aktifKullaniciId.HasValue)
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Oturum açmış kullanıcı bulunamadı.", "Tamam");
                    return;
                }

                // 3. Kiralama ekleme
                string insertQuery = @"
            INSERT INTO kiralanan_araclar (kiralama_id, kullanici_id, kiralama_tarihi, teslim_tarihi, toplam_fiyat)
            VALUES (@kiralamaId, @kullaniciId, @kiralamaTarihi, @teslimTarihi, @toplamFiyat,)
        ";

                using var insertCommand = new MySqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@kiralamaId", arac.KiralamaId);
                insertCommand.Parameters.AddWithValue("@kullaniciId", aktifKullaniciId.Value);
                insertCommand.Parameters.AddWithValue("@kiralamaTarihi", baslangicTarihi);
                insertCommand.Parameters.AddWithValue("@teslimTarihi", bitisTarihi);
                insertCommand.Parameters.AddWithValue("@toplamFiyat", toplamFiyat);

                int affectedRows = await insertCommand.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    throw new Exception("Kiralama eklenemedi.");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Başarılı", "Kiralama başarıyla eklendi.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", $"Kiralama ekleme hatası: {ex.Message}", "Tamam");
            }
        }



        private decimal HesaplaToplamFiyat(DateTime baslangic, DateTime bitis, decimal gunlukFiyat)
        {
            int gunSayisi = (bitis - baslangic).Days + 1;
            return gunSayisi * gunlukFiyat;
        }

        private async void KiralamaListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                var secilenKiralama = e.CurrentSelection[0] as RentModel;

                if (secilenKiralama != null)
                {
                    await Navigation.PushAsync(new RentDetayPage(secilenKiralama));
                    ((CollectionView)sender).SelectedItem = null; // seçimi temizle
                }
            }
        }

        private void FiltreTemizleButton_Clicked(object sender, EventArgs e)
        {
            MarkaEntry.Text = "";
            ModelEntry.Text = "";
            VitesPicker.SelectedIndex = -1;
            YilEntry.Text = "";
            MinFiyatEntry.Text = "";
            MaxFiyatEntry.Text = "";

            RentList.Clear();
            foreach (var item in _allRentList)
                RentList.Add(item);
        }

        private async Task KiralamaEkle(RentModel arac, DateTime baslangicTarihi, DateTime bitisTarihi, decimal toplamFiyat)
        {
            string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                // Müsaitlik kontrolü
                string kontrolQuery = @"
            SELECT COUNT(*) FROM kiralanan_araclar
            WHERE kiralama_id = @kiralamaId              
              AND NOT (teslim_tarihi < @baslangicTarihi OR kiralama_tarihi > @bitisTarihi)
        ";

                using (var kontrolCommand = new MySqlCommand(kontrolQuery, connection))
                {
                    kontrolCommand.Parameters.AddWithValue("@kiralamaId", arac.KiralamaId);
                    kontrolCommand.Parameters.AddWithValue("@baslangicTarihi", baslangicTarihi);
                    kontrolCommand.Parameters.AddWithValue("@bitisTarihi", bitisTarihi);

                    var count = Convert.ToInt32(await kontrolCommand.ExecuteScalarAsync());
                    if (count > 0)
                    {
                        await Application.Current.MainPage.DisplayAlert("Uyarı", "Bu araç seçilen tarihlerde zaten kiralanmış.", "Tamam");
                        return;
                    }
                }

                // Kullanıcı ID kontrolü
                int? aktifKullaniciId = SessionManager.KullaniciId;
                if (!aktifKullaniciId.HasValue)
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Oturum açmış kullanıcı bulunamadı.", "Tamam");
                    return;
                }

                // Kiralama ekleme
                string insertQuery = @"
            INSERT INTO kiralanan_araclar (kiralama_id, kullanici_id, kiralama_tarihi, teslim_tarihi, toplam_fiyat)
            VALUES (@kiralamaId, @kullaniciId, @kiralamaTarihi, @teslimTarihi, @toplamFiyat)
        ";

                using var insertCommand = new MySqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@kiralamaId", arac.KiralamaId);
                insertCommand.Parameters.AddWithValue("@kullaniciId", aktifKullaniciId.Value);
                insertCommand.Parameters.AddWithValue("@kiralamaTarihi", baslangicTarihi);
                insertCommand.Parameters.AddWithValue("@teslimTarihi", bitisTarihi);
                insertCommand.Parameters.AddWithValue("@toplamFiyat", toplamFiyat);

                int affectedRows = await insertCommand.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    throw new Exception("Kiralama eklenemedi.");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Başarılı", "Kiralama başarıyla eklendi.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", $"Kiralama ekleme hatası: {ex.Message}", "Tamam");
            }
        }



        // Kirala butonuna basıldığında çağrılır
        private async void KiralaButton_Clicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.BindingContext is RentModel arac)
            {
                var tarihPopup = new TarihSecPopupPage(arac.GunlukFiyat);
                await Navigation.PushModalAsync(tarihPopup);

                var (baslangicTarihi, bitisTarihi, toplamFiyat) = await tarihPopup.WaitForDateSelectionAsync();

                if (baslangicTarihi.HasValue && bitisTarihi.HasValue)
                {
                    bool uygunMu = await KiralamaTarihleriniKontrolEt(arac, baslangicTarihi.Value, bitisTarihi.Value);
                    if (uygunMu)
                    {
                        await KiralamaEkle(arac, baslangicTarihi.Value, bitisTarihi.Value, toplamFiyat);
                        await DisplayAlert("Başarılı", $"Kiralama işlemi başarıyla gerçekleştirildi.\nToplam Tutar: ₺{toplamFiyat:N2}", "Tamam");
                    }
                    else
                    {
                        await DisplayAlert("Uyarı", "Seçtiğiniz tarihler arasında bu araç zaten kiralanmış.", "Tamam");
                    }
                }
                else
                {
                    await DisplayAlert("Uyarı", "Lütfen geçerli bir başlangıç ve bitiş tarihi seçin.", "Tamam");
                }
            }
        }

    }
}