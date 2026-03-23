using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyRates.Mobile.Models;
using CurrencyRates.Mobile.Services;

namespace CurrencyRates.Mobile.ViewModels;

/// <summary>
/// ViewModel for the main currency rates page.
/// Manages loading, filtering by date, refreshing, and manual sync.
/// </summary>
public partial class CurrencyRatesViewModel : ObservableObject
{
    private readonly ICurrencyRateApiClient apiClient;

    /// <summary>
    /// The collection of currency rates displayed in the UI.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CurrencyRateDto> rates = new();

    /// <summary>
    /// Indicates whether data is currently being loaded.
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// Indicates whether the view is currently refreshing (pull-to-refresh).
    /// </summary>
    [ObservableProperty]
    private bool isRefreshing;

    /// <summary>
    /// The selected date for filtering rates.
    /// </summary>
    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    /// <summary>
    /// Error message displayed to the user when a network or API error occurs.
    /// </summary>
    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Whether an error message is currently visible.
    /// </summary>
    [ObservableProperty]
    private bool hasError;

    /// <summary>
    /// Status message displayed after a successful sync operation.
    /// </summary>
    [ObservableProperty]
    private string? statusMessage;

    /// <summary>
    /// Initializes a new instance of <see cref="CurrencyRatesViewModel"/>.
    /// </summary>
    /// <param name="apiClient">The API client for fetching currency rates.</param>
    public CurrencyRatesViewModel(ICurrencyRateApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    /// <summary>
    /// Loads currency rates for the currently selected date.
    /// Called on page load and when the date changes.
    /// </summary>
    [RelayCommand]
    private async Task LoadRatesAsync()
    {
        await this.FetchRatesAsync();
    }

    /// <summary>
    /// Refreshes currency rates (used by pull-to-refresh).
    /// </summary>
    [RelayCommand]
    private async Task RefreshRatesAsync()
    {
        await this.FetchRatesAsync();
        this.IsRefreshing = false;
    }

    /// <summary>
    /// Loads rates for the selected date when the date picker value changes.
    /// </summary>
    [RelayCommand]
    private async Task DateChangedAsync()
    {
        await this.FetchRatesAsync();
    }

    /// <summary>
    /// Triggers a manual sync on the server and reloads rates.
    /// </summary>
    [RelayCommand]
    private async Task SyncAsync()
    {
        try
        {
            this.IsLoading = true;
            this.ClearError();
            this.StatusMessage = null;

            await this.apiClient.SyncRatesAsync();
            this.StatusMessage = "Sync completed successfully!";

            await this.FetchRatesAsync();
        }
        catch (HttpRequestException)
        {
            this.ShowError("Failed to sync rates. The server may be unreachable.");
        }
        catch (Exception ex)
        {
            this.ShowError($"Unexpected error during sync: {ex.Message}");
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Fetches rates from the API for the selected date and updates the collection.
    /// </summary>
    private async Task FetchRatesAsync()
    {
        try
        {
            this.IsLoading = true;
            this.ClearError();

            var result = await this.apiClient.GetRatesByDateAsync(this.SelectedDate);

            this.Rates.Clear();
            foreach (var rate in result)
            {
                this.Rates.Add(rate);
            }
        }
        catch (HttpRequestException)
        {
            this.ShowError("Unable to connect to the server. Please check your internet connection.");
        }
        catch (Exception ex)
        {
            this.ShowError($"An error occurred: {ex.Message}");
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Displays an error message to the user.
    /// </summary>
    /// <param name="message">The error message text.</param>
    private void ShowError(string message)
    {
        this.ErrorMessage = message;
        this.HasError = true;
    }

    /// <summary>
    /// Clears any currently displayed error message.
    /// </summary>
    private void ClearError()
    {
        this.ErrorMessage = null;
        this.HasError = false;
    }
}
