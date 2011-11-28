/*
	forms.js - format and handle generica form interactions
	
	FUNCTIONS
		-
		-
		-
*/
$(document).ready(function() {

	// FORM SUBMIT
	$("form").live("submit", function() {
		//TODO:  CREATE GENERIC BEHAVIOR AS DESIRED
		return false;
	});	
	
	// FORM INPUT FORMATTING
	$("textarea").wrap('<div class="twrap">');
	
	// SETUP "START" FIELDS
	startset();
	
	// CHANGE "START" FIELDS ON CLICK
	$("[start]").live("focus", function() {
		var input	= $(this);
		var start 	= input.attr("start");
		var val		= input.val();
		
		if (val == start)
		{
			input.val("");
			input.removeClass("start");
			input.focus();	
		}
		if (val != start && val != "")
		{
			input.removeClass("start");
		}
	});
	
	// CHANGE "START" FIELDS ON BLUR
	$("[start]").live("blur", function() {
		var input	= $(this);
		var start 	= input.attr("start");
		var val		= input.val();
		
		if (val == "")
		{
			input.val(start);
			input.addClass("start");
		}
	});
});
// SET ANY FIELDS WITH A "START" VALUE
function startset()
{
	$("[start]").each( function() {
		var start 	= $(this).attr("start");
		var val		= $(this).val();
		
		if (val == "")
		{
			$(this).addClass("start");
			$(this).val(start);
		}
	});	
}

// CLEAR "START" FIELDS ON FORM SUBMIT
function clearstart(form)
{
	form.find("[start]").each( function() {
		var start 	= $(this).attr("start");
		var val		= $(this).val();
		
		if (val == start)
		{
			$(this).val("");	
		}
	});
}
