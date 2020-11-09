extends Node2D

func _ready() -> void:
	Signals.connect("resource_generated", self, "_on_resource_generated")

var resources = {
	"power": 100,
	"science": 100,
	"raw": 20,
	"missiles": 5,
	"lasers": 10
}

var score = 100

var tech = {
	"mine": Enums.raw.mine1,
	"power": Enums.power.power1,
	"science": Enums.science.science1,
	"missile": Enums.missile.missile1,
	"laser": Enums.laser.laser1,
	"shield": Enums.shield.shield1
}

func _on_resource_generated(res_list):
	resources[res_list[0]] += res_list[1]