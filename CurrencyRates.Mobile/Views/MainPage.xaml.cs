using CurrencyRates.Mobile.Services;
using CurrencyRates.Mobile.ViewModels;

namespace CurrencyRates.Mobile.Views;

/// <summary>
/// Main page that displays currency exchange rates.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly CurrencyRatesViewModel viewModel;
    private readonly ISyncNotificationService syncNotificationService;
    private bool isInitialized;

    /// <summary>
    /// Initializes a new instance of <see cref="MainPage"/>.
    /// </summary>
    /// <param name="viewModel">The view model injected via DI.</param>
    /// <param name="syncNotificationService">The notification service for FCM and SignalR.</param>
    public MainPage(CurrencyRatesViewModel viewModel, ISyncNotificationService syncNotificationService)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.syncNotificationService = syncNotificationService;
        this.BindingContext = this.viewModel;
    }

    /// <summary>
    /// Loads rates when the page first appears, requests notification permission,
    /// and starts the sync notification service.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!this.isInitialized)
        {
            this.isInitialized = true;

            // Request notification permission on Android 13+
            await this.RequestNotificationPermissionAsync();

            // Start FCM + SignalR notification service
            this.syncNotificationService.OnRatesSynced += this.HandleRatesSynced;
            await this.syncNotificationService.StartAsync();
        }

        await this.viewModel.LoadRatesCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Handles the DatePicker DateSelected event and triggers rate loading.
    /// </summary>
    private async void OnDateSelected(object? sender, DateChangedEventArgs e)
    {
        await this.viewModel.DateChangedCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Handles real-time sync notifications from the server.
    /// Updates the UI on the main thread.
    /// </summary>
    private void HandleRatesSynced(string date, int ratesCount)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            this.viewModel.StatusMessage = $"New rates synced for {date}!";
            await this.viewModel.LoadRatesCommand.ExecuteAsync(null);
        });
    }

    /// <summary>
    /// Requests POST_NOTIFICATIONS permission on Android 13+ (API 33+).
    /// </summary>
    private async Task RequestNotificationPermissionAsync()
    {
        try
        {
#if ANDROID
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                if (status != PermissionStatus.Granted)
                {
                    await Permissions.RequestAsync<Permissions.PostNotifications>();
                }
            }
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to request notification permission: {ex.Message}");
        }
    }
}
