/*
	contacts.js - manage all contact interactions  
	
	FUNCTIONS
		-
		-
*/
$(document).ready( function() {
	
	// DELETE A CONTACT
	$(".contact_delete").live("submit", function() {
		var href 	= $(this).attr("id");
		var id		= $(this).find(".id").val();
		
		deleteparams = {
			"dream.in.verb" : "DELETE"
		};
		
		var deleteuri = apiuri(href,deleteparams);
		
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: deleteuri,
			success: function(results)
			{		
				$("#" + id).slideUp();
				closeModal();	
			}
		});
		
		return false;	
	});
	
	
	// POST A NEW CONTACT
// 	{
//     user:{
//         firstname:'foo',
//         lastname:'bar'
//     }
// 	}
});

