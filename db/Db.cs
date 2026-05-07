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
    }
}
