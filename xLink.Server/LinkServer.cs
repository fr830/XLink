﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NewLife;
using NewLife.Log;
using NewLife.Net;
using NewLife.Reflection;
using NewLife.Remoting;

namespace xLink
{
    /// <summary>物联服务器</summary>
    public class LinkServer : ApiServer
    {
        #region 属性
        /// <summary>附加参数</summary>
        public IDictionary<String, Object> Parameters { get; set; } = new Dictionary<String, Object>();
        #endregion

        #region 构造
        /// <summary>实例化令牌服务器</summary>
        public LinkServer()
        {
            Log = XTrace.Log;

            StatPeriod = 60;
            ShowError = true;

#if DEBUG
            EncoderLog = XTrace.Log;
            StatPeriod = 10;
#endif

            // 初始数据
            var dic = Parameters;
            dic["OS"] = Environment.OSVersion + "";
            dic["Machine"] = Environment.MachineName;
            dic["Agent"] = Environment.UserName;
            dic["ProcessID"] = Process.GetCurrentProcess().Id;

            var asmx = AssemblyX.Entry;
            dic["Version"] = asmx?.Version;
            dic["Compile"] = asmx?.Compile;
        }
        #endregion

        #region 方法
        /// <summary>启动</summary>
        public override void Start()
        {
            var svr = EnsureCreate();
            svr.Log = Log;

            var dic = Parameters;
            dic["Type"] = Name;

            base.Start();
        }
        #endregion

        #region 过期清理
        /// <summary>删除过期会话</summary>
        /// <param name="ids">会话ID</param>
        /// <returns></returns>
        public Int32 ClearExpire(Int32[] ids)
        {
            if (ids.Length == 0) return 0;

            if (!(Server is NetServer svr)) return 0;

            foreach (var item in ids)
            {
                var ss = svr.GetSession(item);
                if (ss != null) ss.TryDispose();
            }

            return ids.Length;
        }
        #endregion

        #region 辅助
        /// <summary>设置内部日志是否开启</summary>
        /// <param name="session">是否开启会话级日志</param>
        /// <param name="socket">是否开启Socket级日志</param>
        /// <param name="encoder">是否显示编码日志</param>
        public void SetLog(Boolean session, Boolean socket, Boolean encoder = false)
        {
            if (!(Server is NetServer svr)) return;

            // 置空可以让其使用当前Log日志
            svr.SessionLog = session ? null : Logger.Null;
            svr.SocketLog = socket ? null : Logger.Null;

            //if (Encoder != null) Encoder.Log = Log;
            if (encoder) EncoderLog = Log;
        }
        #endregion
    }
}