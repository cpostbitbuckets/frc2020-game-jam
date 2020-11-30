using Godot;
using System;

public class ResearchPopup : AcceptDialog
{
    public event Action<ResearchType, int> ConfirmedEvent;
    public ResearchType Type { get; set; }
    public int Level { get; set; }

    string defaultText = "Do you want to research this Tech?\n";

    public override void _Ready()
    {
        RectScale = new Vector2(2, 2);
        DialogText = defaultText;
        GetOk().Text = "Research!";

        Connect("confirmed", this, nameof(OnConfirmed));
        Connect("set_popup_properties", this, nameof(OnSetPopupProperties));
    }

    public void SetInfo(ResearchType type, int level)
    {
        Type = type;
        Level = level;

        int cost = Constants.ResearchCosts[type][level - 2];
        DialogText = $"{defaultText}\n\nCost: {cost}";
    }

    void OnConfirmed()
    {
        ConfirmedEvent?.Invoke(Type, Level);
    }

    void OnSetPopupProperties()
    {
        WindowTitle = $"{Type} {Level}";
    }

}
