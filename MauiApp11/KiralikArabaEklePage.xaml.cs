using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.IO;

namespace MauiApp11;

public partial class KiralikArabaEklePage : ContentPage
{
    private string connectionString = "server=127.0.0.1;database=bemaotoveritabani;user=root;password=1234;";

    public ObservableCollection<ImageSource> FotoListesi { get; set; } = new ObservableCollection<ImageSource>();
    private List<byte[]> FotoByteListesi = new List<byte[]>();

    public KiralikArabaEklePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnFotoSecClicked(object sender, EventArgs e)
    {
        try
        {
            var results = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Fotoğrafları seçin",
                FileTypes = FilePickerFileType.Images
            });

            if (results != null)
            {
                foreach (var result in results)
                {
                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var bytes = ms.ToArray();

                    FotoByteListesi.Add(bytes);
                    FotoListesi.Add(ImageSource.FromStream(() => new MemoryStream(bytes)));
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Fotoğraf seçilirken hata: {ex.Message}", "Tamam");
        }
    }

    private async void OnKiralikArabaEkleClicked(object sender, EventArgs e)
    {
        try
        {
            string baslik = BaslikEntry.Text;
            string aciklama = AciklamaEntry.Text;
            string marka = MarkaEntry.Text;
            string model = ModelEntry.Text;
            int yil = int.Parse(YilEntry.Text);
            string vites = VitesEntry.Text;
            decimal gunlukFiyat = decimal.Parse(GunlukFiyatEntry.Text);
            decimal aylikFiyat = decimal.Parse(AylikFiyatEntry.Text);
            DateTime baslangicTarihi = BaslangicTarihiPicker.Date;
            DateTime bitisTarihi = BitisTarihiPicker.Date;

            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // 1. Kiralama bilgilerini ekle
                var cmd = new MySqlCommand(@"
                    INSERT INTO kiralama
                    (baslik, aciklama, kiralik_marka, kiralik_model, yil, vites, gunluk_fiyat, aylik_fiyat, baslangic_tarihi, bitis_tarihi)
                    VALUES
                    (@baslik, @aciklama, @marka, @model, @yil, @vites, @gunluk_fiyat, @aylik_fiyat, @baslangic_tarihi, @bitis_tarihi);
                    SELECT LAST_INSERT_ID();", conn, transaction);

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

                int kiralamaId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // 2. Fotoğrafları kaydet (eğer varsa)
                foreach (var fotoBytes in FotoByteListesi)
                {
                    var fotoCmd = new MySqlCommand(@"
                        INSERT INTO kiralama_foto (kiralama_id, foto_data)
                        VALUES (@kiralama_id, @foto_data)", conn, transaction);

                    fotoCmd.Parameters.AddWithValue("@kiralama_id", kiralamaId);
                    fotoCmd.Parameters.AddWithValue("@foto_data", fotoBytes);

                    await fotoCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                await DisplayAlert("Başarılı", "Kiralık araba ve fotoğraflar başarıyla eklendi.", "Tamam");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await DisplayAlert("Hata", "Veritabanı işlemi başarısız: " + ex.Message, "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Bir hata oluştu: " + ex.Message, "Tamam");
        }
    }
}
