extends Control

func _ready():
	Server.CloseConnection();
	Client.CloseConnection();
