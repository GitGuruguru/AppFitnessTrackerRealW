using SQLite;

namespace AppFitnessTrackerReal.Models
{
    internal class ActivityNode
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => string.IsNullOrWhiteSpace(_name) ? "Unnamed activity" : _name;
            set => _name = value;
        }

        private string _description = string.Empty;
        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? "No description provided." : _description;
            set => _description = value;
        }

        private int _calorieBurn;
        public int CalorieBurn
        {
            get => _calorieBurn;
            set => _calorieBurn = value;
        }
    }
}
