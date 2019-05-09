param (
    [Parameter(Mandatory=$true)][string]$ServiceName,
    [Parameter(Mandatory=$true)][string]$PlatformName,
    [Parameter(Mandatory=$true)][string]$PackageNamespace,
    [Parameter(Mandatory=$true)][string]$InputFile
)

# hello android life.yyt.hello build/Hello.apk

$YYT_DIST_SERVICE_NAME="$ServiceName"
$YYT_DIST_PLATFORM_NAME="$PlatformName"
$YYT_DIST_PACKAGE_NAMESPACE="$PackageNamespace"
$YYT_DIST_INPUT_FILE="$InputFile"
$FILE_EXTENSION="apk"

if (-Not "$env:YYT_DIST_AUTH_TOKEN") {
  Write-Output "No auth token for yyt.life"
  exit 1
}

Write-Output $FILE_EXTENSION
Write-Output $env:YYT_DIST_AUTH_TOKEN

if (-Not "$env:APP_VERSION") {
  $env:APP_VERSION="1.0.0"
}

if (-Not "$env:BUILD_NUMBER") {
  $env:BUILD_NUMBER="0"
}

$DEPLOY_NAME="$YYT_DIST_PACKAGE_NAMESPACE-$env:APP_VERSION.$env:BUILD_NUMBER.$FILE_EXTENSION"

Write-Output $DEPLOY_NAME

$HEADERS = @{
    'X-Auth-Token' = "$env:YYT_DIST_AUTH_TOKEN"
}

$DOWNLOAD_URL_QUERY_URL = "https://api.yyt.life/d/$YYT_DIST_SERVICE_NAME/$YYT_DIST_PLATFORM_NAME/$DEPLOY_NAME"

Write-Output $DOWNLOAD_URL_QUERY_URL

#$UPLOAD_URL = (Invoke-RestMethod -Uri $DOWNLOAD_URL_QUERY_URL -Method Put -Headers $HEADERS)
$UPLOAD_URL = (curl.exe -s -XPUT $DOWNLOAD_URL_QUERY_URL -H "X-Auth-Token: $env:YYT_DIST_AUTH_TOKEN")

Write-Output $UPLOAD_URL

#Invoke-RestMethod -Uri $UPLOAD_URL -Method Put -InFile $YYT_DIST_INPUT_FILE
curl.exe -s -XPUT $UPLOAD_URL --upload-file $YYT_DIST_INPUT_FILE
