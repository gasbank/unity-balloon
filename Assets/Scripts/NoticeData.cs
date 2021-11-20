using System;

[Serializable]
public class NoticeData
{
    public string detailUrl; // 자세히 보기 URL (옵션; 주로 공식 카페 URL)
    public string text; // 공지 본문
    public string title; // 공지 제목 (일반적으로 '공지사항')
}