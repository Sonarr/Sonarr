/*
	Base off Tim Holman's tile tilt: http://tholman.com/experiments/css3/metro-tiles/
*/

function addTileTilt()
{
	var tile = this;				
	var mouse = {x: 0, y: 0, down: false};
	var maxRotation = 16;

	var setRotation = function(){
		var width = tile.offsetWidth;
		var height = tile.offsetHeight;
		//Rotations as percentages
		var yRotation = (mouse.x - (width / 2)) / (width / 2);
		var xRotation = (mouse.y - (height / 2)) / (height / 2);
		
		tile.style.webkitTransform = "rotateX("  + -xRotation * maxRotation + "deg)" +
									" rotateY("  + yRotation * maxRotation + "deg)";
									
		tile.style.mozTransform = "rotateX("  + -xRotation * maxRotation + "deg)" +
									" rotateY("  + yRotation * maxRotation + "deg)";
									
		tile.style.transform = "rotateX("  + -xRotation * maxRotation + "deg)" +
									" rotateY("  + yRotation * maxRotation + "deg)";
	}
	
	var MouseDown = function(e){
		mouse.x = e.offsetX;
		mouse.y = e.offsetY;
		mouse.down = true;
		setRotation();
	}
	
	var MouseUp = function(e){
		mouse.down = false;
		tile.style.webkitTransform = "rotateX(0deg)" +
									" rotateY(0deg)";
									
		tile.style.mozTransform = "rotateX(0deg)" +
								  " rotateY(0deg)";
								  
		tile.style.transform = "rotateX(0deg)" +
							   " rotateY(0deg)";
	}
	
	var MouseMove = function(e){
		mouse.x = e.offsetX;
		mouse.y = e.offsetY;
		if (mouse.down == true){
			setRotation();
		}
	}


	tile.addEventListener('mousemove', MouseMove, false);
	tile.addEventListener('mousedown', MouseDown, false);
	tile.addEventListener('mouseup', MouseUp, false);
	tile.addEventListener('mouseout', MouseUp, false);
}