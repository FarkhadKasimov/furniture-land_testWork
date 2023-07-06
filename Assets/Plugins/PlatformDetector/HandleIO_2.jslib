var HandleIO_2 = {
	IsIOSPlatform : function()
    {
		//return Module.SystemInfo.mobile;
        var userAgent = navigator.userAgent;
        isIOS = (
                    /\b(iPhone)\b/i.test(userAgent) ||
                    /\b(iPad|iPod)\b/i.test(userAgent) ||
                    // iPad on iOS 13 detection
                    (userAgent.includes("Mac") && "ontouchend" in document)
                );
        return isIOS;
    }
};
mergeInto(LibraryManager.library, HandleIO_2);