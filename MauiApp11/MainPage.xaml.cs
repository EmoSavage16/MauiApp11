using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MauiApp11;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void OnGirisYapClicked(object sender, EventArgs e)
    {
        // E-posta ve şifreyi al
        string email = EpostaEntry.Text;
        string sifre = SifreEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
        {
            // Eğer e-posta ya da şifre boşsa, kullanıcıyı uyar
            await DisplayAlert("Hata", "E-posta ve şifre boş olamaz.", "Tamam");
            return;
        }

        // E-posta formatını kontrol et
        if (!IsValidEmail(email))
        {
            await DisplayAlert("Hata", "Geçerli bir e-posta adresi giriniz.", "Tamam");
            return;
        }

        // Şifre formatını kontrol et
        if (!IsValidSifre(sifre))
        {
            await DisplayAlert("Hata", "Şifrenin içinde en az 1 büyük harf ve 6 karakter olmalı.", "Tamam");
            return;
        }

        // Giriş doğrulama için MySQL'e bağlanalım
        bool girisBasarili = await GirisYap(email, sifre);

        if (girisBasarili)
        {
            // Giriş başarılıysa HomePage'e yönlendir
            await DisplayAlert("Başarılı", "Giriş başarılı!", "Tamam");
            await Navigation.PushAsync(new HomePage());
        }
        else
        {
            // Giriş başarısızsa hata mesajı göster
            await DisplayAlert("Hata", "E-posta veya şifre yanlış.", "Tamam");
        }
    }

    private bool IsValidSifre(string sifre)
    {
        // Şifre formatını kontrol etmek için regex kullanıyoruz
        var sifreRegex = new Regex(@"^(?=.*[A-Z]).{6,}$"); // En az 1 büyük harf ve 6 karakter
        return sifreRegex.IsMatch(sifre);
    }

    private bool IsValidEmail(string email)
    {
        // E-posta formatını kontrol etmek için regex kullanıyoruz
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }

    private async Task<bool> GirisYap(string email, string sifre)
    {
        string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";

        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT * FROM Kullanicilar WHERE email = @email AND Sifre = @sifre";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@sifre", sifre);

                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        // Kullanıcının ID'sini al
                        int kullaniciId = Convert.ToInt32(reader["kullanici_id"]);

                        // SessionManager’a kaydet
                        SessionManager.KullaniciId = kullaniciId;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Veritabanı hatası: {ex.Message}", "Tamam");
            return false;
        }
    }


    private async void OnKayitOlClicked(object sender, EventArgs e)
    {
        // Kayıt olma işlemi için KayitOlPage'e yönlendir
        await Navigation.PushAsync(new KayitOlPage());
    }

    // Admin Giriş Butonuna Tıklanınca AdminGirisPage'e Yönlendirme
    private async void OnAdminGirisYapClicked(object sender, EventArgs e)
    {
        // Admin giriş bilgilerini kontrol et
        string email = EpostaEntry.Text;
        string sifre = SifreEntry.Text;

        // E-posta ve şifre boş olup olmadığını kontrol et
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
        {
            await DisplayAlert("Hata", "E-posta ve şifre boş olamaz.", "Tamam");
            return;
        }

        // Admin bilgilerini kontrol et (örneğin: "admin@admin.com" ve "admin123")
        if (email == "admin@admin.com" && sifre == "admin123")
        {
            // Admin giriş başarılı, admin paneline yönlendir
            await Navigation.PushAsync(new AdminPanel());
        }
        else
        {
            // Giriş başarısız, kullanıcıyı uyar
            await DisplayAlert("Hata", "Geçersiz admin bilgileri.", "Tamam");
        }
    }

}
