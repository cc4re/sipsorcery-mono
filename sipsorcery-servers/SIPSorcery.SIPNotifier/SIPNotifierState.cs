﻿// ============================================================================
// FileName: SIPNotifierState.cs
//
// Description:
// Application configuration for a SIP Notifier Server.
//
// Author(s):
// Aaron Clauson
//
// History:
// 22 Feb 2010  Aaron Clauson	Created.
//
// License: 
// This software is licensed under the BSD License http://www.opensource.org/licenses/bsd-license.php
//
// Copyright (c) 2010 Aaron Clauson (aaron@sipsorcery.com), SIPSorcery Ltd, London, UK (www.sipsorcery.com)
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that 
// the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following 
// disclaimer in the documentation and/or other materials provided with the distribution. Neither the name of Blue Face Ltd. 
// nor the names of its contributors may be used to endorse or promote products derived from this software without specific 
// prior written permission. 
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, 
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// POSSIBILITY OF SUCH DAMAGE.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using log4net;
using SIPSorcery.SIP;
using SIPSorcery.Sys;

namespace SIPSorcery.SIPNotifier
{
    /// <summary>
    /// Retrieves application conifguration settings from App.Config.
    /// </summary>
    public class SIPNotifierState
    {
        private const string LOGGER_NAME = "sipnotifier";

        public const string SIPNOTIFIER_CONFIGNODE_NAME = "sipnotifier";

        private const string SIPSOCKETS_CONFIGNODE_NAME = "sipsockets";
        private const string MONITOR_LOOPBACK_PORT_KEY = "MonitorLoopbackPort";
        private const string OUTBOUND_PROXY_KEY = "OutboundProxy";
        private const string MONITOR_EVENT_RECEIVE_SOCKET = "MonitorEventReceiveSocket";

        public static ILog logger;

        private static readonly XmlNode m_sipNotifierNode;
        public static readonly XmlNode SIPNotifierSocketsNode;
        public static readonly int MonitorLoopbackPort;
        public static readonly SIPEndPoint OutboundProxy;
        public static readonly string MonitorEventReceiveSocket;

        static SIPNotifierState()
        {
            try
            {
                #region Configure logging.

                try
                {
                    log4net.Config.XmlConfigurator.Configure();
                    logger = log4net.LogManager.GetLogger(LOGGER_NAME);
                }
                catch (Exception logExcp)
                {
                    Console.WriteLine("Exception SIPNotifierState Configure Logging. " + logExcp.Message);
                }

                #endregion

                if (AppState.GetSection(SIPNOTIFIER_CONFIGNODE_NAME) != null)
                {
                    m_sipNotifierNode = (XmlNode)AppState.GetSection(SIPNOTIFIER_CONFIGNODE_NAME);
                }

                if (m_sipNotifierNode == null)
                {
                    logger.Warn("The SIP Notifier " + SIPNOTIFIER_CONFIGNODE_NAME + " config node was not available, the agent will not be able to start.");
                }
                else
                {
                    SIPNotifierSocketsNode = m_sipNotifierNode.SelectSingleNode(SIPSOCKETS_CONFIGNODE_NAME);
                    if (SIPNotifierSocketsNode == null)
                    {
                        throw new ApplicationException("The SIP Notifier could not be started, no " + SIPSOCKETS_CONFIGNODE_NAME + " node could be found.");
                    }

                    Int32.TryParse(AppState.GetConfigNodeValue(m_sipNotifierNode, MONITOR_LOOPBACK_PORT_KEY), out MonitorLoopbackPort);
                    if (!AppState.GetConfigNodeValue(m_sipNotifierNode, OUTBOUND_PROXY_KEY).IsNullOrBlank())
                    {
                        OutboundProxy = SIPEndPoint.ParseSIPEndPoint(AppState.GetConfigNodeValue(m_sipNotifierNode, OUTBOUND_PROXY_KEY));
                    }

                    MonitorEventReceiveSocket = AppState.GetConfigNodeValue(m_sipNotifierNode, MONITOR_EVENT_RECEIVE_SOCKET);
                }
            }
            catch (Exception excp)
            {
                logger.Error("Exception SIPNotifierState. " + excp.Message);
                throw;
            }
        }
    }
}
