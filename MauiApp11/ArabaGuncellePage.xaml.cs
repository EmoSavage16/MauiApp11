using MySql.Data.MySqlClient;
using System;

namespace MauiApp11;

public partial class ArabaGuncellePage : ContentPage
{
    private int arabaId;
    private string connectionString = "server=127.0.0.1;database=bemaotoveritabani;user=root;password=1234;";

    public ArabaGuncellePage(int id)
    {
        InitializeComponent();
        arabaId = id;
        YukuArabaBilgileri();
    }

    private void YukuArabaBilgileri()
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        var cmd = new MySqlCommand("SELECT * FROM arac_ilanlari WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", arabaId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            MarkaEntry.Text = reader["marka"].ToString();
            ModelEntry.Text = reader["model"].ToString();
            YilEntry.Text = reader["yil"].ToString();
            YakitEntry.Text = reader["yakit"].ToString();
            VitesEntry.Text = reader["vites"].ToString();
            KMEntry.Text = reader["km"].ToString();
            KasaTipiEntry.Text = reader["kasa_tipi"].ToString();
            MotorGucuEntry.Text = reader["motor_gucu"].ToString();
            MotorHacmiEntry.Text = reader["motor_hacmi"].ToString();
            RenkEntry.Text = reader["renk"].ToString();
            FiyatEntry.Text = reader["fiyat"].ToString();
        }
    }

    private async void OnKaydetClicked(object sender, EventArgs e)
    {
        try
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"UPDATE arac_ilanlari 
                                         SET marka=@marka, model=@model, yil=@yil, yakit=@yakit, 
                                             vites=@vites, km=@km, kasa_tipi=@kasa_tipi, 
                                             motor_gucu=@motor_gucu, motor_hacmi=@motor_hacmi, 
                                             renk=@renk, fiyat=@fiyat 
                                         WHERE id=@id", conn);

            cmd.Parameters.AddWithValue("@marka", MarkaEntry.Text);
            cmd.Parameters.AddWithValue("@model", ModelEntry.Text);
            cmd.Parameters.AddWithValue("@yil", int.Parse(YilEntry.Text));
            cmd.Parameters.AddWithValue("@yakit", YakitEntry.Text);
            cmd.Parameters.AddWithValue("@vites", VitesEntry.Text);
            cmd.Parameters.AddWithValue("@km", int.Parse(KMEntry.Text));
            cmd.Parameters.AddWithValue("@kasa_tipi", KasaTipiEntry.Text);
            cmd.Parameters.AddWithValue("@motor_gucu", MotorGucuEntry.Text);
            cmd.Parameters.AddWithValue("@motor_hacmi", int.Parse(MotorHacmiEntry.Text));
            cmd.Parameters.AddWithValue("@renk", RenkEntry.Text);
            cmd.Parameters.AddWithValue("@fiyat", decimal.Parse(FiyatEntry.Text));
            cmd.Parameters.AddWithValue("@id", arabaId);

            cmd.ExecuteNonQuery();

            await DisplayAlert("Başarılı", "Araba bilgileri güncellendi.", "Tamam");
            await Navigation.PopAsync(); // Önceki sayfaya dön
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}", "Tamam");
        }
    }
}
