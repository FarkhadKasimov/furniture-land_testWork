PlatformDetector.IsMobile() - возвращает true если это телефон.

PlatformDetector.IsIOSMobile() - вернет true только если это IOS
PlatformDetector.IsAndroidMobile() - вернет true только если это Андроид

Пример определения мобильного устройства
if (PlatformDetector.IsMobile()) MobileControls.SetActive(true);

Значение кэшируются так что можно смело вызывать в апдейте