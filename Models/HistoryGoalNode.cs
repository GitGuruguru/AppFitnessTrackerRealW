using SQLite;

namespace AppFitnessTrackerReal.Models
{
    internal class HistoryGoalNode
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _header = string.Empty;
        public string Header
        {
            get => string.IsNullOrWhiteSpace(_header) ? "Error: Header not provided" : _header;
            set => _header = value;
        }

        private string _description = string.Empty;
        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? "Error: No description provided " : _description;
            set => _description = value;
        }

        public double Progress { get; set; }

        public DateTime FinishDate { get; set; } = DateTime.UtcNow;

        public string ScheduleType { get; set; } = "Daily";

        public bool IsRecurring { get; set; }
    }
}
