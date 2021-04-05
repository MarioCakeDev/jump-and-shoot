extends KinematicBody2D

var velocity: Vector2 = Vector2();
export var speed: float = 750;
var actionsPressed = [];

var snapVector: Vector2 = Vector2.DOWN * 10;

var createShot = load("res://Shot.tscn")
var shootTimer = 0.2
var camera: Camera2D;
var sprite: Sprite
var viewportSize: Vector2;
var viewport: Viewport;

# Called when the node enters the scene tree for the first time.
func _ready():
	camera = $Camera2D
	sprite = $CollisionShape2D/Sprite
	viewportSize = get_viewport_rect().size;
	viewport = get_viewport();


func _physics_process(delta):
	shootTimer -= delta
	
	actOnReleased("left")
	actOnReleased("right")
	
	velocity.x *= 0.8;
	actOnPressed("left", "right")
	actOnPressed("right", "left")
	
	
	move_and_slide_with_snap(velocity, snapVector, Vector2.UP);
	snapVector = Vector2.DOWN * 10;
	
	var isJumping = Input.is_action_pressed("jump");
	
	var isOnGround = is_on_floor();
	var isOnCeiling = is_on_ceiling();
	
	if(isOnGround or isOnCeiling):
		velocity.y = 0;
	
	if isOnGround:
		if(isJumping):
			snapVector = Vector2.ZERO;
			velocity.y = -750;
	else:
		if(isJumping and velocity.y < 0):
			velocity.y += 20;
		else:
			velocity.y += 40;
		
	velocity.y = capAbsolute(velocity.y, 1500);
	
	var mousePositionRelativeToCenter = viewport.get_mouse_position() - (viewportSize / 2);
	camera.position = lerp(camera.position,  mousePositionRelativeToCenter / 3, 0.05);
	sprite.rotation_degrees = map(velocity.x, -speed, speed, -10, 10);

func shoot(shootPosition: Vector2):
	if shootTimer > 0:
		return
	shootTimer = 0.15
	
	var shootDirection: Vector2 = shootPosition - get_global_transform_with_canvas().origin
	shootDirection = shootDirection.normalized()	
		
	var shot: Shot = createShot.instance()
	
	shot.rotation = shootDirection.angle()
	shot.position = position
	
	velocity -= shootDirection * Vector2(100, 225 if velocity.y > 0 else 100)
	
	$"..".add_child(shot)
	shot.fly()
	
func _input(event):	
	if event is InputEventMouseButton and event.pressed and event.button_index == BUTTON_LEFT:
		shoot(event.position)	
		
func actOnPressed(action: String, againstAction: String):
	if Input.is_action_pressed(action):
		if not actionsPressed.has(action):
			actionsPressed.push_back(action)
			call("on_" + action);
		else:
			var againstActionPosition = actionsPressed.find(againstAction);
			var actionPosition = actionsPressed.find(action);
			if(againstActionPosition == -1 or actionPosition > againstActionPosition):
				call("on_" + action);

func actOnReleased(action: String):
	if Input.is_action_just_released(action):
		if actionsPressed.has(action):
			var pos = actionsPressed.find(action)
			actionsPressed.remove(pos)

func on_left():
	velocity.x = lerp(velocity.x / 0.8, -speed, 0.3);
	
func on_right():
	velocity.x = lerp(velocity.x / 0.8, speed, 0.3);

func capAbsolute(value, maxValue):
	return sign(velocity.y) * min(abs(velocity.y), maxValue);

func map(currentValue, minValue, maxValue, resultMinValue, resultMaxValue):
	var maxDistance = maxValue - minValue;
	var currentDistance = currentValue - minValue;
	var percentalDistance = currentDistance/maxDistance;
	var resultDistance = resultMaxValue - resultMinValue;
	return percentalDistance * resultDistance + resultMinValue;
