using SQLite;

namespace AppFitnessTrackerReal.Models
{
    internal class DietNode
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        private string _dishname = string.Empty;
        public string Dishname
        {
            get => string.IsNullOrWhiteSpace(_dishname) ? "Error: Name not provided!" : _dishname;
            set => _dishname = value;
        }

        private string _description = string.Empty;
        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? "Error: Description not provided!" : _description;
            set => _description = value;
        }

        // sqlite-net cannot map complex collection properties directly.
        [Ignore]
        public List<Ingridien> Ingridiens { get; init; } = [];

        private int _protein;
        public int Protein
        {
            get => _protein;
            set => _protein = value;
        }

        private int _carbs;
        public int Carbs
        {
            get => _carbs;
            set => _carbs = value;
        }

        private int _fats;
        public int Fats
        {
            get => _fats;
            set => _fats = value;
        }
    }
}
