/*
	template.js - combine HTML and a javascript object
	
	FUNCTIONS
		- 
		
*/


function template(templatehtml,objecturi,xpath,callback)// TODO: CHANGE NAME FROM XPATH TO SOMETHING ELSE
{
	// GET THE DYNAMIC CALL BACK (dcb)
	var dcb = queryparam("dream.out.pre",objecturi);
	
	// IF THERE IS A FOREACH STATEMENT, TARGET JUST THE FOREACH TEXT
	var fhtml = templatehtml.match(new RegExp('\{foreach([^\n]*\n+)+foreach\}', "g"));
	if (fhtml)
	{
		// SET THE FULL TEMPLATE HTML
		var fullhtml = templatehtml;
		
		// REPLACE THE TEMPLATE HTML WITH THE HTML FROM WITHIN THE {FOREACH...FOREACH}
		var templatehtml = fhtml[0].replace("{foreach","");	
		var templatehtml = templatehtml.replace("foreach}","");	
	}
	
	// IF THERE IS AN IN STATEMENT, USE THAT AS THE XPATH
	var fxpath = templatehtml.match(new RegExp('\{in:(.*?)\}', "g"));
	if (fxpath)
	{
		// LOAD THE XPATH FROM THE TEMPLATE HTML	
		var xpath = fxpath[0];
		var xpath = xpath.replace("{in:","");
		var xpath = xpath.substring(0,xpath.length-1);
		var templatehtml = templatehtml.replace(new RegExp('\{in:(.*?)\}', "g"),"");
	}
	
	$.ajax({
		crossDomain:true, 
		url: objecturi,
		dataType: 'jsonp',
		jsonpCallback: dcb,
		jsonpCallback: dcb,
		mimeType: 'application/json',
		contentType: 'application/json;',
		success: function(objectref) // TODO:  Change ObjectRef to a better name
		{
			
			// Load the data from the object with XPATH
			if (xpath.indexOf("_") > 0)  // TODO:  CHOOSE A BETTER SEPARATOR
			{
				var arr = xpath.split("_");
				var pointer = objectref[arr[0]];
				arr.splice(0,1);
				
				// TODO: REPLACE WITH $.EACH
				for (i=0;i<=arr.length-1;i++)  // TRY TO GET RID OF .LENGTH, CAUSED JS PROBLEMS
				{
					pointer = pointer[arr[i]];
				}
				obj = pointer;
			}
			else if (xpath != null)
			{
				obj =  objectref[xpath];
			}		
			else
			{
				obj = objectref;	
			}
			
			
			// REPLACE THE TEMPLATE HTML WITH OBJECT VARIABLES
			var returnhtml = "";
			
			if (obj && obj.length > 0)
			{
				$.each(obj, function(key,value){
					returnhtml = returnhtml + replacevariable(templatehtml,value);
				});	
			}
			else
			{
				returnhtml = returnhtml + replacevariable(templatehtml,obj);
			}
			
			// IF THERE WAS A FOREACH, THEN REPLACE THE JUST THE FOREACH CONTENT
			if (fullhtml)
			{
				returnhtml = fullhtml.replace(new RegExp('\{foreach([^\n]*\n+)+foreach\}', "g"),returnhtml);
				returnhtml = replacevariable(returnhtml,objectref); // TODO:  THIS NEEDS TO ONLY APPLY TO THE "REMAINING HTML" NOT THE FULL HTML
			}
			
			callback(returnhtml);
		}
	});
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