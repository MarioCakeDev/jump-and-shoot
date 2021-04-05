extends KinematicBody2D
class_name Shot

export var speed = 1000

var destroyed = false

var velocity: Vector2 = Vector2()
var collisionShape: CollisionShape2D;
var particles: Particles2D;
var shouldFree = false;

func _ready():
	collisionShape = $CollisionShape2D;
	particles = $Particles2D;
	particles.visible = false;

func _physics_process(delta):
	var collision: KinematicCollision2D = move_and_collide(velocity * delta)
	if not collision:
		return
	if collision.collider.has_method("onShot"):
		collision.collider.onShot()
		
	destroy();

func fly():
	velocity = Vector2(cos(rotation), sin(rotation)) * speed
	yield(get_tree().create_timer(2), "timeout")
	destroy();
	return free();

func free():
	if shouldFree:
		queue_free()
		return
	shouldFree = true;

func destroy():
	if destroyed:
		return;
	destroyed = true;
	set_process(false);
	set_physics_process(false);
	collisionShape.visible = false;
	collisionShape.disabled = true;
	particles.visible = true;
	
	yield(get_tree().create_timer(0.1), "timeout");
	return free();
