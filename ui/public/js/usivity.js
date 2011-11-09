
$(document).ready( function() {
	
	// Load Tabs
	var activetab 		= $(".tabs li:first");
	var activetarget	= activetab.attr("alt");
	
	$(".tabtarget").hide();
	$(".tabs li").removeClass("active");
	$(activetarget).show();
	activetab.addClass("active");
	
	//  Toggle Tabs
	$(".tabs li").hover( function() {
		var target = $(this).attr("alt");
		$(".tabtarget").hide();
		$(".tabs li").removeClass("active");
		$(target).show();
		$(this).addClass("active");
	});	
});