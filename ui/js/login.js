/*
	login.js - format and handle generica form interactions
	TODO: MOVE THIS FILE TO SEPARATE DIRECTORY.  THIS FILE LIVES ON THE API FOR SECURE AUTHENTICATION
	FUNCTIONS
		-
		-
		-
*/

$(document).ready( function() {
	
	// POST AUTHENTICATION
	$(".login").live("submit", function() {
		
		form = $(this);
		var input = $(this).find(".user");
		var user = $(this).find(".user").val();
		var pass = $(this).find(".password").val();
		
		process(form);
		
		var apiurl = ("/api/1/users/authentication"); // TODO:  CHANGE TO USE APIURI()

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
				loaddata();
				closeModal();
			},
			error: function (xhr, ajaxOptions, thrownError){
				error(form);
				errormessage(form, "Incorrect username or password");
				console.log(xhr.statusText);
				error(input,xhr.statusText);
			} 
		});
		
		return false;	
	});
		
});

