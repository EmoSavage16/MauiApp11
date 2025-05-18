namespace MauiApp11
{
    public partial class AdminPanel : ContentPage
    {
        public AdminPanel()
        {
            InitializeComponent();
        }

        private async void OnCikisYapClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Çıkış", "Çıkmak istediğinize emin misiniz?", "Evet", "Hayır");
            if (confirm)
            {
                await Navigation.PopToRootAsync(); // Ana sayfaya döner
            }
        }

        private async void OnCalisanlariYonetClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CalisanlarPage());
        }

        // Kiralama Yönet Butonuna Tıklanma İşlemi
        private async void OnKiralamaYonetClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KiraYonetPage());
        }

        private async void OnKiralananAraclarYonetClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KiralananAraclarYonetPage());
        }


        private async void OnTemizlemeYonetClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TemizlemeYonet());
        }

        private async void OnGaleriYonetClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GaleriYonetPage());
        }
    }
}