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
            get => string.IsNullOrWhiteSpace(_dishname) ? "Blad: nie podano nazwy!" : _dishname;
            set => _dishname = value;
        }

        private string _description = string.Empty;
        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? "Blad: nie podano opisu!" : _description;
            set => _description = value;
        }

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

        private int _calories;
        public int Calories
        {
            get => _calories;
            set => _calories = value;
        }
    }
}
