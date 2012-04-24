$(document).ready( function() 
{
	// RESIZE THE LAYOUT
	resize();	

	openstreamparams = {
		"dream.out.format" : "json"
	};
	var objecturi = apiuri(api.current,openstreamparams);
	
		
	// CHECK FOR AUTHENTICATION
	$.ajax({
		type: "GET",
		crossDomain:true, 
		url: objecturi,
		dataType: 'json',
		jsonp: false,
		jsonpCallback: 'callback',
		mimeType: 'application/json',
		contentType: 'application/json;',
		success: function(data) {
			
			// IF THE USER IS AUTHENTICATED, LOAD THE STREAM AND CONTACT PANEL
			loaddata();	
		},
        error:function (xhr, ajaxOptions, thrownError){	        
	        errorMessage(xhr.status);
        }
	});
});
