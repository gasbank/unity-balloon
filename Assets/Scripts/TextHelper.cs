using System.Collections.Generic;

public class TextHelper
{
    public static List<string> availableLanguage = new List<string>();
    public static string currentLanguage;

    public static string GetText(string stringId, params object[] values)
    {
        return string.Format(GetTextLocalized(stringId), values);
    }

    static string GetTextLocalized(string stringId)
    {
        switch (stringId)
        {
            case "platform_saving":
                return "\\클라우드 저장 중...".Localized();
            case "platform_save_confirm_popup"
                : // 인자 여섯 개 쓰임: 저장된 계정 레벨, 저장된 계정 경험치, 저장된 보석, 불러올 데이터 작성 시각, 현재 계정 레벨, 현재 계정 경험치, 현재 보석, 현재 시각
                return "\\클라우드 저장을 진행하시겠습니까?".Localized();
            case "platform_save_cancelled_popup":
                return "\\클라우드 저장이 취소되었습니다.".Localized();
            case "platform_saved_popup":
                return "\\클라우드 저장이 완료됐습니다.".Localized();
            case "platform_loading":
                return "\\클라우드 불러오기 중...".Localized();
            case "platform_load_confirm_popup"
                : // 인자 여섯 개 쓰임: 저장된 계정 레벨, 저장된 계정 경험치, 저장된 보석, 불러올 데이터 작성 시각, 현재 계정 레벨, 현재 계정 경험치, 현재 보석, 현재 시각
                return "\\클라우드 불러오기를 진행하시겠습니까?".Localized();
            case "platform_load_cancelled_popup":
                return "\\클라우드 불러오기가 취소되었습니다.".Localized();
            case "platform_game_center_login_required_popup":
                return "\\홈 > 설정 > Game Center 로그인을 먼저 진행 해 주십시오.".Localized();
            case "platform_load_require_internet_popup":
                return "\\클라우드 기능은 인터넷이 연결된 상태에서 가능합니다.".Localized();
            case "platform_logging_in":
                return "\\로그인 중...".Localized();
            case "platform_account_game_center":
                return "\\Game Center".Localized();
            case "platform_account_google":
                return "\\Google Play".Localized();
            case "platform_google_login_required_popup":
                return "\\Google Play 로그인이 먼저 필요합니다.".Localized();
            case "platform_login_failed_popup":
                return "\\로그인을 실패했습니다.".Localized();
            case "platform_cloud_save_fail":
                return "\\클라우드 저장에 실패했습니다.".Localized();
            case "platform_cloud_load_fail":
                return "\\클라우드 불러오기를 실패했습니다.".Localized();
            case "platform_load_confirm_popup_rollback_alert":
                return "\\<color=red>경고: 불러오려는 데이터보다 현재 플레이 중인 것이 더 많이 진행된 상태입니다!</color>".Localized();
            case "platform_save_confirm_popup_rollback_alert":
                return "\\<color=red>경고: 현재 플레이 중인 것보다 덮어쓰려는 데이터가 더 많이 진행되었습니다!</color>".Localized();
        }

        return stringId;
    }
}