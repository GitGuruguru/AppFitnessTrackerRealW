
namespace AppFitnessTrackerReal
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("DashBord", typeof(Dashbord));
            Routing.RegisterRoute("MainPage", typeof(MainPage));
        }
    }
}
