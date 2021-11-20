#!/bin/bash

#security unlock-keychain -p ${!KEYCHAIN_PASSWORD}

cd build

pod install

xcodebuild \
    -workspace Unity-iPhone.xcworkspace \
    -scheme Unity-iPhone \
    archive \
    -archivePath build \
    PROVISIONING_PROFILE_SPECIFIER="Balloon App Store" \
    CODE_SIGN_IDENTITY="Apple Distribution: GEOYEOB KIM (TG9MHV97AH)"

xcodebuild \
    -exportArchive \
    -exportOptionsPlist ../exportoptions-appstore.plist \
    -archivePath "build.xcarchive" \
    -exportPath "build"

