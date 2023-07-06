var HandleIO_3 = {
	IsAndroidPlatform : function()
    {
		//return Module.SystemInfo.mobile;
        var userAgent = navigator.userAgent;
        isAndroid = (
                    /\b(BlackBerry|IEMobile)\b/i.test(userAgent) ||
                    /\b(Android|Windows Phone)\b/i.test(userAgent)
                );
        return isAndroid;
    }
};
mergeInto(LibraryManager.library, HandleIO_3);