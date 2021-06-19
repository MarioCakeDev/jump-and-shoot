var _cooldownMs: int;
var _currentTime: int;

func _init(cooldownMs):
	self._cooldownMs = cooldownMs;
	self.reset();
	
func reset():
	self._currentTime = OS.get_ticks_msec();
	
func isRunCooldown() -> bool:
	#print(self._cooldownMs, ' ', OS.get_ticks_msec());
	return self._currentTime + self._cooldownMs <= OS.get_ticks_msec();
	
func stop():
	self.reset();
	self._currentTime += self._cooldownMs + 1;
