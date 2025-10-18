namespace NightTerrorMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell(); //Gør Shell til vores hovedside - så app'en bliver fleksibel til fremtidigt arbejde
        }
    }
}