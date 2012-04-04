$(document).ready(function() {
	
	openstreamparams = {
		"stream" : "open",
		"dream.out.format" : "json",
		"dream.out.pre": cb()
	};
	var objecturi = apiuri(usivity.openstream.url,openstreamparams);
	
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
        error:function (xhr, ajaxOptions, thrownError){	        
	        //TODO:  CREATE FUNCTION SERVERERROR(error#) that processes all server errors, consolidate with applications erros function
	        
            if (xhr.status == "403")
            {
	            buildModal("","/template/login.htm");
            }
            if (xhr.status == "503")
            {
	         	buildModal("","/error/error_unavailable.htm   ");
            }
        }
	});
});