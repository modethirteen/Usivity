/*
	forms.js - format and handle generica form interactions
	
	FUNCTIONS
		-
		-
		-
*/
$(document).ready(function() {

	/*FORM SUBMIT*/
	$("form").live("submit", function() {
		//TODO:  CREATE GENERIC BEHAVIOR AS DESIRED
		return false;
	});	
	
	/*FORM INPUT FORMATTING*/
	$("textarea").wrap('<div class="twrap">');
});