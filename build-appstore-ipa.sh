#!/bin/bash

#security unlock-keychain -p ${!KEYCHAIN_PASSWORD}

cd build

xcodebuild \
    -project Unity-iPhone.xcodeproj \
    -scheme Unity-iPhone \
    archive \
    -archivePath build \
    CODE_SIGN_STYLE="Manual" \
    PROVISIONING_PROFILE_SPECIFIER="Balloon iOS Distribution" \
    CODE_SIGN_IDENTITY="iPhone Distribution: GEOYEOB KIM (TG9MHV97AH)"

xcodebuild \
    -exportArchive \
    -exportOptionsPlist ../exportoptions-appstore.plist \
    -archivePath "build.xcarchive" \
    -exportPath "build"

