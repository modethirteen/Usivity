/*
	animations.js - manage generic animations
	
	FUNCTIONS
		- sectionresize - 
		- resize - 
*/
$(window).load(function() {	
	
	// RESIZE THE LAYOUT WHEN THE BROWSER IS RESIZED
	$(window).resize(function() {
		resize();
	});
});