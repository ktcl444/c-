/*----------------------------------------------------------------
// Copyright (C) 2007 蚂蚁互动 版权所有。 
//
// 文件名：ILoginRequest.cs
// 文件功能描述：
// 
// 创建标识：jillzhang
// 修改标识：
// 修改描述：
//----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SSO
{
    public interface ILoginRequest
    {
        string Identity { get;}
        DateTime TimeStamp { get;}
    }
}
