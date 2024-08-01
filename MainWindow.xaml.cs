using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace CosmicSnapshot
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.nasa.gov/planetary/apod";

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            ApiKeyTextBox.Text = "Enter your NASA API key";
            ApiKeyTextBox.GotFocus += (s, e) => {
                if (ApiKeyTextBox.Text == "Enter your NASA API key")
                    ApiKeyTextBox.Text = "";
            };
        }

        private async void FetchAPOD_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = ApiKeyTextBox.Text.Trim();
            if (string.IsNullOrEmpty(apiKey) || apiKey == "Enter your NASA API key")
            {
                ShowError("Please enter a valid NASA API key.");
                return;
            }

            try
            {
                SetUIState(isLoading: true);
                var apodData = await FetchAPODData(apiKey);
                UpdateUI(apodData);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to capture cosmic wonder: {ex.Message}");
            }
            finally
            {
                SetUIState(isLoading: false);
            }
        }

        private async Task<JObject> FetchAPODData(string apiKey)
        {
            string apiUrl = $"{ApiBaseUrl}?api_key={apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseBody);
        }

        private void UpdateUI(JObject apodData)
        {
            TitleTextBlock.Text = apodData["title"]?.ToString();
            DescriptionTextBlock.Text = apodData["explanation"]?.ToString();

            string imageUrl = apodData["url"]?.ToString();
            if (!string.IsNullOrEmpty(imageUrl))
            {
                APODImage.Source = new BitmapImage(new Uri(imageUrl));
            }
            else
            {
                APODImage.Source = null;
            }
        }

        private void SetUIState(bool isLoading)
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            ApiKeyTextBox.IsEnabled = !isLoading;
            FetchButton.IsEnabled = !isLoading;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Cosmic Anomaly Detected", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}