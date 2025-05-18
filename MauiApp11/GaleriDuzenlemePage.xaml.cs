using MySql.Data.MySqlClient;
using System;
using Microsoft.Maui.Controls;

namespace MauiApp11;

public partial class GaleriDuzenlemePage : ContentPage
{
    private int _arabaId;
    private string connectionString = "server=127.0.0.1;database=bemaotoveritabani;user=root;password=1234;";

    public GaleriDuzenlemePage(int arabaId)
    {
        InitializeComponent();
        _arabaId = arabaId;
        LoadArabaBilgileri();
    }

    private void LoadArabaBilgileri()
    {
        try
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT marka, model, yil, yakit, vites, km, kasa_tipi, motor_gucu, motor_hacmi, renk, fiyat FROM arac_ilanlari WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", _arabaId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                MarkaEntry.Text = reader.GetString("marka");
                ModelEntry.Text = reader.GetString("model");
                YilEntry.Text = reader.GetInt32("yil").ToString();
                YakitEntry.Text = reader.GetString("yakit");
                VitesEntry.Text = reader.GetString("vites");
                KMEntry.Text = reader.GetInt32("km").ToString();
                KasaTipiEntry.Text = reader.GetString("kasa_tipi");
                MotorGucuEntry.Text = reader.GetString("motor_gucu");
                MotorHacmiEntry.Text = reader.GetInt32("motor_hacmi").ToString();
                RenkEntry.Text = reader.GetString("renk");
                FiyatEntry.Text = reader.GetDecimal("fiyat").ToString();
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Hata", $"Araba bilgileri yüklenirken hata oluştu: {ex.Message}", "Tamam");
        }
    }

    private async void OnGuncelleClicked(object sender, EventArgs e)
    {
        try
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"UPDATE arac_ilanlari SET 
                                        marka = @marka,
                                        model = @model,
                                        yil = @yil,
                                        yakit = @yakit,
                                        vites = @vites,
                                        km = @km,
                                        kasa_tipi = @kasa_tipi,
                                        motor_gucu = @motor_gucu,
                                        motor_hacmi = @motor_hacmi,
                                        renk = @renk,
                                        fiyat = @fiyat
                                        WHERE id = @id", conn);

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
            cmd.Parameters.AddWithValue("@id", _arabaId);

            int affected = cmd.ExecuteNonQuery();

            if (affected > 0)
            {
                await DisplayAlert("Başarılı", "Araba bilgileri güncellendi.", "Tamam");
                await Navigation.PopAsync(); // Önceki sayfaya dön
            }
            else
            {
                await DisplayAlert("Hata", "Güncelleme başarısız oldu.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Güncelleme sırasında hata oluştu: {ex.Message}", "Tamam");
        }
    }
}
