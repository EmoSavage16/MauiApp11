using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace MauiApp11;

public partial class CalisanlarPage : ContentPage
{
    private ObservableCollection<Calisan> calisanListesi = new();
    private Calisan seciliCalisan;
    private string connectionString = "server=127.0.0.1;database=bemaotoveritabani;user=root;password=1234;";

    public CalisanlarPage()
    {
        InitializeComponent();
        CalisanlariGetir();
    }

    private void CalisanlariGetir()
    {
        calisanListesi.Clear();

        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        var cmd = new MySqlCommand("SELECT * FROM calisanlar", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            calisanListesi.Add(new Calisan
            {
                CalisanId = reader.GetInt32("calisan_id"),
                Ad = reader.GetString("ad"),
                Soyad = reader.GetString("soyad"),
                Pozisyon = reader["pozisyon"]?.ToString(),
                Email = reader["email"]?.ToString(),
                Telefon = reader["telefon"]?.ToString(),
                Maas = reader["maas"] != DBNull.Value ? reader.GetDecimal("maas") : 0
            });
        }

        CalisanlarCollectionView.ItemsSource = calisanListesi;
    }

    private void OnCalisanSelected(object sender, SelectionChangedEventArgs e)
    {
        seciliCalisan = (Calisan)e.CurrentSelection.FirstOrDefault();
        if (seciliCalisan == null) return;

        AdEntry.Text = seciliCalisan.Ad;
        SoyadEntry.Text = seciliCalisan.Soyad;
        PozisyonEntry.Text = seciliCalisan.Pozisyon;
        EmailEntry.Text = seciliCalisan.Email;
        TelefonEntry.Text = seciliCalisan.Telefon;
        MaasEntry.Text = seciliCalisan.Maas.ToString();
    }

    private void OnGuncelleClicked(object sender, EventArgs e)
    {
        if (seciliCalisan == null)
        {
            DisplayAlert("Hata", "Güncellenecek çalışan seçilmedi.", "Tamam");
            return;
        }

        try
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            var cmd = new MySqlCommand("UPDATE calisanlar SET ad=@ad, soyad=@soyad, pozisyon=@pozisyon, email=@email, telefon=@telefon, maas=@maas WHERE calisan_id=@id", conn);
            cmd.Parameters.AddWithValue("@ad", AdEntry.Text);
            cmd.Parameters.AddWithValue("@soyad", SoyadEntry.Text);
            cmd.Parameters.AddWithValue("@pozisyon", PozisyonEntry.Text);
            cmd.Parameters.AddWithValue("@email", EmailEntry.Text);
            cmd.Parameters.AddWithValue("@telefon", TelefonEntry.Text);
            cmd.Parameters.AddWithValue("@maas", decimal.Parse(MaasEntry.Text));
            cmd.Parameters.AddWithValue("@id", seciliCalisan.CalisanId);

            cmd.ExecuteNonQuery();

            DisplayAlert("Başarılı", "Çalışan bilgisi güncellendi.", "Tamam");
            CalisanlariGetir();
        }
        catch (Exception ex)
        {
            DisplayAlert("Hata", $"Güncelleme başarısız: {ex.Message}", "Tamam");
        }
    }


    // Yeni Çalışan Ekleme İşlemi
    private void OnEkleClicked(object sender, EventArgs e)
    {
        var ad = AdEntry.Text;
        var soyad = SoyadEntry.Text;
        var pozisyon = PozisyonEntry.Text;
        var email = EmailEntry.Text;
        var telefon = TelefonEntry.Text;
        var maas = decimal.Parse(MaasEntry.Text);

        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        var cmd = new MySqlCommand("INSERT INTO calisanlar (ad, soyad, pozisyon, email, telefon, maas) VALUES (@ad, @soyad, @pozisyon, @email, @telefon, @maas)", conn);
        cmd.Parameters.AddWithValue("@ad", ad);
        cmd.Parameters.AddWithValue("@soyad", soyad);
        cmd.Parameters.AddWithValue("@pozisyon", pozisyon);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@telefon", telefon);
        cmd.Parameters.AddWithValue("@maas", maas);

        cmd.ExecuteNonQuery();

        DisplayAlert("Başarılı", "Yeni çalışan eklendi.", "Tamam");
        CalisanlariGetir();
    }

    private async void OnSilClicked(object sender, EventArgs e)
    {
        if (seciliCalisan == null)
        {
            await DisplayAlert("Hata", "Silinecek çalışan seçilmedi.", "Tamam");
            return;
        }

        var confirm = await DisplayAlert("Onay", $"{seciliCalisan.Ad} {seciliCalisan.Soyad} adlı çalışanı silmek istediğinizden emin misiniz?", "Evet", "Hayır");

        if (confirm)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            var cmd = new MySqlCommand("DELETE FROM calisanlar WHERE calisan_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", seciliCalisan.CalisanId);
            cmd.ExecuteNonQuery();

            DisplayAlert("Başarılı", "Çalışan başarıyla silindi.", "Tamam");
            CalisanlariGetir(); // Listeyi güncelle
        }
    }

    // Calisan sınıfı burada tanımlandı
    public class Calisan
    {
        public int CalisanId { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Pozisyon { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public decimal Maas { get; set; }
    }
}
