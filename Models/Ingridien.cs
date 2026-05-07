using SQLite;

namespace AppFitnessTrackerReal.Models
{
    internal class Ingridien
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        // Persist an image reference instead of a MAUI visual control.
        public string ImagePath { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
