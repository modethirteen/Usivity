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
	
	// POST AUTHENTICATION
	$(".login").live("submit", function() {
		var user = $(this).find(".user").val();
		var pass = $(this).find(".password").val();
		
		var apiurl = ("http://usivity.com/api/usivity/users/authentication");

		$.ajax({
			type: "GET",
			crossDomain:true,
			url: apiurl,
			beforeSend : function(xhr) {
  				var hash = $.base64Encode(user + ':' + pass);
				xhr.setRequestHeader("Authorization", "Basic " + hash);
			},
			success: function(results, status, xhr)
			{
				document.cookie = results;
				window.location.href = "http://usivity.com/index.htm";
			},
			error: function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			} 
		});
		
		return false;	
	});
	
	// MENU
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

