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
				"dream.out.format" : "json"
			};
				
			var uri = apiuri("/connections/" + source + "/token",connectionparams);
			
			$.get(uri, function(results) {	
				var url = results["uri.authorize"];
				location.href = url	
			});
			return false;
		}
	});
	
	// CREATE NEW EMAIL CONNECTION
	$(".connection_details_email").live("submit", function() {
		form		= $(this);
		var username 	= form.find(".connection_email_username").val();
		var host		= form.find(".connection_host").val();
		var name		= form.find(".connection_name").val();
		var password 	= form.find(".connection_password").val();
		var port 		= form.find(".connection_port").val();
		var ssl			= form.find(".connection_ssl").val();
		
		process(form);
		// VALIDATE REQUIRED FIELDS
		form.find(".required").removeClass("errortext");
		required = form.find(".required:text[value=''],.required:password[value='']");
		if (required.size() >= 1)
		{
			required.addClass("errortext");
			error(form);
			return false;
		}
		
		// VALIDATE THE EMAIL ADDRESS
		if (username == "" || !checkemail(username))
		{
			form.find(".connection_email_username").addClass("errortext");
			error(form);
			return false;
		}
		
		// CREATE THE DATA OBJECT
		data = {
		    connection:{
	            host: host,
	            username: username,
	            password: password,
	            port: port,
	            ssl:  ssl
		    }
		}
	
		// CREATE THE API URI
		connectionparams = {
			"dream.out.format" : "json"
		};
				
		var uri = apiuri("/connections/email",connectionparams);
		
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: JSON.stringify(data),
			url: uri,
			dataType: "json",
			mimeType: 'application/json',
			contentType: 'application/json',
			success: function(results)
			{				
				success(form);
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
				error(form);
			}   
		});
		return false;
	});
});