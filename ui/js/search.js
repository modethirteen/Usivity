/*
	search.js - conduct application wide search - client side, no server side yet
	
	FUNCTIONS
		- 
		-
*/
$(document).ready(function() {

	/*SEARCH*/
	$(".search input").keyup( function() {
		var q = $(this).val();
		
		// Get Rid of Existing Highlighted Search Terms
		$(".search_highlight").each(function(){
			$(this).replaceWith( $(this).text() ); 
		});
		
		if (q == "") 
		{
			$(".target tbody tr").show();
		}
		else if (q.length >= 3)
		{		
			// Loop through the messages and look for matching search terms
			$(".streams .message_text").each( function() {
				highlight(q,$(this));
			});
			
			$(".contacts .contact_name").each( function() {
				highlight(q,$(this));
			});
		}
	
	});
});

function highlight(q,ele,parent) 
{
	var content = ele.html().replace(new RegExp( '(' + q + ')', 'gi'), '<span class="search_highlight">$1</span>');
	ele.html(content);
	ele.show();
	$(".target tbody tr").hide();
	$(".search_highlight").parents("tbody tr").show();	
}