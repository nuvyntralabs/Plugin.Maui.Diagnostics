using Plugin.Maui.Diagnostics;

namespace Plugin.Maui.Diagnostics.Sample;

public partial class OrderPage : ContentPage
{
    readonly IMauiDiagnostics _diagnostics;

    public OrderPage(IMauiDiagnostics diagnostics)
    {
        InitializeComponent();
        _diagnostics = diagnostics;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _diagnostics.TrackScreen("Order Screen");
    }

    void OnQuantityClicked(object? sender, EventArgs e)
    {
        _diagnostics.TrackUserAction("Change quantity");
        StatusLabel.Text = "Last user action is now Change quantity.";
    }
}
