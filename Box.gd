extends KinematicBody2D
class_name Box

var life = 4;
var maxLives = 4;
var sprite: Sprite;
var box: Node2D;

var velocity: Vector2 = Vector2();

func _ready():
	sprite = $"box";
	box = $"..";

func _physics_process(_delta):
	if(!is_on_floor()):
		velocity.y += 40;
		
	velocity = move_and_slide_with_snap(velocity, Vector2.DOWN * 5, Vector2.UP, true, 2, 0.785398, false);

func onShot():
	life -= 1;
	
	if(life == 0):
		box.queue_free();
		return;
		
	sprite.frame = maxLives - life;
	
