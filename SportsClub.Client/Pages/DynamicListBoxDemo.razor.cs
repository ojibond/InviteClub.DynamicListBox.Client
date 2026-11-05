using SportsClub.Components;

namespace SportsClub.Client.Pages
{
    public partial class DynamicListBoxDemo
    {
        private IReadOnlyList<int> _selectedMarkupIds = Array.Empty<int>();

        private string SelectedMarkupLabel => _selectedMarkupIds.Count == 0
            ? "(none)"
            : string.Join(", ", _selectedMarkupIds.Select(GetMarkupLabel));

        private static string GetMarkupLabel(int value) => value switch
        {
            1 => "Markup Item A",
            2 => "Markup Item B",
            3 => "Markup Item C",
            _ => value.ToString()
        };
    }
}
