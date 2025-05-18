using System;

namespace MauiApp11;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnRentClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RentPage());
    }

    private async void OnGaleriClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GaleriPage());
    }

    private async void OnTemizlikClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Temizlik());
    }
}
