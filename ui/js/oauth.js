/*
	oauth.js - format and handle generica form interactions
	
	FUNCTIONS
		-
		-
		-
*/
$(document).ready(function() {

	var authtoken		= queryparam("oauth_token");
	
	if (authtoken)
	{
		var authverifier	= queryparam("oauth_verifier");
		var connection 		= queryparam("connection");
		
		data = {
			    connection: {
				    oauth : {
			        	token		: authtoken,
			        	verifier 	: authverifier
		        	}
			    }
			}
		
		var apiuri = ("/api/1/connections/" + connection + "?dream.in.verb=PUT&dream.out.format=json");
		
		
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: JSON.stringify(data),
			url: apiuri,
			dataType: "json",
			mimeType: 'application/json',
			contentType: 'application/json',
			success: function(results)
			{
				console.log("connection saved");
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			}   
		});
	}
	
});
