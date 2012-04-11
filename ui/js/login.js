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
		
		var user = $(this).find(".user").val();
		var pass = $(this).find(".password").val();
		
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
				console.log(xhr.statusText);
				//TODO:  CREATE FUNCTION SERVERERROR(error#) that processes all server errors, consolidate with applications erros function
				// SAME TODO IS IN authenticate.js
			} 
		});
		
		return false;	
	});
		
});
