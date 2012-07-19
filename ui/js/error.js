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
		$(this).find(".buttons").prepend('<div class="responseicon"></div>');	
	});
}

// ADD THE PROCESSING ICON TO THE FORM
function process(form)
{
	var respicon = form.find(".responseicon");
	respicon.removeClass("responsesuccess");
	respicon.addClass("responseprocessing");
	respicon.show();
}

// PROCESS ERROR
function error(input, message)
{
	// TODO: change from message popup to icon display (much cleaner) and add hover over popup on error icon
	var form = input.parents("form");
	var response = form.prev(".response");
	response.html(message);
	response.addClass("error");
	response.removeClass("success");
	response.slideDown(50);
}

// ADD THE SUCCESS ICON TO THE FORM
function success(form)
{
	var respicon = form.find(".responseicon");
	respicon.removeClass("responseprocessing");
	respicon.addClass("responsesuccess");
	respicon.delay(2000).fadeOut();
	
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