using Godot;
using System;

public class Main : MarginContainer
{

    WindowDialog hostWindow;
    WindowDialog joinWindow;
    LineEdit joinHostEdit;
    LineEdit hostPortEdit;
    LineEdit joinPortEdit;

    CheckButton easyModeCheckButton;

    private bool joining = false;

    public override void _Ready()
    {
        hostWindow = GetNode<WindowDialog>("HostWindow");
        joinWindow = GetNode<WindowDialog>("JoinWindow");
        hostPortEdit = (LineEdit)hostWindow.FindNode("PortEdit");
        joinHostEdit = (LineEdit)joinWindow.FindNode("HostEdit");
        joinPortEdit = (LineEdit)joinWindow.FindNode("PortEdit");
        easyModeCheckButton = (CheckButton)FindNode("EasyModeCheckButton");

        hostPortEdit.Text = GameSettings.Instance.ServerPort.ToString();
        joinHostEdit.Text = GameSettings.Instance.ClientHost;
        joinPortEdit.Text = GameSettings.Instance.ClientPort.ToString();
        easyModeCheckButton.Pressed = GameSettings.Instance.Easy;

        FindNode("ExitButton").Connect("pressed", this, nameof(OnExitButtonPressed));
        FindNode("SettingsButton").Connect("pressed", this, nameof(OnSettingsButtonPressed));
        FindNode("NewGameButton").Connect("pressed", this, nameof(OnNewGameButtonPressed));
        FindNode("HostGameButton").Connect("pressed", this, nameof(OnHostGameButtonPressed));
        FindNode("JoinGameButton").Connect("pressed", this, nameof(OnJoinGameButtonPressed));
        easyModeCheckButton.Connect("toggled", this, nameof(OnEasyModeCheckButtonPressed));

        joinWindow.Connect("popup_hide", this, nameof(OnJoinWindoPopupHide));
        joinWindow.FindNode("CancelButton").Connect("pressed", this, nameof(OnJoinWindowCancelButtonPressed));
        joinWindow.FindNode("JoinButton").Connect("pressed", this, nameof(OnJoinWindowJoinButtonPressed));
        hostWindow.Connect("popup_hide", this, nameof(OnHostWindowPopupHide));
        hostWindow.FindNode("HostButton").Connect("pressed", this, nameof(OnHostWindowHostButtonPressed));

        Signals.PlayerUpdatedEvent += OnPlayerUpdated;
        GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
        GetTree().Connect("connection_failed", this, nameof(OnConnectionFailed));
    }

    public override void _ExitTree()
    {
        Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
    }

    void OnEasyModeCheckButtonPressed(bool buttonPressed)
    {
        GameSettings.Instance.Easy = buttonPressed;
    }


    void OnJoinWindowCancelButtonPressed()
    {
        joining = false;
        Client.Instance.CloseConnection();
        ((Button)joinWindow.FindNode("CancelButton")).Disabled = true;
        ((Button)joinWindow.FindNode("JoinButton")).Text = "Join";
    }

    void OnJoinWindowJoinButtonPressed()
    {
        joining = true;
        ((Button)joinWindow.FindNode("CancelButton")).Disabled = false;
        ((Button)joinWindow.FindNode("JoinButton")).Text = "Joining...";
        var host = ((LineEdit)joinWindow.FindNode("HostEdit")).Text;
        var port = int.Parse(((LineEdit)joinWindow.FindNode("PortEdit")).Text);
        GameSettings.Instance.ClientHost = host;
        GameSettings.Instance.ClientPort = port;
        Client.Instance.JoinGame(host, port);
    }

    void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

    void OnNewGameButtonPressed()
    {
        GetTree().ChangeScene("res://src/World.tscn");
    }

    void OnSettingsButtonPressed()
    {
        GetTree().ChangeScene("res://src/Screens/Settings.tscn");
    }

    void OnHostGameButtonPressed()
    {
        Hide();
        hostWindow.PopupCentered();
    }

    void OnJoinGameButtonPressed()
    {
        Hide();
        joinWindow.PopupCentered();
    }

    void OnJoinWindoPopupHide()
    {
        Show();
    }

    void OnHostWindowPopupHide()
    {
        Show();
    }

    void OnHostWindowHostButtonPressed()
    {
        GameSettings.Instance.ServerPort = int.Parse(hostPortEdit.Text);
        Server.Instance.HostGame(GameSettings.Instance.ServerPort);
        Server.Instance.BeginGame();
        GetTree().ChangeScene("res://src/Screens/Lobby.tscn");
    }

    public void OnServerDisconnected()
    {
        joining = false;
    }

    public void OnConnectionFailed()
    {
        joining = false;
    }

    void OnPlayerUpdated(PlayerData player)
    {
        if (joining && this.IsClient() && player.NetworkId == this.GetNetworkId())
        {
            GetTree().ChangeScene("res://src/Screens/Lobby.tscn");
        }
    }
}
