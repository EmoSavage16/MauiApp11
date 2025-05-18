using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MauiApp11
{
    public partial class TarihSecPopupPage : ContentPage
    {
        public DateTime? BaslangicTarihi { get; private set; }
        public DateTime? BitisTarihi { get; private set; }
        public decimal ToplamFiyat { get; private set; } = 0;

        private decimal GunlukFiyat;

        // TaskCompletionSource ile dışarıya tarih bilgisi ve toplam fiyat göndermek için
        private TaskCompletionSource<(DateTime?, DateTime?, decimal)> _tcs;

        // Yeni constructor: Günlük fiyatı alır
        public TarihSecPopupPage(decimal gunlukFiyat)
        {
            InitializeComponent();

            GunlukFiyat = gunlukFiyat;
            _tcs = new TaskCompletionSource<(DateTime?, DateTime?, decimal)>();

            // Tarihleri bugünün tarihi olarak başlat
            BaslangicDatePicker.Date = DateTime.Today;
            BitisDatePicker.Date = DateTime.Today;

            // Tarih picker değişimlerine event ekle
            BaslangicDatePicker.DateSelected += DatePicker_DateChanged;
            BitisDatePicker.DateSelected += DatePicker_DateChanged;

            HesaplaVeGoster();
        }

        // Çağıran sayfadan çağrılacak, popup kapandığında tarihleri ve toplam fiyatı beklemek için
        public Task<(DateTime?, DateTime?, decimal)> WaitForDateSelectionAsync()
        {
            return _tcs.Task;
        }

        private void DatePicker_DateChanged(object sender, DateChangedEventArgs e)
        {
            HesaplaVeGoster();
        }

        private void HesaplaVeGoster()
        {
            var baslangic = BaslangicDatePicker.Date;
            var bitis = BitisDatePicker.Date;

            if (bitis < baslangic)
            {
                ToplamFiyatLabel.Text = "Bitiş tarihi, başlangıçtan küçük olamaz!";
                ToplamFiyat = 0;
                return;
            }

            int gunSayisi = (bitis - baslangic).Days + 1;
            ToplamFiyat = gunSayisi * GunlukFiyat;

            ToplamFiyatLabel.Text = $"Toplam Fiyat: ₺{ToplamFiyat:N2} ( {gunSayisi} gün )";
        }

        private async void OnaylaButton_Clicked(object sender, EventArgs e)
        {
            BaslangicTarihi = BaslangicDatePicker.Date;
            BitisTarihi = BitisDatePicker.Date;

            if (BitisTarihi < BaslangicTarihi)
            {
                await DisplayAlert("Hata", "Bitiş tarihi, başlangıç tarihinden küçük olamaz.", "Tamam");
                return;
            }

            // Task'ı tamamla, tarih ve toplam fiyat gönder
            _tcs.TrySetResult((BaslangicTarihi, BitisTarihi, ToplamFiyat));

            await Navigation.PopModalAsync();
        }

        private async void IptalButton_Clicked(object sender, EventArgs e)
        {
            _tcs.TrySetResult((null, null, 0));
            await Navigation.PopModalAsync();
        }
    }
}