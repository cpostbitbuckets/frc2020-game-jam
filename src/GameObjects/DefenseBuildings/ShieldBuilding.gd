extends GameBuilding

func _ready() -> void:
	is_defense_building = true
	is_resource_building = false

func activate():
	.activate()
	$ShieldArea.active = true
