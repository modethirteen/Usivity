/*
	response.js - handle all of the error/success response messages
	
	FUNCTIONS
		-
		-
		-
*/


function errorMessage (errno)
{
	var errmap = {
		503 : "/error/error_unavailable.htm",
		400 : "/error/error_404.htm",
		403 : "/template/login.htm"
	}
	
	if (errmap[errno])
	{
		buildModal("",errmap[errno]);	
	}
	return false;
}

$(document).ready( function() {
	
	// ADD ERROR/SUCCESS MESSAGE TO FORMS
	setupresponse();	
});


// ADD ERROR/SUCCESS MESSAGE TO FORMS
function setupresponse() {
	
	$("form").each( function() {
		$(this).before('<div class="response"></div>');	
	});
}

// PROCESS ERROR
function error(input, message)
{
	var form = input.parents("form");
	var response = form.prev(".response");
	response.html(message);
	response.addClass("error");
	response.removeClass("success");
	response.slideDown(50);
}

// PROCESS SUCCESS
function success(input, message)
{
	var form = input.parents("form");
	var response = form.prev(".response");
	response.html(message);
	response.addClass("success");
	response.removeClass("error");
	response.slideDown(50);
}

// PROCESS SUCCESS
function hint(input, message)
{
	var form = input.parents("form");
	var response = form.prev(".response");
	response.html(message);
	response.removeClass("success");
	response.removeClass("error");
	response.slideDown(50);
}