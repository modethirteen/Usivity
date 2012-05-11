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
	});
});