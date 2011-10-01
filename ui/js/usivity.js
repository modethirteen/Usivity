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
	
	/*MINIMIZE/MAXIMIZE MESSAGE*/
	$(".message .minimize").live("click", function() {
		$(this).parents(".message").find(".message_text").slideToggle('fast', function() {
    
		});
		return false;
	});	
	
	/*Delete Message*/
	$(".message_delete").live("submit", function() {
		
		var id = $(this).attr("id");
		var apiaction = "?dream.in.verb=DELETE";
		var apiurl = (apiroot + apiopenstream + "/" + id + apiaction);
		
		console.log(apiurl);
		
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: apiurl,
			success: function(results)
			{
				closeModal();	
			}
		});
		
		
		$("#" + id).parents(".parent").remove();
		
		return false;
	})
	
	/*My Connections*/
	$(".myconnections a").live("click", function() {
		var width = 500;
    	var height = 500;
    	var left = parseInt((screen.availWidth/2) - (width/2));
    	var top = parseInt((screen.availHeight/2) - (height/2));
		var url = $(this).attr('href');
		
		window.open(url,'mywin','left=' + left + ',top=' + top + ',width=' + width + ',height=' + height + ',toolbar=1,resizable=0');
		return false;	
	});
	
	/*Form Input Formatting*/
	$("textarea").wrap('<div class="twrap">');
	
	/*Section Expand / Collapse*/
	$(".stitle").live("click", function() {
		$(this).next(".section").slideToggle();	
		$(this).toggleClass("contracted");	
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

function resize() 
{
	var bheight = $(window).height(); // Body Height
	var cheight = $(".header").outerHeight() // Height of Controls
	var colheight = (bheight - cheight - 5); // Column Height
	
	/*Wrapper Div*/
	$(".col").css("height",colheight);
	
	/*Height*/
	$(".mystream").css("height",(colheight - 15));
	$(".openstream").css("height",(colheight - 15));
	$(".contacts").css("height",(colheight - 15));
	
	$(".fwrap").fadeIn();
}

function fixmessage(text) {
	
	// Convert URL to HREF
	var text = text.replace(/(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/ig,'<a class="external" target="_new" href="$1">$1</a>'); 
	var text = text.replace(/(^|\s)@(\w+)/g, '$1<a class="profile" href="http://www.twitter.com/$2">$2</a>');
	return text;
}