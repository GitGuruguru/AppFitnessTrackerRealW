using AppFitnessTrackerReal.db;
using AppFitnessTrackerReal.Models;
using System.Collections.ObjectModel;

namespace AppFitnessTrackerReal;

public partial class Dashbord : ContentPage
{
    private readonly ObservableCollection<ActivityNode> _activities = [];

    public Dashbord()
    {
        InitializeComponent();
    }
}
