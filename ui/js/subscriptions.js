/*
	subscriptions.js - manage all stream subscriptions.  Example - words that are searched on twitter
	
	FUNCTIONS
		- Delete Subscriptions
		- Post Subscriptions
*/

$(document).ready(function() {
	
	// DELETE SUBSCRIPTION
	$(".subscriptions .delete").live("click", function() {
		var link	= $(this);
		var href	= link.attr("href");
		
		deleteparams = {
			"dream.in.verb" : "DELETE"
		};
		var uri = apiuri(href,deleteparams);
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: uri,
			success: function(results)
			{
				link.parents("tr").remove(); 		
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			} 
		});
		
		return false;	
	});	
	
	
	/*POST SUBSCRIPTIONS*/
	$(".subscriptions form").live("submit", function() {
		var form 		= $(this);
		var uri 		= form.attr("action");
		var input		= form.find(".constraints");
		var constraints = input.val();
		
		data = {
		    subscription:{
		        constraints:{
		            constraint:[
		                constraints
		            ]
		        },
		        language:'en'
		    }
		}
		
		subscriptionparams = {
			"dream.out.format" : "json"
		};
		
		var uri = apiuri(uri,subscriptionparams);
		
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: JSON.stringify(data),
			url: uri,
			dataType: "json",
			mimeType: 'application/json',
			contentType: 'application/json',
			success: function(results)
			{
				input.val("");
				input.focus("");
				var newele = $(document.createElement('tr'));
				newele.html('<td>' + results.constraints + '</td><td class="center"><a href="' + results['@href'] + '" class="delete">delete</a></td>');
				$(".subscriptions table tbody").append(newele);
				
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			}   
		});
		
		return false;	
	});
});