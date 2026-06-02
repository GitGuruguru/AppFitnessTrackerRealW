using AppFitnessTrackerReal.Models;
using SQLite;

namespace AppFitnessTrackerReal.db
{
    internal class Db
    {
        private const string ActiveUserPreferenceKey = "active_user_id";

        private static SQLiteAsyncConnection? _sqliteDb;

        public static async Task Init()
        {
            if (_sqliteDb != null)
            {
                return;
            }

            

            var dbPath = Path.Combine(@"C:\Users\lokom\Documents\AppFitnessTrackerReal\db\", "fitnessDb.db");
            _sqliteDb = new SQLiteAsyncConnection(dbPath);

            await _sqliteDb.CreateTableAsync<User>();
            await _sqliteDb.CreateTableAsync<ActivityNode>();
            await _sqliteDb.CreateTableAsync<Ingridien>();

            await EnsureColumnAsync("User", "DishesJson", "TEXT NOT NULL DEFAULT '[]'");
            await EnsureColumnAsync("User", "HistoryJson", "TEXT NOT NULL DEFAULT '[]'");
            await EnsureColumnAsync("User", "DailyCalorieGoal", "INTEGER NOT NULL DEFAULT 2000");
        }

        private static async Task EnsureColumnAsync(string tableName, string columnName, string columnDefinition)
        {
            var existingColumns = await _sqliteDb!.QueryAsync<TableInfoRow>($"PRAGMA table_info([{tableName}])");
            if (existingColumns.Any(column => string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            await _sqliteDb.ExecuteAsync($"ALTER TABLE [{tableName}] ADD COLUMN [{columnName}] {columnDefinition}");
        }

        public static async Task<User> GetOrCreateUser(User candidate)
        {
            await Init();

            var existingUser = await _sqliteDb!
                .Table<User>()
                .Where(user => user.Email == candidate.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                Preferences.Set(ActiveUserPreferenceKey, existingUser.Id);
                return existingUser;
            }

            await _sqliteDb.InsertAsync(candidate);
            Preferences.Set(ActiveUserPreferenceKey, candidate.Id);
            return candidate;
        }

        public static async Task AddUser(User user)
        {
            await Init();
            await _sqliteDb!.InsertAsync(user);
            Preferences.Set(ActiveUserPreferenceKey, user.Id);
        }

        public static async Task UpdateUser(User user)
        {
            await Init();
            await _sqliteDb!.UpdateAsync(user);
        }

        public static async Task<string> DeleteUser(User user)
        {
            await Init();
            await _sqliteDb!.DeleteAsync(user);
            return $"Sukces, usunieto {user.Name}, {user.Email}!";
        }

        public static int GetActiveUserId()
        {
            return Preferences.Get(ActiveUserPreferenceKey, 0);
        }

        public static void ClearActiveUser()
        {
            Preferences.Remove(ActiveUserPreferenceKey);
        }

        public static async Task<User?> GetActiveUser()
        {
            await Init();

            var activeUserId = GetActiveUserId();
            if (activeUserId <= 0)
            {
                return null;
            }

            return await _sqliteDb!
                .Table<User>()
                .Where(user => user.Id == activeUserId)
                .FirstOrDefaultAsync();
        }

        public static async Task<User?> GetLatestUser()
        {
            await Init();
            return await _sqliteDb!
                .Table<User>()
                .OrderByDescending(user => user.Id)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<ActivityNode>> GetActivities()
        {
            await Init();
            return await _sqliteDb!
                .Table<ActivityNode>()
                .OrderByDescending(activity => activity.Id)
                .ToListAsync();
        }

        public static async Task AddActivity(ActivityNode activity)
        {
            await Init();
            await _sqliteDb!.InsertAsync(activity);
        }

        public static async Task SaveDailyCalorieGoal(int calorieGoal)
        {
            var user = await GetRequiredActiveUser();
            user.DailyCalorieGoal = calorieGoal;
            await UpdateUser(user);
        }

        public static async Task<List<DietNode>> GetDishes(int userId)
        {
            var user = await GetUserById(userId);
            return user?.Dishes.OrderByDescending(dish => dish.Id).ToList() ?? [];
        }

        public static async Task AddDish(DietNode dish)
        {
            var user = await GetRequiredActiveUser();
            user.AddDish(dish);
            await UpdateUser(user);
        }

        public static async Task UpdateDish(DietNode dish)
        {
            var user = await GetRequiredActiveUser();
            user.UpdateDish(dish);
            await UpdateUser(user);
        }

        public static async Task DeleteDish(DietNode dish)
        {
            var user = await GetRequiredActiveUser();
            user.DeleteDish(dish.Id);
            await UpdateUser(user);
        }

        public static async Task<List<HistoryGoalNode>> GetGoals(int userId)
        {
            var user = await GetUserById(userId);
            return user?.History.OrderBy(goal => goal.FinishDate).ToList() ?? [];
        }

        public static async Task AddGoal(HistoryGoalNode goal)
        {
            var user = await GetRequiredActiveUser();
            user.AddHistoryGoal(goal);
            await UpdateUser(user);
        }

        public static async Task UpdateGoal(HistoryGoalNode goal)
        {
            var user = await GetRequiredActiveUser();
            user.UpdateHistoryGoal(goal);
            await UpdateUser(user);
        }

        public static async Task DeleteGoal(HistoryGoalNode goal)
        {
            var user = await GetRequiredActiveUser();
            user.DeleteHistoryGoal(goal.Id);
            await UpdateUser(user);
        }

        private static async Task<User?> GetUserById(int userId)
        {
            await Init();
            return await _sqliteDb!
                .Table<User>()
                .Where(user => user.Id == userId)
                .FirstOrDefaultAsync();
        }

        private static async Task<User> GetRequiredActiveUser()
        {
            var user = await GetActiveUser();
            if (user == null)
            {
                throw new InvalidOperationException("Brak aktywnego uzytkownika.");
            }

            return user;
        }

        private sealed class TableInfoRow
        {
            [Column("name")]
            public string Name { get; set; } = string.Empty;
        }
    }
}
