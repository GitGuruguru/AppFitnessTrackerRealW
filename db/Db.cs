using AppFitnessTrackerReal.Models;
using SQLite;

namespace AppFitnessTrackerReal.db
{
    internal class Db
    {
        private static SQLiteAsyncConnection? _sqliteDb;
        private static SQLiteAsyncConnection? _sqliteDb2;

        public static async Task Init()
        {
            if (_sqliteDb != null || _sqliteDb2 != null)
            {
                return;
            }

            var dbDirectory = FileSystem.AppDataDirectory;
            Directory.CreateDirectory(dbDirectory);

            var dbPath = Path.Combine(dbDirectory, "fitnessDb.db");
            var dbPath2 = Path.Combine(dbDirectory, "restoredb.db");

            if (_sqliteDb2 == null)
            {
                _sqliteDb2 = new SQLiteAsyncConnection(dbPath2);
                await _sqliteDb2.CreateTableAsync<User>();
            }

            _sqliteDb = new SQLiteAsyncConnection(dbPath);

            await _sqliteDb.CreateTableAsync<User>();
            await _sqliteDb.CreateTableAsync<HistoryGoalNode>();
            await _sqliteDb.CreateTableAsync<DietNode>();
            await _sqliteDb.CreateTableAsync<ActivityNode>();
            await _sqliteDb.CreateTableAsync<Ingridien>();
        }

        public static async Task AddUser(User user)
        {
            await Init();
            await _sqliteDb!.InsertAsync(user);
        }

        public static async Task UpdateUser(User user)
        {
            await Init();
            await _sqliteDb!.UpdateAsync(user);
        }

        public static async Task<string> DeleteUser(User user)
        {
            await Init();
            await _sqliteDb2!.InsertAsync(user);
            await _sqliteDb!.DeleteAsync(user);
            return $"Succces, deleted {user.Name}, {user.Email}  !";
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

        public static async Task<List<DietNode>> GetDishes()
        {
            await Init();
            return await _sqliteDb!
                .Table<DietNode>()
                .OrderByDescending(dish => dish.Id)
                .ToListAsync();
        }

        public static async Task AddDish(DietNode dish)
        {
            await Init();
            await _sqliteDb!.InsertAsync(dish);
        }

        public static async Task UpdateDish(DietNode dish)
        {
            await Init();
            await _sqliteDb!.UpdateAsync(dish);
        }

        public static async Task DeleteDish(DietNode dish)
        {
            await Init();
            await _sqliteDb!.DeleteAsync(dish);
        }

        public static async Task<List<HistoryGoalNode>> GetGoals()
        {
            await Init();
            return await _sqliteDb!
                .Table<HistoryGoalNode>()
                .OrderBy(goal => goal.FinishDate)
                .ToListAsync();
        }

        public static async Task AddGoal(HistoryGoalNode goal)
        {
            await Init();
            await _sqliteDb!.InsertAsync(goal);
        }

        public static async Task UpdateGoal(HistoryGoalNode goal)
        {
            await Init();
            await _sqliteDb!.UpdateAsync(goal);
        }

        public static async Task DeleteGoal(HistoryGoalNode goal)
        {
            await Init();
            await _sqliteDb!.DeleteAsync(goal);
        }
    }
}
