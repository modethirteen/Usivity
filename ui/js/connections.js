/*
	connections.js - handle all connections to SOURCES such as twitter, facebook, email, etc...
	
	FUNCTIONS
		- GET
		- POST
		- PUT
		- DELETE
		
	CUSTOM FUNCTIONS
		- LOAD AUTHENTICATION
*/
$(document).ready(function() {
	
	// DELETE CONNECTION
	$(".connections .delete").live("click", function() {
		var link	= $(this);
		var href	= link.attr("href");
		
		deleteparams = {
			"dream.in.verb" : "DELETE"
		};
		var uri = apiuri(href,deleteparams);
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: uri,
			success: function(results)
			{
				link.parents("tr").remove(); 		
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			} 
		});
		
		return false;	
	});	

	
	
	// CREATE NEW CONNECTION
	$(".connections_new a").live("click", function() {
		var link	= $(this);
		var source = link.attr("id");
		
		if (source)
		{
			connectionparams = {
				"dream.out.format" : "json",
				"source" : source
			};
				
			var uri = apiuri(api.sources,connectionparams);
			
			$.ajax({
				type: "POST",
				crossDomain:true,
				url: uri,
				dataType: "json",
				mimeType: 'application/json',
				contentType: 'application/json',
				success: function(results)
				{
					var url = results["uri.authorize"];
					location.href = url	
				},
				error:function (xhr, ajaxOptions, thrownError){
					console.log(xhr.statusText);
					return false;
				}   
			});
			return false;
		}
	});
	
	// CREATE NEW EMAIL CONNECTION
	$(".connection_details_email").live("submit", function() {
		var email 		= $(this).find(".connection_email_address");
		var host		= $(this).find(".connection_host");
		var name		= $(this).find(".connection_name");
		var password 	= $(this).find(".connection_password");
		
		// VALIDATE THE EMAIL ADDRESS
		if (email.val() == "" || !checkemail(email.val()))
		{
			error(email,"A valid email address is required to create a new connection.");
			return false;
		}
		
		// CREATE THE DATA OBJECT
		data = {
			    email: email.val()
			}
			
		// CREATE THE API URI
		connectionparams = {
			"dream.out.format" : "json",
			"source" : "email"
		};
				
		var uri = apiuri(api.sources,connectionparams);
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
		return false;
	});
});