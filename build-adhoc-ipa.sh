#!/bin/bash

#security unlock-keychain -p ${!KEYCHAIN_PASSWORD}

cd build

xcodebuild \
    -workspace Unity-iPhone.xcworkspace \
    -scheme Unity-iPhone \
    archive \
    -archivePath build \
    ENABLE_BITCODE="NO"

xcodebuild \
    -exportArchive \
    -exportOptionsPlist ../exportoptions.plist \
    -archivePath "build.xcarchive" \
    -exportPath "build"
