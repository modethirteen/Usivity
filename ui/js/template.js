/*
	template.js - combine HTML and a javascript object
	
	FUNCTIONS
		- preparedata(templatehtml, objecturl, callback)
		- injecttemplate (templatehtml,objectref)
		- replacevariable(templatehtml,objectref)
		
*/

function preparedata(templatehtml, objecturi, callback) {
	/*
	templatehtml	- Template HTML
	objectref		- JSONP Object Reference stored in the data library //TODO:  STORE OBJECT REQUESTS BY URL SO A URL CAN ALSO BE A REFERENCE.  HAVE PREPAREDATA() CONDUCT THE CHECK
	objecturl 		- URL of API to retrieve the JSONP Object
	*/

	// RETURN ERROR IF NO URI PROVIDED
	if (!objecturi)
	{
		return false
	};
	
	// TODO:  CHECK IF URI DATA IS ALREADY STORED
	
	// LOAD THE DATA FROM THE OBJECTURL
	$.ajax({
		crossDomain:true, 
		url: objecturi,
		dataType: 'jsonp',
		jsonp: false,
		jsonpCallback: 'callback',
		mimeType: 'application/json',
		contentType: 'application/json;',
		success: function(json)
		{
			// STORE HTML AND OBJECT IN AN OBJECT
			usivity.data[objecturi] = {};
			usivity.data[objecturi].json	 = json;
			usivity.data[objecturi].templatehtml = templatehtml;
			
			
			// LOOP THROUGH FOREACH VARIABLE STATEMENTS
			var foreachhtml = templatehtml.match(new RegExp('\{foreach([^\n]*\n+)+foreach\}', "g"));
			
			if (foreachhtml)
			{
				var replacehtml = foreach(foreachhtml[0],json);
				templatehtml = templatehtml.replace(foreachhtml[0],replacehtml);
			}
			
			// LOOP THROUGH SINGLE VARIABLES
			templatehtml = replacevariable(templatehtml,json);

			callback (templatehtml);
		}
	});	

}

function foreach (foreachhtml,json) 
{	
		// Remove foreach statements from template variable
		var loop = foreachhtml.replace("{foreach","");
		var loop = loop.replace("foreach}","");
		var loopmarkup = "";  //TODO: RENAME BETTER OR ADD VAR
		
		// Find the object (Strip away the foreach markup)
		var matches = loop.match(new RegExp('\{in:(.*?)\}', "g"));
		var val = matches[0];
		var val = val.replace("{in:","");
		var val = val.substring(0,val.length-1);
		
		// Clean the content of all template markup
		var content = loop.replace(new RegExp('\{in:(.*?)\}', "g"),"");
		
		// Load the data from the object
		if (val.indexOf("_") > 0)  // TODO:  CHOOSE A BETTER SEPARATOR
		{
			var arr = val.split("_");
			var pointer = json[arr[0]];
			arr.splice(0,1);
			
			// TODO: REPLACE WITH $.EACH
			for (i=0;i<=arr.length-1;i++)  // TRY TO GET RID OF .LENGTH, CAUSED JS PROBLEMS
			{
				pointer = pointer[arr[i]];
			}
			obj = pointer;
		}
		else
		{
			obj =  json[val];
		}	
		
		if (obj)
		{
			// Check for the correct formatting of the object
			if (obj.length)
			{
				// Loop through each occurance of the object to templatize
				$.each(obj, function(key,value){
					loopmarkup = loopmarkup + replacevariable(content, value);
				});	
			}
			else
			{
				loopmarkup = loopmarkup + replacevariable(content, obj);
			}
		}	
	
		// REMOVE THE {FOREACH FOREACH} STATEMENTS
		foreachhtml = foreachhtml.replace(new RegExp('\{foreach([^\n]*\n+)+foreach\}', "g"),loopmarkup);
			
		return foreachhtml;
}

function replacevariable(templatehtml,objectref) 
{
	// REPLACE ALL TEMPLATE VARIABLES
	var matches = templatehtml.match(new RegExp('\{(.*?)\}', "g"));
	
	if (matches)
	{
		$.each(matches, function(key, value){
		
			// Remove brackets {} from template variable
			var val = value.substring(1,value.length-1);  // TRY TO GET RID OF .LENGTH, CAUSED JS PROBLEMS
			if (val.indexOf("_") > 0)  // TODO:  CHOOSE A BETTER SEPARATOR
			{
				var arr = val.split("_");
				var pointer = objectref[arr[0]];
				arr.splice(0,1);
				
				for (i=0;i<=arr.length-1;i++)  // TRY TO GET RID OF .LENGTH, CAUSED JS PROBLEMS
				{
					pointer = pointer[arr[i]];
				}
				
				templatehtml = templatehtml.replace(value, pointer);
			}
			else
			{
				templatehtml = templatehtml.replace(value, objectref[val]);
			}	
		});
	}
	 
	return templatehtml;		
}
