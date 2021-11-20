using System;
using System.Numerics;
using UnityEngine.Events;

public class CloudMetadata
{
    public static readonly CloudMetadata Invalid = new CloudMetadata
        {level = -1, levelExp = -1, gem = -1, riceRate = -1, saveDate = -1};

    public BigInteger gem;
    public int level;
    public int levelExp;
    public BigInteger riceRate;
    public long saveDate;
}

public interface IPlatformBase
{
    // 마지막 로그인 시도가 실패했다면 true 반환, 아니면 false
    bool LoginFailedLastTime();

    // 로그인 전 초기화 필요한 작업
    void PreAuthenticate();

    // 로그인 시작, 결과는 비동기 함수 호출로 반환
    void Login(Action<bool, string> onAuthResult);

    // 로그아웃
    void Logout();

    // 클라우드 기능을 쓸 수 있는 상황이라면 true 반환, 쓸 수 없는 상황이라면 팝업으로 물어보고 진행, 아니면 false
    bool CheckLoadSavePrecondition(string progressMessage, UnityAction onNotLoggedIn, UnityAction onAbort);

    // 클라우드에 저장된 데이터의 정원 레벨과 경험치를 조회하여 비동기 함수 호출로 반환
    void
        GetCloudLastSavedMetadataAsync(
            Action<CloudMetadata> onPeekResult); // ACCOUNT_LEVEL_KEY, ACCOUNT_LEVEL_EXP_KEY, ACCOUNT_GEM_KEY, ACCOUNT_RICE_RATE_KEY, SAVE_DATE_KEY

    // 클라우드 로드 실행 (모든 전제조건 확보되었다고 가정)
    void ExecuteCloudLoad();

    // 클라우드 저장 실행 (모든 전제조건 확보되었다고 가정)
    void ExecuteCloudSave();

    // 버그 제보
    void Report(string reportPopupTitle, string mailTo, string subject, string text, byte[] saveData);

    // 스크린샷 공유
    void ShareScreenshot(byte[] pngData);

    // 모든 로컬 알림 등록
    void RegisterAllNotifications(string title, string body, string largeIcon);

    // 테스트용 로컬 알림 (1회) 등록
    void RegisterSingleNotification(string title, string body, int afterMs, string largeIcon);

    // 모든 로컬 알림 삭제
    void ClearAllNotifications();

    // 클라우드 저장 API 호출 결과 핸들링
    void OnCloudSaveResult(string result);

    // 클라우드 로드 API 호출 결과 핸들링
    void OnCloudLoadResult(string result, byte[] data);

    // 유저 별점 요청하는 버튼 눌렀을 때
    void RequestUserReview();

    // 계정 종류 이름 ('구글', 'Game Center', ...)
    string GetAccountTypeText();
}