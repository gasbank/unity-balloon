#import <CloudKit/CloudKit.h>
#import <GameKit/GameKit.h>
#import <UserNotifications/UserNotifications.h>
#import <MessageUI/MessageUI.h>

// MailComposeDelegate : 개발자에게 문의 메일 보내는 View의 실행 결과를 처리할 수 있는 Delegate
@interface MailComposeDelegate : NSObject<MFMailComposeViewControllerDelegate>
@end

// 메일 View가 생기는 시점부터 View가 닫힐 때까지 살아 있어야 하기 때문에 전역으로 뺐는데,
// 메모리가 샐 수도 있는데, 문의 메일 보내는 시도 한번 할 때마다만 발생하는 이벤트이므로
// 지금은 그대로 둔다.
static MailComposeDelegate* mailComposeDelegate;

@interface MailComposeDelegate()
@end
@implementation MailComposeDelegate
- (void)mailComposeController:(MFMailComposeViewController *)controller
          didFinishWithResult:(MFMailComposeResult)result error:(NSError *)error {
    // Check the result or perform other tasks.
    [UnityGetGLViewController() dismissViewControllerAnimated:YES completion:nil];
}
@end

// [C# API]
// 앞으로 예약된 로컬 알림과 현재 쌓여 있는 로컬 알림을 모두 지우는 함수이다.
// (Unity에 내장된 기능으로는 iOS 10 이후에는 작동하지 않는 것 같아 만들었음)
void clearAllNotifications() {
    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    [center removeAllDeliveredNotifications];
    [center removeAllPendingNotificationRequests];
}

// [C# API]
// 개발자에게 문의 메일 보내기
void sendMail(const char* title, const char* body, const char* recipient, const char* attachment) {
    NSString *emailTitle = [NSString stringWithUTF8String:title];
    NSString *messageBody = [NSString stringWithUTF8String:body];
    NSString *recipientStr = [NSString stringWithUTF8String:recipient];
    NSString *attachmentStr = [NSString stringWithUTF8String:attachment];
    NSArray *toRecipents = [NSArray arrayWithObject:recipientStr];
    NSData *attachmentData = [NSData dataWithContentsOfFile: attachmentStr];
    
    MFMailComposeViewController *mc = [[MFMailComposeViewController alloc] init];
    if (mc) {
        mailComposeDelegate = [[MailComposeDelegate alloc] init];
        mc.mailComposeDelegate = mailComposeDelegate;
        [mc setSubject:emailTitle];
        [mc setMessageBody:messageBody isHTML:NO];
        [mc addAttachmentData:attachmentData mimeType:@"application/octet-stream" fileName:@"default-remote-save"];
        
        [mc setToRecipients:toRecipents];
        
        [UnityGetGLViewController() presentViewController:mc animated:YES completion:NULL];
        
    } else {
        NSLog(@"mc nil...");
    }
}

typedef void(iCloudFunction)(NSString*, NSString*);

// iCloud 사용 가능한 상태라면 fn(str1, str2)를 호출하고, 그렇지 않으면 fn(null, null)을 호출하는 래퍼 함수
static void checkIcloudLoginStateAndDo(iCloudFunction fn, NSString* str1, NSString* str2, NSString* loginErrorTitle, NSString* loginErrorMessage, NSString* confirmMessage) {
    [[CKContainer defaultContainer] accountStatusWithCompletionHandler:^(CKAccountStatus accountStatus, NSError *error) {
        if (accountStatus == CKAccountStatusNoAccount) {
            UIAlertController *alert = [UIAlertController alertControllerWithTitle:loginErrorTitle
                                                                           message:loginErrorMessage
                                                                    preferredStyle:UIAlertControllerStyleAlert];
            [alert addAction:[UIAlertAction actionWithTitle:confirmMessage
                                                      style:UIAlertActionStyleCancel
                                                    handler:nil]];
            [UnityGetGLViewController() presentViewController:alert animated:YES completion:nil];
            fn(0, 0);
        }
        else
        {
            // Insert your just-in-time schema code here
            fn(str1, str2);
        }
    }];
}

static const char* unitySidePlatformCallbackHandlerName = "PlatformCallbackHandler";
static const char* unitySideOnSaveResultName = "OnIosSaveResult";
static const char* unitySideOnLoadResultName = "OnIosLoadResult";

// 클라우드에 저장하기 (iCloud 로그인 되어 있다고 가정)
// 처리는 비동기로 진행되며, 완료 시 'Platform' GameObject의 OnIosSaveResult()를 호출한다.
static void saveToCloudPrivateDo(NSString* playerID, NSString* data) {
    if (playerID == nil || data == nil) {
        UnitySendMessage(unitySidePlatformCallbackHandlerName, unitySideOnSaveResultName, "FAILED");
        return;
    }
    CKDatabase *privateDatabase = [[CKContainer defaultContainer] privateCloudDatabase];
    CKRecordID *savedGameRecordID = [[CKRecordID alloc] initWithRecordName:playerID];
    [privateDatabase fetchRecordWithID:savedGameRecordID completionHandler:^(CKRecord *savedGameRecord, NSError *error) {
        if (error) {
            // Error handling for failed fetch from public database
            NSLog(@"saveToCloudPrivate: Fetching failed. Try to create a new save game record...");
            savedGameRecord = [[CKRecord alloc] initWithRecordType:@"SavedGame" recordID:savedGameRecordID];
        }
        savedGameRecord[@"data"] = data;
        [privateDatabase saveRecord:savedGameRecord completionHandler:^(CKRecord *savedRecord, NSError *saveError) {
            // Error handling for failed save to public database
            if (saveError) {
                NSLog(@"Save failed!!!!!");
                UnitySendMessage(unitySidePlatformCallbackHandlerName, unitySideOnSaveResultName, [[saveError description] UTF8String]);
                return;
            }
            NSLog(@"Save ok");
            UnitySendMessage(unitySidePlatformCallbackHandlerName, unitySideOnSaveResultName, "OK");
        }];
    }];
}

// [C# API]
// 클라우드에 저장하기
void saveToCloudPrivate(const char* playerID, const char* data, const char* loginErrorTitle, const char* loginErrorMessage, const char* confirmMessage) {
    NSString *savedGameRecordIDStr = [NSString stringWithUTF8String:playerID];
    NSString *dataStr = [NSString stringWithUTF8String:data];
    NSString *loginErrorTitleStr = [NSString stringWithUTF8String:loginErrorTitle];
    NSString *loginErrorMessageStr = [NSString stringWithUTF8String:loginErrorMessage];
    NSString *confirmMessageStr = [NSString stringWithUTF8String:confirmMessage];
    checkIcloudLoginStateAndDo(saveToCloudPrivateDo, savedGameRecordIDStr, dataStr, loginErrorTitleStr, loginErrorMessageStr, confirmMessageStr);
}

// 클라우드에서 로드하기 (iCloud 로그인 되어 있다고 가정)
// 처리는 비동기로 진행되며, 완료 시 'Platform' GameObject의 OnIosLoadResult()를 호출한다.
static void loadFromCloudPrivateDo(NSString* playerID, NSString* _) {
    if (playerID == nil) {
        UnitySendMessage(unitySidePlatformCallbackHandlerName, unitySideOnLoadResultName, "*****ERROR***** NO PLAYER ID");
        return;
    }
    CKDatabase *privateDatabase = [[CKContainer defaultContainer] privateCloudDatabase];
    CKRecordID *savedGameRecordID = [[CKRecordID alloc] initWithRecordName:playerID];
    [privateDatabase fetchRecordWithID:savedGameRecordID completionHandler:^(CKRecord *savedGameRecord, NSError *error) {
        if (error) {
            if (error.code == CKErrorUnknownItem) {
                // (클라우드 저장을 한 적이 없어서) 불러오기를 실패한 경우는
                // 실패로 치지 말고 '데이터 없음'으로 치도록 하자.
                // Unity C#쪽에서 에러로 분류하지 않도록 만들기 위해
                // "*****ERROR***** "를 보내지 않는다.
                UnitySendMessage(unitySidePlatformCallbackHandlerName, unitySideOnLoadResultName, "");
            } else {
                // Error handling for failed fetch from public database
                NSLog(@"loadFromCloudPrivate: Fetching failed. No save data exist...");
                NSString* errorDesc = [NSString stringWithFormat:@"*****ERROR***** %@", [error description]];
                UnitySendMessage(unitySidePlatformCallbackHandlerName, unitySideOnLoadResultName, [errorDesc UTF8String]);
            }
            return;
        }
        NSString* data = savedGameRecord[@"data"];
        NSLog(@"Load OK: %@", data);
        if (data) {
            UnitySendMessage(unitySidePlatformCallbackHandlerName, unitySideOnLoadResultName, [data UTF8String]);
        } else {
            UnitySendMessage(unitySidePlatformCallbackHandlerName, unitySideOnLoadResultName, "");
        }
    }];
}

// [C# API]
// 클라우드에서 로드하기
void loadFromCloudPrivate(const char* playerID, const char* loginErrorTitle, const char* loginErrorMessage, const char* confirmMessage) {
    NSString *savedGameRecordIDStr = [NSString stringWithUTF8String:playerID];
    NSString *loginErrorTitleStr = [NSString stringWithUTF8String:loginErrorTitle];
    NSString *loginErrorMessageStr = [NSString stringWithUTF8String:loginErrorMessage];
    NSString *confirmMessageStr = [NSString stringWithUTF8String:confirmMessage];
    checkIcloudLoginStateAndDo(loadFromCloudPrivateDo, savedGameRecordIDStr, 0, loginErrorTitleStr, loginErrorMessageStr, confirmMessageStr);
}
