namespace NightTerrorMaui
{
    // Code-behind for MainPage.xaml – styrer logikken bag UI-elementerne
    public partial class MainPage : ContentPage
    {
        int count = 0; //tæller hvor mange gange knappen er trykket

        public MainPage()
        {
            InitializeComponent(); // loader XAML-indholdet (UI-elementerne)
        }

        // Event handler: kaldes når knappen i UI trykkes
        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++; 
            
            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
