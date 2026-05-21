using AppFitnessTrackerReal.db;
using AppFitnessTrackerReal.Models;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;

namespace AppFitnessTrackerReal;

public partial class Dashbord : ContentPage
{
    private readonly ObservableCollection<DietNode> _dishes = [];
    private readonly ObservableCollection<HistoryGoalNode> _goals = [];
    private readonly MacroRingDrawable _macroRingDrawable = new();

    private User? _activeUser;
    private DietNode? _selectedDish;
    private DishEditorMode _dishEditorMode = DishEditorMode.Add;
    private bool _isLoaded;

    private double _displayProtein;
    private double _displayCarbs;
    private double _displayFats;

    public Dashbord()
    {
        InitializeComponent();

        DishesCollection.ItemsSource = _dishes;
        GoalsCollection.ItemsSource = _goals;
        MacroRingView.Drawable = _macroRingDrawable;
        GoalSchedulePicker.ItemsSource = new List<string> { "Daily", "Weekly", "Monthly", "Yearly" };
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
        UserNameLabel.Text = _activeUser?.Name ?? "Guest";
    }

    private async Task LoadDishesAsync()
    {
        if (_activeUser == null)
        {
            _dishes.Clear();
            await AnimateMacroRingAsync(0, 0, 0);
            return;
        }

        var dishes = await Db.GetDishes(_activeUser.Id);

        _dishes.Clear();
        foreach (var dish in dishes)
        {
            _dishes.Add(dish);
        }

        await AnimateMacroRingAsync(
            _dishes.Sum(dish => dish.Protein),
            _dishes.Sum(dish => dish.Carbs),
            _dishes.Sum(dish => dish.Fats));
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
        _ = DisplayAlertAsync("Account", $"Signed in as {UserNameLabel.Text}.", "OK");
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        Db.ClearActiveUser();
        await Shell.Current.GoToAsync("//MainPage");
    }

    private void OnAddDishClicked(object? sender, EventArgs e)
    {
        _dishEditorMode = DishEditorMode.Add;
        DishEditorTitleLabel.Text = "Add dish";
        DishSaveButton.Text = "Save dish";
        ClearDishEditor();
        DishEditorPanel.IsVisible = true;
    }

    private async void OnEditDishClicked(object? sender, EventArgs e)
    {
        if (_selectedDish == null)
        {
            await DisplayAlertAsync("Select a dish", "Tap a dish card first so we know what to edit.", "OK");
            return;
        }

        _dishEditorMode = DishEditorMode.Edit;
        DishEditorTitleLabel.Text = "Edit dish";
        DishSaveButton.Text = "Update dish";
        FillDishEditor(_selectedDish);
        DishEditorPanel.IsVisible = true;
    }

    private async void OnRemoveDishClicked(object? sender, EventArgs e)
    {
        if (_selectedDish == null)
        {
            await DisplayAlertAsync("Select a dish", "Tap a dish card first so we know what to remove.", "OK");
            return;
        }

        var confirmed = await DisplayAlertAsync(
            "Remove dish",
            $"Delete {_selectedDish.Dishname} from your nutrition list?",
            "Delete",
            "Cancel");

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
            protein < 0 || carbs < 0 || fats < 0)
        {
            await DisplayAlertAsync("Missing data", "Fill in the dish name, description, and non-negative macro values.", "OK");
            return;
        }

        if (_dishEditorMode == DishEditorMode.Edit && _selectedDish != null)
        {
            _selectedDish.Dishname = DishNameEntry.Text.Trim();
            _selectedDish.Description = DishDescriptionEditor.Text.Trim();
            _selectedDish.Protein = protein;
            _selectedDish.Carbs = carbs;
            _selectedDish.Fats = fats;

            await Db.UpdateDish(_selectedDish);
        }
        else
        {
            if (_activeUser == null)
            {
                await DisplayAlertAsync("No user", "Log in again before adding dishes.", "OK");
                return;
            }

            var dish = new DietNode
            {
                UserId = _activeUser.Id,
                Dishname = DishNameEntry.Text.Trim(),
                Description = DishDescriptionEditor.Text.Trim(),
                Protein = protein,
                Carbs = carbs,
                Fats = fats
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
    }

    private void ClearDishEditor()
    {
        DishNameEntry.Text = string.Empty;
        DishDescriptionEditor.Text = string.Empty;
        ProteinEntry.Text = string.Empty;
        CarbsEntry.Text = string.Empty;
        FatsEntry.Text = string.Empty;
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
            await DisplayAlertAsync("Missing data", "Add a goal title and description before saving.", "OK");
            return;
        }

        var goal = new HistoryGoalNode
        {
            UserId = _activeUser?.Id ?? 0,
            Header = GoalTitleEntry.Text.Trim(),
            Description = GoalDescriptionEditor.Text.Trim(),
            FinishDate = GoalDatePicker.Date ?? DateTime.Today,
            ScheduleType = GoalSchedulePicker.SelectedItem?.ToString() ?? "Daily",
            IsRecurring = RecurringGoalCheckBox.IsChecked,
            Progress = GoalProgressSlider.Value / 100d
        };

        if (goal.UserId <= 0)
        {
            await DisplayAlertAsync("No user", "Log in again before creating goals.", "OK");
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
            "Delete goal",
            $"Remove {goal.Header}?",
            "Delete",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        await Db.DeleteGoal(goal);
        await LoadGoalsAsync();
    }

    private Task AnimateMacroRingAsync(double protein, double carbs, double fats)
    {
        var startProtein = _displayProtein;
        var startCarbs = _displayCarbs;
        var startFats = _displayFats;

        this.AbortAnimation("MacroRing");

        var completion = new TaskCompletionSource<bool>();
        this.Animate(
            "MacroRing",
            callback: progress =>
            {
                _displayProtein = Lerp(startProtein, protein, progress);
                _displayCarbs = Lerp(startCarbs, carbs, progress);
                _displayFats = Lerp(startFats, fats, progress);

                ApplyMacroVisuals(_displayProtein, _displayCarbs, _displayFats);
            },
            start: 0,
            end: 1,
            rate: 16,
            length: 420,
            easing: Easing.CubicOut,
            finished: (_, _) =>
            {
                ApplyMacroVisuals(protein, carbs, fats);
                completion.TrySetResult(true);
            });

        return completion.Task;
    }

    private void ApplyMacroVisuals(double protein, double carbs, double fats)
    {
        _macroRingDrawable.Protein = (float)protein;
        _macroRingDrawable.Carbs = (float)carbs;
        _macroRingDrawable.Fats = (float)fats;
        MacroRingView.Invalidate();

        ProteinValueLabel.Text = $"{Math.Round(protein)} g";
        CarbsValueLabel.Text = $"{Math.Round(carbs)} g";
        FatsValueLabel.Text = $"{Math.Round(fats)} g";
        MacroTotalLabel.Text = $"{Math.Round(protein + carbs + fats)} g";
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

internal sealed class MacroRingDrawable : IDrawable
{
    public float Protein { get; set; }
    public float Carbs { get; set; }
    public float Fats { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        var stroke = 26f;
        var inset = stroke / 2f;
        var arcRect = new RectF(
            dirtyRect.X + inset,
            dirtyRect.Y + inset,
            dirtyRect.Width - stroke,
            dirtyRect.Height - stroke);

        canvas.StrokeSize = stroke;
        canvas.StrokeLineCap = LineCap.Round;

        canvas.StrokeColor = Color.FromArgb("#1D2A45");
        canvas.DrawArc(arcRect, 0, 360, false, false);

        var total = Math.Max(Protein + Carbs + Fats, 1f);
        var startAngle = -90f;
        var gap = 4f;

        DrawSegment(canvas, arcRect, startAngle, (Protein / total * 360f) - gap, "#78D7B4");
        startAngle += Protein / total * 360f;

        DrawSegment(canvas, arcRect, startAngle, (Carbs / total * 360f) - gap, "#6AA7FF");
        startAngle += Carbs / total * 360f;

        DrawSegment(canvas, arcRect, startAngle, (Fats / total * 360f) - gap, "#FFB45C");
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
