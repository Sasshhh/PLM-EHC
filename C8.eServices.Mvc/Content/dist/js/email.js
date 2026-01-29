(function ($) {

	$('#toggleCheckboxes').on('click', function(){
    	$('.msg-row').toggleClass('check-active');
    	$('.checkbox-active-tools').toggleClass('active');
    });

    // Check and Uncheck All checkboxes in the active inbox item
	$('#chkAll').on('change', function(){
		
		var msg = $('.msg-row'),
			ch = msg.find('input[type="checkbox"]:not(.msg-star)');

		if(this.checked) {
			ch.each(function(){				
				ch.prop('checked', true);
				$(this).closest($('.msg-row')).addClass('selected');
			});
		} else {
			ch.each(function(){
				ch.prop('checked', false);
				$(this).closest($('.msg-row')).removeClass('selected');
			});
		}
		
	});

	$('.msg-wrapper').on('change', 'input[type="checkbox"]:not(.msg-star)', function(){		
		var msg = $(this).closest($('.msg-row'));

		if(this.checked) {
			msg.addClass('selected');
		} else {
			msg.removeClass('selected');
		}		
	});

	var options = {
	    valueNames: [ 'msg-from' ] 
	};

	var emailList = new List('emailList', options);

})(jQuery);