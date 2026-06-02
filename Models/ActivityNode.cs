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
            get => string.IsNullOrWhiteSpace(_name) ? "Aktywnosc bez nazwy" : _name;
            set => _name = value;
        }

        private string _description = string.Empty;
        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? "Brak opisu." : _description;
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
