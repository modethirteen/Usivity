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
		var user = $(this).find(".user");
		var pass = $(this).find(".pass");
		
		var apiurl = ("http://api.usivity.com/usivity/users/authentication");
		
		$.ajax({
			type: "GET",
			crossDomain:true,
			url: apiurl,
			beforeSend : function(xhr) {
  				var hash = $.base64Encode(user + ':' + pass);
				xhr.setRequestHeader("Authorization", "Basic " + "ZGFtaWVuaDpmb28=");
			},
			success: function(results)
			{
				console.log("authenticated dude");
				console.log(results);
			},
			error:function (xhr, ajaxOptions, thrownError){
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

