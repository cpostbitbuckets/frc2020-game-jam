extends Button

func _ready():
	# close our connection, we're done
	get_tree().root.get_node("Server").CloseConnection()
	get_tree().root.get_node("Client").CloseConnection()

func _on_Button_pressed() -> void:
	get_tree().root.get_node("PlayersManager").Reset()
	get_tree().root.get_node("Server").Reset()
	get_tree().change_scene("res://src/Main.tscn")
