using AppFitnessTrackerReal.db;
using AppFitnessTrackerReal.Models;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;

namespace AppFitnessTrackerReal;

public partial class Dashbord : ContentPage
{
    private readonly ObservableCollection<DietNode> _dishes = [];
    private readonly ObservableCollection<HistoryGoalNode> _goals = [];
    private readonly CalorieRingDrawable _calorieRingDrawable = new();

    private User? _activeUser;
    private DietNode? _selectedDish;
    private DishEditorMode _dishEditorMode = DishEditorMode.Add;
    private bool _isLoaded;

    private double _displayProtein;
    private double _displayCarbs;
    private double _displayFats;
    private double _displayCalories;

    public Dashbord()
    {
        InitializeComponent();

        DishesCollection.ItemsSource = _dishes;
        GoalsCollection.ItemsSource = _goals;
        MacroRingView.Drawable = _calorieRingDrawable;
        GoalSchedulePicker.ItemsSource = new List<string> { "Dzienny", "Tygodniowy", "Miesieczny", "Roczny" };
        GoalSchedulePicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        await LoadUserAsync();
        await LoadDishesAsync();
        await LoadGoalsAsync();
    }

    private async Task LoadUserAsync()
    {
        _activeUser = await Db.GetActiveUser() ?? await Db.GetLatestUser();
        UserNameLabel.Text = _activeUser?.Name ?? "Gosc";
        DailyCaloriesGoalEntry.Text = (_activeUser?.DailyCalorieGoal ?? 2000).ToString();
    }

    private async Task LoadDishesAsync()
    {
        if (_activeUser == null)
        {
            _dishes.Clear();
            await AnimateCalorieRingAsync(0, 0, 0, 0);
            return;
        }

        var dishes = await Db.GetDishes(_activeUser.Id);

        _dishes.Clear();
        foreach (var dish in dishes)
        {
            _dishes.Add(dish);
        }

        await AnimateCalorieRingAsync(
            _dishes.Sum(dish => dish.Calories),
            _dishes.Sum(dish => dish.Protein),
            _dishes.Sum(dish => dish.Carbs),
            _dishes.Sum(dish => dish.Fats));
    }

    private async void OnSaveCalorieGoalClicked(object? sender, EventArgs e)
    {
        if (_activeUser == null)
        {
            await DisplayAlertAsync("Brak uzytkownika", "Zaloguj sie ponownie przed ustawieniem celu kalorii.", "OK");
            return;
        }

        if (!int.TryParse(DailyCaloriesGoalEntry.Text, out var calorieGoal) || calorieGoal <= 0)
        {
            await DisplayAlertAsync("Blad celu", "Podaj dodatni dzienny cel kalorii.", "OK");
            return;
        }

        await Db.SaveDailyCalorieGoal(calorieGoal);
        _activeUser.DailyCalorieGoal = calorieGoal;
        await LoadDishesAsync();
    }

    private async Task LoadGoalsAsync()
    {
        if (_activeUser == null)
        {
            _goals.Clear();
            return;
        }

        var goals = await Db.GetGoals(_activeUser.Id);

        _goals.Clear();
        foreach (var goal in goals)
        {
            _goals.Add(goal);
        }
    }

    private void OnAccountTapped(object? sender, TappedEventArgs e)
    {
        _ = DisplayAlertAsync("Konto", $"Zalogowano jako {UserNameLabel.Text}.", "OK");
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        Db.ClearActiveUser();
        await Shell.Current.GoToAsync("//MainPage");
    }

    private void OnAddDishClicked(object? sender, EventArgs e)
    {
        _dishEditorMode = DishEditorMode.Add;
        DishEditorTitleLabel.Text = "Dodaj posilek";
        DishSaveButton.Text = "Zapisz posilek";
        ClearDishEditor();
        DishEditorPanel.IsVisible = true;
    }

    private async void OnEditDishClicked(object? sender, EventArgs e)
    {
        if (_selectedDish == null)
        {
            await DisplayAlertAsync("Wybierz posilek", "Najpierw dotknij karty posilku, aby wybrac element do edycji.", "OK");
            return;
        }

        _dishEditorMode = DishEditorMode.Edit;
        DishEditorTitleLabel.Text = "Edytuj posilek";
        DishSaveButton.Text = "Aktualizuj";
        FillDishEditor(_selectedDish);
        DishEditorPanel.IsVisible = true;
    }

    private async void OnRemoveDishClicked(object? sender, EventArgs e)
    {
        if (_selectedDish == null)
        {
            await DisplayAlertAsync("Wybierz posilek", "Najpierw dotknij karty posilku, aby wybrac element do usuniecia.", "OK");
            return;
        }

        var confirmed = await DisplayAlertAsync(
            "Usun posilek",
            $"Usun {_selectedDish.Dishname} z listy odzywiania?",
            "Usun",
            "Anuluj");

        if (!confirmed)
        {
            return;
        }

        await Db.DeleteDish(_selectedDish);
        _selectedDish = null;
        DishesCollection.SelectedItem = null;
        DishEditorPanel.IsVisible = false;
        await LoadDishesAsync();
    }

    private async void OnSaveDishClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DishNameEntry.Text) ||
            string.IsNullOrWhiteSpace(DishDescriptionEditor.Text) ||
            !int.TryParse(ProteinEntry.Text, out var protein) ||
            !int.TryParse(CarbsEntry.Text, out var carbs) ||
            !int.TryParse(FatsEntry.Text, out var fats) ||
            !int.TryParse(CaloriesEntry.Text, out var calories) ||
            protein < 0 || carbs < 0 || fats < 0 || calories < 0)
        {
            await DisplayAlertAsync("Brak danych", "Uzupelnij nazwe posilku, opis, kalorie oraz nieujemne wartosci makro.", "OK");
            return;
        }

        if (_dishEditorMode == DishEditorMode.Edit && _selectedDish != null)
        {
            _selectedDish.Dishname = DishNameEntry.Text.Trim();
            _selectedDish.Description = DishDescriptionEditor.Text.Trim();
            _selectedDish.Protein = protein;
            _selectedDish.Carbs = carbs;
            _selectedDish.Fats = fats;
            _selectedDish.Calories = calories;

            await Db.UpdateDish(_selectedDish);
        }
        else
        {
            if (_activeUser == null)
            {
                await DisplayAlertAsync("Brak uzytkownika", "Zaloguj sie ponownie przed dodaniem posilku.", "OK");
                return;
            }

            var dish = new DietNode
            {
                UserId = _activeUser.Id,
                Dishname = DishNameEntry.Text.Trim(),
                Description = DishDescriptionEditor.Text.Trim(),
                Protein = protein,
                Carbs = carbs,
                Fats = fats,
                Calories = calories
            };

            await Db.AddDish(dish);
        }

        ClearDishEditor();
        DishEditorPanel.IsVisible = false;
        DishesCollection.SelectedItem = null;
        _selectedDish = null;

        await LoadDishesAsync();
    }

    private void OnCancelDishEditorClicked(object? sender, EventArgs e)
    {
        ClearDishEditor();
        DishEditorPanel.IsVisible = false;
    }

    private void OnDishSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedDish = e.CurrentSelection.FirstOrDefault() as DietNode;
    }

    private void FillDishEditor(DietNode dish)
    {
        DishNameEntry.Text = dish.Dishname;
        DishDescriptionEditor.Text = dish.Description;
        ProteinEntry.Text = dish.Protein.ToString();
        CarbsEntry.Text = dish.Carbs.ToString();
        FatsEntry.Text = dish.Fats.ToString();
        CaloriesEntry.Text = dish.Calories.ToString();
    }

    private void ClearDishEditor()
    {
        DishNameEntry.Text = string.Empty;
        DishDescriptionEditor.Text = string.Empty;
        ProteinEntry.Text = string.Empty;
        CarbsEntry.Text = string.Empty;
        FatsEntry.Text = string.Empty;
        CaloriesEntry.Text = string.Empty;
    }

    private void OnAddGoalClicked(object? sender, EventArgs e)
    {
        GoalEditorPanel.IsVisible = true;
        GoalTitleEntry.Text = string.Empty;
        GoalDescriptionEditor.Text = string.Empty;
        GoalDatePicker.Date = DateTime.Today;
        GoalSchedulePicker.SelectedIndex = 0;
        RecurringGoalCheckBox.IsChecked = false;
        GoalProgressSlider.Value = 0;
        GoalProgressValueLabel.Text = "0%";
    }

    private void OnCancelGoalEditorClicked(object? sender, EventArgs e)
    {
        GoalEditorPanel.IsVisible = false;
    }

    private void OnGoalProgressChanged(object? sender, ValueChangedEventArgs e)
    {
        GoalProgressValueLabel.Text = $"{Math.Round(e.NewValue)}%";
    }

    private async void OnSaveGoalClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GoalTitleEntry.Text) ||
            string.IsNullOrWhiteSpace(GoalDescriptionEditor.Text))
        {
            await DisplayAlertAsync("Brak danych", "Dodaj tytul i opis celu przed zapisaniem.", "OK");
            return;
        }

        var goal = new HistoryGoalNode
        {
            UserId = _activeUser?.Id ?? 0,
            Header = GoalTitleEntry.Text.Trim(),
            Description = GoalDescriptionEditor.Text.Trim(),
            FinishDate = GoalDatePicker.Date ?? DateTime.Today,
            ScheduleType = GoalSchedulePicker.SelectedItem?.ToString() ?? "Dzienny",
            IsRecurring = RecurringGoalCheckBox.IsChecked,
            Progress = GoalProgressSlider.Value / 100d
        };

        if (goal.UserId <= 0)
        {
            await DisplayAlertAsync("Brak uzytkownika", "Zaloguj sie ponownie przed utworzeniem celu.", "OK");
            return;
        }

        await Db.AddGoal(goal);
        GoalEditorPanel.IsVisible = false;
        await LoadGoalsAsync();
    }

    private async void OnDeleteGoalClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not int goalId)
        {
            return;
        }

        var goal = _goals.FirstOrDefault(item => item.Id == goalId);
        if (goal == null)
        {
            return;
        }

        var confirmed = await DisplayAlertAsync(
            "Usun cel",
            $"Usun {goal.Header}?",
            "Usun",
            "Anuluj");

        if (!confirmed)
        {
            return;
        }

        await Db.DeleteGoal(goal);
        await LoadGoalsAsync();
    }

    private Task AnimateCalorieRingAsync(double calories, double protein, double carbs, double fats)
    {
        var startCalories = _displayCalories;
        var startProtein = _displayProtein;
        var startCarbs = _displayCarbs;
        var startFats = _displayFats;

        this.AbortAnimation("MacroRing");

        var completion = new TaskCompletionSource<bool>();
        this.Animate(
            "MacroRing",
            callback: progress =>
            {
                _displayCalories = Lerp(startCalories, calories, progress);
                _displayProtein = Lerp(startProtein, protein, progress);
                _displayCarbs = Lerp(startCarbs, carbs, progress);
                _displayFats = Lerp(startFats, fats, progress);

                ApplyNutritionVisuals(_displayCalories, _displayProtein, _displayCarbs, _displayFats);
            },
            start: 0,
            end: 1,
            rate: 16,
            length: 420,
            easing: Easing.CubicOut,
            finished: (_, _) =>
            {
                ApplyNutritionVisuals(calories, protein, carbs, fats);
                completion.TrySetResult(true);
            });

        return completion.Task;
    }

    private void ApplyNutritionVisuals(double calories, double protein, double carbs, double fats)
    {
        var calorieGoal = Math.Max(_activeUser?.DailyCalorieGoal ?? 2000, 1);
        _calorieRingDrawable.Calories = (float)calories;
        _calorieRingDrawable.CalorieGoal = calorieGoal;
        MacroRingView.Invalidate();

        ProteinValueLabel.Text = $"{Math.Round(protein)} g";
        CarbsValueLabel.Text = $"{Math.Round(carbs)} g";
        FatsValueLabel.Text = $"{Math.Round(fats)} g";
        MacroTotalLabel.Text = $"{Math.Round(calories)} / {calorieGoal} kcal";
    }

    private static double Lerp(double start, double end, double progress)
    {
        return start + ((end - start) * progress);
    }

    private enum DishEditorMode
    {
        Add,
        Edit
    }
}

internal sealed class CalorieRingDrawable : IDrawable
{
    public float Calories { get; set; }
    public int CalorieGoal { get; set; } = 2000;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        var stroke = 26f;
        var radiusBoxSize = Math.Min(dirtyRect.Width, dirtyRect.Height) - stroke;
        var arcRect = new RectF(
            dirtyRect.Center.X - (radiusBoxSize / 2f),
            dirtyRect.Center.Y - (radiusBoxSize / 2f),
            radiusBoxSize,
            radiusBoxSize);

        canvas.StrokeSize = stroke;
        canvas.StrokeLineCap = LineCap.Round;

        var progress = Math.Clamp(Calories / Math.Max(CalorieGoal, 1), 0f, 1f);
        DrawSegment(canvas, arcRect, -90f, progress * 360f, "#F04438");
    }

    private static void DrawSegment(ICanvas canvas, RectF rect, float startAngle, float sweep, string colorHex)
    {
        if (sweep <= 1f)
        {
            return;
        }

        canvas.StrokeColor = Color.FromArgb(colorHex);
        canvas.DrawArc(rect, startAngle, startAngle + sweep, false, false);
    }
}
