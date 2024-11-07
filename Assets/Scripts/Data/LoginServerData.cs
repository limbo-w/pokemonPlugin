using System;

[Serializable]
public class LoginRegisterResult
{
    public int code;
    public string msg;
    public string token;
}


//注册请求
[Serializable]
public class LoginRegisterReq
{
    public string userName;
    public string qq;
    public string openId;
}