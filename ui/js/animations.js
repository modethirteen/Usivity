/*
	animations.js - manage generic animations
	
	FUNCTIONS
		- sectionresize - 
		- resize - 
*/
$(document).ready(function() {	
	/*Section Expand / Collapse*/
	$(".stitle").live("click", function() {
		$(this).next(".section").slideToggle();	
		$(this).toggleClass("contracted");	
	});
});

function resize() 
{
	var bheight = $(window).height(); // Body Height
	var cheight = $(".header").outerHeight() // Height of Controls
	var colheight = (bheight - cheight - 5); // Column Height
	
	/*Wrapper Div*/
	$(".col").css("height",colheight);
	
	/*Height*/
	$(".mystream").css("height",(colheight - 15));
	$(".openstream").css("height",(colheight - 15));
	$(".contacts").css("height",(colheight - 15));
	
	$(".fwrap").fadeIn();
}