extends Button

func _ready():
	# close our connection, we're done
	Server.CloseConnection()
	Client.CloseConnection()

func _on_Button_pressed() -> void:
	PlayersManager.Reset()
	Server.Reset()
	get_tree().change_scene("res://src/Main.tscn")
