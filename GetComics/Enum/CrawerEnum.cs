﻿using System;
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
        上传完图片,
        处理中,
        无法处理=99
 
    }

    public enum Source
    {
        QQ=1,
        dongmanmanhua,
        U17,
        Zymk,
        Manhuatai,
        _163,
        dmzj,
        Icartoons,
        mh160=99
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

    public enum PageState
    {
        None,
        失败=1,
        成功

        

    }
}
