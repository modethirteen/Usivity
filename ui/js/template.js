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
					if (typeof pointer !="undefined")
					{
						pointer = pointer[arr[i]];
					}
					else
					{
						xpath = "";	
					}
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
			
			// IF THERE WAS A FOREACH, THEN REPLACE JUST THE FOREACH CONTENT
			if (fullhtml)
			{
				// TODO:  CLOSE OFF THE MARKUP FOR THE FOREACH  {foreach}  {/foreach}
				returnhtml = fullhtml.replace(new RegExp('\{foreach([^\n]*\n+)+foreach\}', "g"),returnhtml);
				returnhtml = replacevariable(returnhtml,objectref); // TODO:  THIS NEEDS TO ONLY APPLY TO THE "REMAINING HTML" NOT THE FULL HTML
			}
			
			// REPLACE ALL REMAINING (UNMAPPED/UNWRITTERN) VARIABLES WTIH ""
			returnhtml = returnhtml.replace(new RegExp('\{(.*?)\}', "g"),"");
			
			//
			
			callback(returnhtml);
		}
	});
}

function replacevariable(templatehtml,objectref) 
{
	if (typeof objectref !="undefined")
	{
		// LOCATE CONDITIONAL (IF) STATEMENTS  {if:xxx}    {/if}
		statements = templatehtml.match(new RegExp('\{if:(.*?)\}', "g"));
		if (statements)
		{
			$.each(statements, function(key,value)
			{
				// ISOLATE THE CONDITIONAL VARIABLE AND CHECK TO SEE IF IT EXISTS
				var val = value.replace("{if:","");
				var val = val.replace("}","");
				
				// TODO: GET CODE TO LOOP THROUGH NESTED VARIABLES REFERENCES				
				
				//  OBJECT 	= NONE
				//  STATE   = NEGATIVE
				if( (typeof objectref[val] == "undefined" && val.indexOf("!") == -1) )
				{
					templatehtml = templatehtml.replace(new RegExp('\{if:' + val + '([^\n]*\n+)+\{if\}', "g"),"");
				}
				
				//  OBJECT 	= EXISTS
				//  STATE   = NEGATIVE
				if ( (typeof objectref[val.replace("!","")] != "undefined" && val.indexOf("!") == -1) )
				{
					//templatehtml = templatehtml.replace(new RegExp('\{if:' + val + '([^\n]*\n+)+\{if\}', "g"),"");
					templatehtml = templatehtml.replace('{if:' + val + '}',"");
					templatehtml = templatehtml.replace('{if}',"");
				} 
				
				//  OBJECT 	= EXISTS
				//  STATE   = POSITIVE
				if ( (typeof objectref[val.replace("!","")] != "undefined" && val.indexOf("!") >= 0) )
				{
					templatehtml = templatehtml.replace(new RegExp('\{if:' + val + '([^\n]*\n+)+\{!if\}', "g"),"");
				}  
				
			});	
			
			// REMOVE ALL OF THE {if:}  TAGS
			var templatehtml = templatehtml.replace(new RegExp('\{if:(.*?)\}', "g"),"");
			var templatehtml = templatehtml.replace("{if}","");
		}

		
		// REPLACE ALL TEMPLATE VARIABLES
		var matches = templatehtml.match(new RegExp('\{(.*?)\}', "g"));
		if (matches)
		{
			$.each(matches, function(key, value)
			{
			
				// Remove brackets {} from template variable
				var val = value.substring(1,value.length-1);  // TRY TO GET RID OF .LENGTH.  USE .EACH
				
				
				// CHECK IF THE TEMPLATE VARIABLE IS NESTED (_)
				if (val.indexOf("_") > 0)  // TODO:  CHOOSE A BETTER SEPARATOR
				{
					var arr = val.split("_");
					
					if(typeof objectref[arr[0]] != "undefined")
					{
						var pointer = objectref[arr[0]];
						arr.splice(0,1);
					
						// LOOP THROUGH NESTED OBJECTS TO FIND THE TEMPLATED VALUE
						for (i=0;i<=arr.length-1;i++)  // TODO:  TRY TO GET RID OF .LENGTH.  USE .EACH
						{
							if (typeof pointer !="undefined")
							{
								pointer = pointer[arr[i]];
							}
						}
						
						// REPLACE THE VARIABLE WITH THE OBJECT VALUE (pointer)
						if(typeof pointer != "undefined")
						{
							templatehtml = templatehtml.replace(value, pointer);
						}
					}
				}
				else
				{
					// REPLACE THE NON-NESTED VARIABLES
					if(typeof objectref[val] != "undefined")
					{
						templatehtml = templatehtml.replace(value, objectref[val]);
					}
				}
			});
		}
		return templatehtml;
	}
	return "";
}