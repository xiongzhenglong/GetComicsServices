using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawerEnum
{
    public enum DownChapter
    {
        待处理链接,
        处理完链接,
        上传完图片
    }

    public enum Source
    {
        QQ=1,
        dongmanmanhua
    }

    public enum ErrComic
    {
        解析出错,
        图片出错
    }

    public enum ErrChapter
    {
        解析出错,
        图片出错
    }

    public enum ErrPage
    {
        限制访问
    }

    public enum NoticeType
    {
        章节更新,
        目录变更
    }

    public enum NoticeStatus
    {
        等待处理=1,
        等待发送,
        已发送
    }
}
