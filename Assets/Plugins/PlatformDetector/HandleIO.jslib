var HandleIO = {
    IsMobilePlatform : function()
    {
		//return Module.SystemInfo.mobile;
        var userAgent = navigator.userAgent;
        isMobile = (
                    /\b(BlackBerry|webOS|iPhone|IEMobile)\b/i.test(userAgent) ||
                    /\b(Android|Windows Phone|iPad|iPod)\b/i.test(userAgent) ||
                    // iPad on iOS 13 detection
                    (userAgent.includes("Mac") && "ontouchend" in document)
                );
        return isMobile;
    }
};
mergeInto(LibraryManager.library, HandleIO);