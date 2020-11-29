tool
extends Control

var PlayerColors = preload("res://assets/PlayerColors.tres")

var source_player_num = 1
export var player_num := 2 setget set_player_num
export var player_name := "Bit Buckets" setget set_player_name
export var resource_give_amount := 1 # default to give one resource

func _ready():
	if not Engine.editor_hint:
		source_player_num = PlayersManager.Me.Num

		Signals.connect("player_data_updated", self, "_on_player_data_updated")
		_on_player_data_updated(PlayersManager.GetPlayer(player_num))

func _input(event):
	if event.is_action_pressed("ui_give_100"):
		resource_give_amount = 100
	if event.is_action_pressed("ui_give_10"):
		resource_give_amount = 10
	if event.is_action_released("ui_give_10") or event.is_action_released("ui_give_100"):
		resource_give_amount = 1

func set_player_num(value):
	if value >= 0 && value < PlayerColors.colors.size():
		modulate = PlayerColors.colors[value]
		player_num = value

func set_player_name(value):
	player_name = value
	$PlayerName.text = value

func _on_player_data_updated(player_data):
	if player_data.Num == player_num:
		pass
		#$Resources/Raw/Value.text = "%s" % player_data.Resources.Raw
		#$Resources/Power/Value.text = "%s" % player_data.Resources.Power
		#$Resources/Science/Value.text = "%s" % player_data.Resources.Science

func _on_GiveRawButton_pressed():
	Signals.emit_signal("player_give_resources", source_player_num, player_num, Enums.resource_types.raw, resource_give_amount)
	# RPC.send_player_give_resources(source_player_num, player_num, Enums.resource_types.raw, resource_give_amount)


func _on_GivePowerButton_pressed():
	Signals.emit_signal("player_give_resources", source_player_num, player_num, Enums.resource_types.power, resource_give_amount)
	# RPC.send_player_give_resources(source_player_num, player_num, Enums.resource_types.power, resource_give_amount)


func _on_GiveScienceButton_pressed():
	Signals.emit_signal("player_give_resources", source_player_num, player_num, Enums.resource_types.science, resource_give_amount)
	# RPC.send_player_give_resources(source_player_num, player_num, Enums.resource_types.science, resource_give_amount)
