using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace MauiApp11;

public partial class KayitOlPage : ContentPage
{
    public KayitOlPage()
    {
        InitializeComponent();
    }

    private async void OnKayitButtonClicked(object sender, EventArgs e)
    {
        string ad = AdEntry.Text;
        string soyad = SoyadEntry.Text;
        string eposta = EpostaEntry.Text;
        string sifre = SifreEntry.Text;

        if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) ||
            string.IsNullOrWhiteSpace(eposta) || string.IsNullOrWhiteSpace(sifre))
        {
            await DisplayAlert("Hata", "Lütfen tüm alanları doldurun.", "Tamam");
            return;
        }

        if (!IsValidEmail(eposta))
        {
            await DisplayAlert("Hata", "Geçerli bir e-posta adresi giriniz.", "Tamam");
            return;
        }

        if (!IsValidSifre(sifre))
        {
            await DisplayAlert("Hata", "Şifrede en az 1 büyük harf ve toplamda en az 6 karakter olmalı.", "Tamam");
            return;
        }

        if (await IsEmailAlreadyRegistered(eposta))
        {
            await DisplayAlert("Hata", "Bu e-posta adresi zaten kayıtlı.", "Tamam");
            return;
        }

        string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "INSERT INTO kullanicilar (ad, soyad, email, sifre) VALUES (@ad, @soyad, @eposta, @sifre)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ad", ad);
            command.Parameters.AddWithValue("@soyad", soyad);
            command.Parameters.AddWithValue("@eposta", eposta);
            command.Parameters.AddWithValue("@sifre", sifre);

            int result = await command.ExecuteNonQueryAsync();

            if (result > 0)
            {
                await DisplayAlert("Başarılı", "Kayıt başarıyla tamamlandı!", "Tamam");
                await Navigation.PopAsync(); // Giriş ekranına dön
            }
            else
            {
                await DisplayAlert("Hata", "Kayıt yapılamadı.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Veritabanı hatası: {ex.Message}", "Tamam");
        }
    }
    private async void OnGirisYapTapped(object sender, EventArgs e)
    {
        // MainPage giriş sayfasına yönlendirme
        await Navigation.PopAsync(); // Eğer Kayıt sayfası MainPage'den push edilmişse, pop ile dönebiliriz.

        // Eğer Navigation.PopAsync() uygun değilse:
        // await Navigation.PushAsync(new MainPage());
    }



    private bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }

    private bool IsValidSifre(string sifre)
    {
        var sifreRegex = new Regex(@"^(?=.*[A-Z]).{6,}$"); // En az 1 büyük harf ve 6 karakter
        return sifreRegex.IsMatch(sifre);
    }



    private async Task<bool> IsEmailAlreadyRegistered(string eposta)
    {
        string connectionString = "Server=localhost;Database=bemaotoveritabani;User ID=root;Password=1234;";

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT COUNT(*) FROM kullanicilar WHERE email = @eposta";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@eposta", eposta);

            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Veritabanı hatası: {ex.Message}", "Tamam");
            return false;
        }
    }
}
