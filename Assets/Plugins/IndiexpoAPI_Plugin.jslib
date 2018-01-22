mergeInto(LibraryManager.library, {

  ShowMessage: function (str) 
	{

    window.alert(Pointer_stringify(str));
	},


  UploadScore: function (s)
	{

		if (window.IndiexpoAPI)		
		{
		
				IndiexpoAPI.sendScore(s).done(function(result) {});
		}
		else
		{

				window.alert ("Please, login to indiexpo to send your score");		
		}		

	}, 

  });