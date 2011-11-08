/*
	template.js - combine HTML and a javascript object
	
	FUNCTIONS
		- 
		-
		
*/

//TODO:  	RENAME OBJECT to OBJECTREF.  ADD PARAMTER FOR OBJECT TO PASS IN AN OBJECT, NOT JUST A REFERENCE
function preparedata(markup, object, objecturl, callback) {
	/*
	markup 		- Template HTML
	object 		- JSONP Object Reference stored in the data library
	objecturl 	- URL of API to retrieve the JSONP Object
	*/

	/*IF A URL IS PROVIDED, LOAD THE DATA FROM THE URL SOURCE*/
	if (objecturl)
	{
		var uri = apiuri(objecturl);// TODO, GET RID OF THIS
		$.ajax({
			crossDomain:true, 
			url: objecturl,
			dataType: 'jsonp',
			jsonp: false,
			jsonpCallback: 'callback',
			mimeType: 'application/json',
			contentType: 'application/json;',
			success: function(results)
			{
				var returnhtml = injecttemplate(markup,results);
				callback(returnhtml);
			}
		});	
	}
	else 
	{
		return injecttemplate(markup,object);
	}
}

function injecttemplate (markup,object) {
	// TODO:  CREATE RESOURCE LIBRARY FOR GLOBAL VARIABLES  // TODO:  ADD SUPPORT FOR SETTING VARIABLES FROM SETTINGS.JS
	
	
	// LOOK THROUGH ALL FOREACH STATEMENTS
	var foreach = markup.match(new RegExp('\{foreach([^\n]*\n+)+foreach\}', "g"));
	if (foreach)
	{
		$.each(foreach, function(key, value){
			// Remove foreach statements from template variable
 			var loop = value.replace("{foreach","");
 			var loop = loop.replace("foreach}","");
 			loopmarkup = "";  //TODO: RENAME BETTER OR ADD VAR
 			
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
				var pointer = object[arr[0]];
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
				obj =  object[val];
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
			markup = markup.replace(new RegExp('\{foreach([^\n]*\n+)+foreach\}', "g"),loopmarkup);
			
		});
	}
	markup = replacevariable(markup,object);	
	
	return markup;	
}

function replacevariable(markup,object) {
	// REPLACE ALL TEMPLATE VARIABLES
	var matches = markup.match(new RegExp('\{(.*?)\}', "g"));
	if (matches)
	{
		$.each(matches, function(key, value){
		
			// Remove brackets {} from template variable
			var val = value.substring(1,value.length-1);  // TRY TO GET RID OF .LENGTH, CAUSED JS PROBLEMS
			if (val.indexOf("_") > 0)  // TODO:  CHOOSE A BETTER SEPARATOR
			{
				var arr = val.split("_");
				var pointer = object[arr[0]];
				arr.splice(0,1);
				
				for (i=0;i<=arr.length-1;i++)  // TRY TO GET RID OF .LENGTH, CAUSED JS PROBLEMS
				{
					pointer = pointer[arr[i]];
				}
				
				markup = markup.replace(value, pointer);
			}
			else
			{
				markup = markup.replace(value, object[val]);
			}	
		});
	}
	 
	return markup;		
}
