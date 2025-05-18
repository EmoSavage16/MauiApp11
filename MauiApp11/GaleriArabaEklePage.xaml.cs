using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.IO;

namespace MauiApp11
{
    public partial class GaleriArabaEklePage : ContentPage
    {
        private string connectionString = "server=127.0.0.1;database=bemaotoveritabani;user=root;password=1234;";
        public ObservableCollection<ImageSource> FotoListesi { get; set; } = new ObservableCollection<ImageSource>();
        private List<byte[]> FotoByteListesi = new List<byte[]>();

      

        public GaleriArabaEklePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        // Fotoğraf seç butonuna tıklayınca
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



        private async void OnArabaEkleClicked(object sender, EventArgs e)
        {
            try
            {
                var ilanNo = DateTime.Now.Ticks;

                var marka = MarkaEntry.Text;
                var model = ModelEntry.Text;
                var yil = int.Parse(YilEntry.Text);
                var yakit = YakitEntry.Text;
                var vites = VitesEntry.Text;
                var km = int.Parse(KMEntry.Text);
                var kasaTipi = KasaTipiEntry.Text;
                var motorGucu = MotorGucuEntry.Text;
                var motorHacmi = int.Parse(MotorHacmiEntry.Text);
                var renk = RenkEntry.Text;
                var fiyat = decimal.Parse(FiyatEntry.Text);

                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                using var transaction = await conn.BeginTransactionAsync();

                try
                {
                    var cmd = new MySqlCommand(@"
                        INSERT INTO arac_ilanlari 
                        (ilan_no, ilan_tarihi, marka, model, yil, yakit, vites, km, kasa_tipi, motor_gucu, motor_hacmi, renk, fiyat)
                        VALUES
                        (@ilan_no, @ilan_tarihi, @marka, @model, @yil, @yakit, @vites, @km, @kasa_tipi, @motor_gucu, @motor_hacmi, @renk, @fiyat)", conn, transaction);

                    cmd.Parameters.AddWithValue("@ilan_no", ilanNo);
                    cmd.Parameters.AddWithValue("@ilan_tarihi", DateTime.Now);
                    cmd.Parameters.AddWithValue("@marka", marka);
                    cmd.Parameters.AddWithValue("@model", model);
                    cmd.Parameters.AddWithValue("@yil", yil);
                    cmd.Parameters.AddWithValue("@yakit", yakit);
                    cmd.Parameters.AddWithValue("@vites", vites);
                    cmd.Parameters.AddWithValue("@km", km);
                    cmd.Parameters.AddWithValue("@kasa_tipi", kasaTipi);
                    cmd.Parameters.AddWithValue("@motor_gucu", motorGucu);
                    cmd.Parameters.AddWithValue("@motor_hacmi", motorHacmi);
                    cmd.Parameters.AddWithValue("@renk", renk);
                    cmd.Parameters.AddWithValue("@fiyat", fiyat);

                    await cmd.ExecuteNonQueryAsync();

                    // Fotoğrafları kaydet
                    foreach (var foto in FotoListesi)
                    {
                        byte[] fotoBytes = await ImageSourceToBytes(foto);
                        var fotoCmd = new MySqlCommand(@"
                            INSERT INTO arac_ilanlari_foto (ilan_no, foto)
                            VALUES (@ilan_no, @foto)", conn, transaction);

                        fotoCmd.Parameters.AddWithValue("@ilan_no", ilanNo);
                        fotoCmd.Parameters.AddWithValue("@foto", fotoBytes);

                        await fotoCmd.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();

                    await DisplayAlert("Başarılı", "Yeni araba ve fotoğraflar başarıyla eklendi.", "Tamam");
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

        // ImageSource'u byte[]'e dönüştürür
        private async Task<byte[]> ImageSourceToBytes(ImageSource source)
        {
            if (source is StreamImageSource streamImageSource)
            {
                var stream = await streamImageSource.Stream(CancellationToken.None);
                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
            return null;
        }
    }
}
