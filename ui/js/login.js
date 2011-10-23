/*
	login.js - format and handle generica form interactions
	TODO: MOVE THIS FILE TO SEPARATE DIRECTORY.  THIS FILE LIVES ON THE API FOR SECURE AUTHENTICATION
	FUNCTIONS
		-
		-
		-
*/

$(document).ready( function() {
	
	$(window).resize(function() {
		resize();
	});
	$(window).load(function() {
		resize();
	});
	
	/*MENU*/
	$(".menu").click( function() {
		var src = $(this).attr("href");
		dialog(src);
		return false;
	});
	
	
		
});

function resize() {
	
	var bheight = $(window).height(); 			// Body Height
	var bwidth	= $(window).width();			// Body Width
	
	var lheight = $(".logo").height();			// Logo Height
	
	var wheight = $(".wrap").height(); 			// Wrap Height
	var wtop 	= (((bheight - wheight) / 2) - lheight); 	// Calculate the top of the login
	
	var wwidth 	= $(".wrap").width(); 			// Wrap Width
	var wleft	= ((bwidth - wwidth) / 2);		// Calculate the left of the login
	
	
	$(".wrap").css("top",wtop);
	$(".wrap").css("left",wleft);
	$(".wrap").fadeIn();
}

