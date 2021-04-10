extends KinematicBody2D
class_name Shot

export var speed = 1000

var destroyed = false

var velocity: Vector2 = Vector2()
var collisionShape: CollisionShape2D;
var particles: Particles2D;
var shouldFree = false;
var shotSound: AudioStreamPlayer2D;
var shotImpact: AudioStreamPlayer2D;

func _ready():
	collisionShape = $CollisionShape2D;
	particles = $Particles2D;
	particles.visible = false;
	shotSound = $ShotSound;
	shotImpact = $ShotImpact;

func _physics_process(delta):
	var collision: KinematicCollision2D = move_and_collide(velocity * delta)
	if not collision:
		return
	if collision.collider.has_method("onShot"):
		collision.collider.onShot()
	
	shotImpact.play();
	destroy();

func fly():
	shotSound.play();
	velocity = Vector2(cos(rotation), sin(rotation)) * speed
	yield(get_tree().create_timer(2), "timeout")
	destroy();
	yield(get_tree().create_timer(0.15), "timeout");
	queue_free();


func destroy():
	if destroyed:
		return;
	destroyed = true;
	set_process(false);
	set_physics_process(false);
	collisionShape.visible = false;
	collisionShape.disabled = true;
	particles.visible = true;
	
	yield(get_tree().create_timer(0.15), "timeout");
	particles.visible = false;
