using SQLite;
using System.Text.Json;

namespace AppFitnessTrackerReal.Models
{
    internal class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => string.IsNullOrEmpty(_name) ? "Blad: nazwa jest pusta" : _name;
            set => _name = value;
        }

        private string _password = string.Empty;
        public string Password
        {
            get => string.IsNullOrEmpty(_password) ? "Blad: haslo jest puste" : _password;
            set => _password = value;
        }

        private string _email = string.Empty;
        public string Email
        {
            get => string.IsNullOrEmpty(_email) ? "email" : _email;
            set => _email = value;
        }

        public string DishesJson { get; set; } = "[]";

        public string HistoryJson { get; set; } = "[]";

        [Ignore]
        public List<DietNode> Dishes
        {
            get => DecodeList<DietNode>(DishesJson);
            set => DishesJson = EncodeList(value);
        }

        [Ignore]
        public List<HistoryGoalNode> History
        {
            get => DecodeList<HistoryGoalNode>(HistoryJson);
            set => HistoryJson = EncodeList(value);
        }

        public void AddDish(DietNode dish)
        {
            var dishes = Dishes;
            dish.Id = NextId(dishes.Select(item => item.Id));
            dish.UserId = Id;
            dishes.Add(dish);
            Dishes = dishes;
        }

        public void UpdateDish(DietNode dish)
        {
            var dishes = Dishes;
            var index = dishes.FindIndex(item => item.Id == dish.Id);
            if (index >= 0)
            {
                dish.UserId = Id;
                dishes[index] = dish;
                Dishes = dishes;
            }
        }

        public void DeleteDish(int dishId)
        {
            var dishes = Dishes;
            dishes.RemoveAll(item => item.Id == dishId);
            Dishes = dishes;
        }

        public void AddHistoryGoal(HistoryGoalNode goal)
        {
            var history = History;
            goal.Id = NextId(history.Select(item => item.Id));
            goal.UserId = Id;
            history.Add(goal);
            History = history;
        }

        public void UpdateHistoryGoal(HistoryGoalNode goal)
        {
            var history = History;
            var index = history.FindIndex(item => item.Id == goal.Id);
            if (index >= 0)
            {
                goal.UserId = Id;
                history[index] = goal;
                History = history;
            }
        }

        public void DeleteHistoryGoal(int goalId)
        {
            var history = History;
            history.RemoveAll(item => item.Id == goalId);
            History = history;
        }

        private static int NextId(IEnumerable<int> ids)
        {
            return ids.DefaultIfEmpty(0).Max() + 1;
        }

        private static string EncodeList<T>(List<T>? items)
        {
            return JsonSerializer.Serialize(items ?? []);
        }

        private static List<T> DecodeList<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<T>>(json) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }
    }
}
