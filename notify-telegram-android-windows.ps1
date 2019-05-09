$BOT_TOKEN=$env:BOT_TOKEN

$BUILD_TYPE='AndroidWindows'

$DOWNLOAD_URL_RESULT_ARMEABI_V7A=$(Invoke-WebRequest -s -XGET https://api.yyt.life/d/balloon/android-armeabi-v7a?count=1)
$DOWNLOAD_URL_RESULT_ARM64_V8A=$(Invoke-WebRequest -s -XGET https://api.yyt.life/d/balloon/android-arm64-v8a?count=1)
$DOWNLOAD_URL_RESULT_X86=$(Invoke-WebRequest -s -XGET https://api.yyt.life/d/balloon/android-x86?count=1)

# *****************
$DOWNLOAD_URL_ARMEABI_V7A=$(Write-Output $DOWNLOAD_URL_RESULT_ARMEABI_V7A.content | ConvertFrom-Json).versions[0]
$DOWNLOAD_URL_ARM64_V8A=$(Write-Output $DOWNLOAD_URL_RESULT_ARM64_V8A.content | ConvertFrom-Json).versions[0]
$DOWNLOAD_URL_X86=$(Write-Output $DOWNLOAD_URL_RESULT_X86.content | ConvertFrom-Json).versions[0]
# *****************
$BUILD_NUMBER=$env:BUILD_NUMBER #'%build.number%'

$HEADERS = @{
    'Content-Type' = 'text/plain'
}

$MESSAGE = "Build Number: $BUILD_NUMBER\nBuild Type: $BUILD_TYPE\nDownload: <a href='$DOWNLOAD_URL_ARMEABI_V7A'>ARMEABI_V7A</a> / <a href='$DOWNLOAD_URL_ARM64_V8A'>ARM64_V8A</a> / <a href='$DOWNLOAD_URL_X86'>X86</a>"

Write-Output $MESSAGE

Invoke-WebRequest -Method POST -Headers $HEADERS -Body $MESSAGE -Uri "https://plusalpha.top/balloon-telegram/$BOT_TOKEN/teamcity-build-event"
