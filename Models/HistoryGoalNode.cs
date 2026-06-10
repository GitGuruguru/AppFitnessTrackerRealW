using SQLite;

namespace AppFitnessTrackerReal.Models
{
    internal class HistoryGoalNode
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        private string _header = string.Empty;
        public string Header
        {
            get => string.IsNullOrWhiteSpace(_header) ? "Blad: nie podano naglowka" : _header;
            set => _header = value;
        }

        private string _description = string.Empty;
        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? "Blad: nie podano opisu" : _description;
            set => _description = value;
        }

        public double Progress { get; set; }

        public DateTime FinishDate { get; set; } = DateTime.UtcNow;

        public string ScheduleType { get; set; } = "Dzienny";


    }
}
