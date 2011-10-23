/*
	search.js - conduct application wide search - client side, no server side yet
	
	FUNCTIONS
		- 
		-
*/
$(document).ready(function() {

	$(window).resize(function() {
		resize();
	});
	$(window).load(function() {
		resize();
	});
	
	/*SEARCH*/
	$(".search input").keyup( function() {
		var q = $(this).val();
		$(".highlight").each(function(){
			$(this).replaceWith( $(this).text() ); 
		});
		$(".streams .message_text").each( function() {
			highlight(q,$(this));
		});	
		$(".contacts .contact_name").each( function() {
			highlight(q,$(this));
		});
	});
});

function highlight(q,ele,parent) 
{
	
	if (q == "") 
	{
		$(".parent").show();
	}
	else
	{
		var content = ele.html().replace(new RegExp( '(' + q + ')', 'gi'), '<span class="highlight">$1</span>');
		ele.html(content);
		ele.show();
		$(".streams .parent").hide();
		$(".contacts .parent").hide();
		$(".parent .highlight").parents(".parent").show();	
	}
}