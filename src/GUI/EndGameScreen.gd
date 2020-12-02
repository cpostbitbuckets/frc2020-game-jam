extends Control


func _ready():
	get_tree().root.get_node("Server").call("CloseConnection")
	get_tree().root.get_node("Client").call("CloseConnection")

