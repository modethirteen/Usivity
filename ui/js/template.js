function maketemplate(html, datastore) {

	var matches = html.match(new RegExp('\{(.*?)\}', "g"));
	
	
	if (matches)
	{
		$.each(matches, function(key, value){
		
			// Remove brackets {} from template variable
			var val = value.substring(1,value.length-1);
			
			if (val.indexOf(".") > 0)
			{
				var arr = val.split(".");
				var pointer = datastore[arr[0]];
				arr.splice(0,1);
				
				for (i=0;i<=arr.length-1;i++)
				{
					pointer = pointer[arr[i]];
				}
				
				html = html.replace(value, pointer);
			}
			else
			{
				html = html.replace(value, datastore[val]);
			}	
		});
	}
	 
	return html;
}
