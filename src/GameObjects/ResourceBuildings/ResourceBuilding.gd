#class_name ResourceBuilding
extends Area2D

enum resource_types {power, science, raw, EXCEPTION}

export (resource_types) var resource_type = resource_types.EXCEPTION
export var resource_amt := -1

func _ready() -> void:
	Signals.connect("day_passed", self, "_on_day_passed")

func _on_day_passed():
	assert(resource_type != resource_types.EXCEPTION)
	assert(resource_amt != -1)

	var resource_info = [resource_type, resource_amt]
	Signals.emit_signal("resource_generated", resource_info)