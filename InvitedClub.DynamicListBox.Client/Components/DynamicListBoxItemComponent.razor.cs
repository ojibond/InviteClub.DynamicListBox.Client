using Microsoft.AspNetCore.Components;

namespace InvitedClub.DynamicListBox.Client.Components
{
    public partial class DynamicListBoxItemComponent<TValue> : ComponentBase
    {
        [CascadingParameter] internal DynamicListBox<TValue>? Parent { get; set; }

        [Parameter, EditorRequired] public TValue Value { get; set; } = default!;
        [Parameter, EditorRequired] public string Text { get; set; } = "";

        protected override void OnInitialized()
        {
            Parent?.RegisterChildItem(new ListBoxOption<TValue>(Value, Text));
        }
    }
}
