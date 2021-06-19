extends KinematicBody2D

var velocity: Vector2 = Vector2();
export var speed: float = 750;
var actionsPressed = [];

var snapVector: Vector2 = Vector2.DOWN * 10;

var Stopwatch = load("res://Stopwatch.gd");
var createShot = load("res://Shot.tscn")
var camera: Camera2D;
var sprite: Sprite
var viewportSize: Vector2;
var startPosition: Vector2;
var viewport: Viewport;
var jumpSound: AudioStreamPlayer2D;
var previouslyOnGround: bool = false;

var jumpTimer = Stopwatch.new(200);
var shootTimer = Stopwatch.new(135);
var backupJumpTimer = Stopwatch.new(150);

var canRun: bool = true;
var canJump: bool = true;
var hasFullJump: bool = true;
var canShoot: bool = true;
var canPropell: bool = true;


func _ready():
	startPosition = position;
	camera = $Camera2D
	sprite = $CollisionShape2D/Sprite
	viewportSize = get_viewport_rect().size;
	viewport = get_viewport();
	jumpSound = $JumpSound;

func _physics_process(delta):	
	actOnReleased("left")
	actOnReleased("right")
	
	velocity.x *= 0.8;
	actOnPressed("left", "right")
	actOnPressed("right", "left")
	
	move_and_slide_with_snap(velocity, snapVector, Vector2.UP, true, 2, 0.785398, false);
	
	if(Input.is_action_just_pressed("toggleJump")):
		canJump = !canJump;
	if(Input.is_action_just_pressed("toggleJumpHeight")):
		hasFullJump = !hasFullJump;
	if(Input.is_action_just_pressed("toggleShoot")):
		canShoot = !canShoot;
	if(Input.is_action_just_pressed("togglePropell")):
		canPropell = !canPropell;
	if(Input.is_action_just_pressed("toggleRunSpeed")):
		canRun = !canRun;
	if(Input.is_action_just_pressed("reset")):
		reset();
	
	snapVector = Vector2.DOWN;
	
	handleJumping();
		
	velocity.y = capAbsolute(velocity.y, -1500 if hasFullJump else -300, 1500);
	
	var mousePositionRelativeToCenter = viewport.get_mouse_position() - (viewportSize / 2);
	camera.position = lerp(camera.position,  mousePositionRelativeToCenter / 3, 0.05);
	sprite.rotation_degrees = map(velocity.x, -speed, speed, -20, 20);

func reset():
	#print("Reset");
	position = startPosition;
	velocity = Vector2.ZERO;

func handleJumping():
	var isOnGround = is_on_floor();
	var isOnCeiling = is_on_ceiling();
	
	if(isOnGround):
		velocity.y = 0;
	
	if(isOnCeiling && velocity.y < 0):
		velocity.y = 0;
	
	var isJumping = Input.is_action_pressed("jump");
	
	if (isOnGround or !backupJumpTimer.isRunCooldown()) and isJumping and canJump and jumpTimer.isRunCooldown():
		jumpTimer.reset();
		backupJumpTimer.reset();
		previouslyOnGround = false;
		isOnGround = false;
		snapVector = Vector2.ZERO;
		velocity.y = -750 if hasFullJump else -300;
		jumpSound.play();
	elif isJumping and velocity.y < 0:
		velocity.y += 20;
	else:
		velocity.y += 40;
		
	if previouslyOnGround and not isOnGround:
		backupJumpTimer.reset();
	
	previouslyOnGround = isOnGround;

func shoot(shootPosition: Vector2):	
	if(not canShoot):
		return;
		
	if !shootTimer.isRunCooldown():
		return;
	shootTimer.reset();
	
	var shootDirection: Vector2 = shootPosition - get_global_transform_with_canvas().origin
	shootDirection = shootDirection.normalized()	
		
	var shot: Shot = createShot.instance()
	
	shot.rotation = shootDirection.angle()
	shot.position = position
	
	if(canPropell):
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
	run(-getRunSpeed());
	
func on_right():
	run(getRunSpeed());
	
func getRunSpeed():
	return speed if canRun else speed / 5;	
	
func run(runSpeed):
	velocity.x = lerp(velocity.x / 0.8, runSpeed, 0.3);
	
	if(velocity.x < 0):
		sprite.flip_h = true;
	if(velocity.x > 0):
		sprite.flip_h = false;

func capAbsolute(value, minValue, maxValue):
	return max(minValue, min(value, maxValue));

func map(currentValue, minValue, maxValue, resultMinValue, resultMaxValue):
	var maxDistance = maxValue - minValue;
	var currentDistance = currentValue - minValue;
	var percentalDistance = currentDistance/maxDistance;
	var resultDistance = resultMaxValue - resultMinValue;
	return percentalDistance * resultDistance + resultMinValue;
