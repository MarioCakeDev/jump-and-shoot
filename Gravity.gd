extends Area2D


func _ready():
	applyDirection()
	$GravityArrow.scale.x /= scale.x / 8
	$GravityArrow.region_rect.size.x *= scale.x / 8
	$GravityArrow.scale.y /= scale.y / 8
	$GravityArrow.region_rect.size.y *= scale.y / 8

func applyDirection():
	var quarterRotation = PI / 2;
	if has_meta("Direction"):
		var direction = get_meta("Direction");
		if direction == "Down":
			$SignalReceiver.gravityRotation = quarterRotation * 0
		if direction == "Left":
			$SignalReceiver.gravityRotation = quarterRotation * 1
		if direction == "Up":
			$SignalReceiver.gravityRotation = quarterRotation * 2
		if direction == "Right":
			$SignalReceiver.gravityRotation = quarterRotation * 3
		
		$GravityArrow.rotation = $SignalReceiver.gravityRotation 
		
func _process(_delta):
	$GravityArrow.region_rect.position += Vector2.UP * 0.2
