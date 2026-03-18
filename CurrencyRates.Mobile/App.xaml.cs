namespace CurrencyRates.Mobile;

/// <summary>
/// The main application class. Configures the app shell as the root page.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="App"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates the main window with the application shell.
    /// </summary>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(this.serviceProvider.GetRequiredService<AppShell>());
    }
}
