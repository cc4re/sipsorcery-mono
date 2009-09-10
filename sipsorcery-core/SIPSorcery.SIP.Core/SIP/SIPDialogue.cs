//-----------------------------------------------------------------------------
// Filename: SIPDialogue.cs
//
// Description: Base class for SIP dialogues. 
// 
// History:
// 20 Oct 2005	Aaron Clauson	Created.
//
// License: 
// This software is licensed under the BSD License http://www.opensource.org/licenses/bsd-license.php
//
// Copyright (c) 2006 Aaron Clauson (aaronc@blueface.ie), Blue Face Ltd, Dublin, Ireland (www.blueface.ie)
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
//-----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using SIPSorcery.Sys;
using log4net;

namespace SIPSorcery.SIP {
    public enum SIPDialogueStateEnum {
        Unknown = 0,
        Early = 1,
        Confirmed = 2,
    }

    /// <summary>
    /// See "Chapter 12 Dialogs" in RFC3261.
    /// </summary>
    /// <remarks>
    /// The standard states that there are two independent CSeq's for a dialogue: one for requests from the UAC and for requests
    /// from the UAS. In practice it's been noted that is a UAS (initial UAS) sends an in-dialogue request with a CSeq less than the
    /// UAC's CSeq it can cause problems. To avoid this issue when generating requests the remote CSeq is always used.
    /// </remarks>
    public class SIPDialogue {
        protected static ILog logger = AssemblyState.logger;

        protected static string m_CRLF = SIPConstants.CRLF;
        protected static string m_sipVersion = SIPConstants.SIP_VERSION_STRING;

        //public SIPTransport m_sipTransport;

        public Guid Id { get; set; }                                // Id for persistence, NOT used for SIP call purposes.
        public string Owner { get; set; }                           // In cases where ownership needs to be set on the dialogue this value can be used. Does not have any effect on the operation of the dialogue and is for info only.
        public string AdminMemberId { get; set; }
        public string CallId { get; set; }
        public SIPRouteSet RouteSet { get; set; }
        public SIPUserField LocalUserField { get; set; }            // To header for a UAS, From header for a UAC.
        public string LocalTag { get; set; }
        public SIPUserField RemoteUserField { get; set; }           // To header for a UAC, From header for a UAS.    
        public string RemoteTag { get; set; }
        public int CSeq { get; set; }                               // CSeq being used by the remote UA for sending requests.
        public SIPURI RemoteTarget { get; set; }                    // This will be the Contact URI in the INVITE request or in the 2xx INVITE response and is where subsequent dialogue requests should be sent.
        public string DialogueId { get; set; }
        public Guid CDRId { get; set; }                             // Call detail record for call the dialogue belongs to.
        public string ContentType { get; private set; }             // The content type on the request or response that created this dialogue. This is not part of or required for the dialogue and is kept for info and consumer app. purposes only.
        public string SDP { get; private set; }                     // The sessions description protocol payload. This is not part of or required for the dialogue and is kept for info and consumer app. purposes only.
        public string RemoteSDP { get; private set; }               // The sessions description protocol payload from the remote end. This is not part of or required for the dialogue and is kept for info and consumer app. purposes only.
        public Guid BridgeId { get; set; }                          // If this dialogue gets bridged by a higher level application server the id for the bridge can be stored here.                   
        public int CallDurationLimit { get; set; }                  // If non-zero indicates the dialogue established should only be permitted to stay up for this many seconds.
        public DateTime Inserted { get; set; }

        public SIPDialogueStateEnum DialogueState = SIPDialogueStateEnum.Unknown;

        public SIPDialogue() { }

        public SIPDialogue(
            //SIPTransport sipTransport,
            string callId,
            SIPRouteSet routeSet,
            SIPUserField localUser,
            SIPUserField remoteUser,
            int cseq,
            SIPURI remoteTarget,
            string localTag,
            string remoteTag,
            Guid cdrId,
            string owner,
            string adminMemberId,
            string sdp,
            string remoteSDP) {
            //m_sipTransport = sipTransport;

            Id = Guid.NewGuid();
            DialogueId = GetDialogueId(callId, localTag, remoteTag);

            CallId = callId;
            RouteSet = routeSet;
            LocalUserField = localUser;
            LocalTag = localTag;
            RemoteUserField = remoteUser;
            RemoteTag = remoteTag;
            CSeq = cseq;
            RemoteTarget = remoteTarget;
            CDRId = cdrId;
            Owner = owner;
            AdminMemberId = adminMemberId;
            SDP = sdp;
            RemoteSDP = remoteSDP;
            Inserted = DateTime.Now;
        }

        /// <summary>
        /// This constructor is used by server user agents or SIP elements acting in a server user agent role. When
        /// acting as a server user agent the local fields are contained in the To header and the remote fields are 
        /// in the From header.
        /// </summary>
        public SIPDialogue(
            //SIPTransport sipTransport,
            UASInviteTransaction uasInviteTransaction,
            string owner,
            string adminMemberId) {
            //m_sipTransport = sipTransport;

            Id = Guid.NewGuid();

            CallId = uasInviteTransaction.TransactionRequest.Header.CallId;
            RouteSet = (uasInviteTransaction.TransactionFinalResponse != null && uasInviteTransaction.TransactionFinalResponse.Header.RecordRoutes != null) ? uasInviteTransaction.TransactionFinalResponse.Header.RecordRoutes.Reversed() : null;
            LocalUserField = uasInviteTransaction.TransactionFinalResponse.Header.To.ToUserField;
            LocalTag = uasInviteTransaction.TransactionFinalResponse.Header.To.ToTag;
            RemoteUserField = uasInviteTransaction.TransactionFinalResponse.Header.From.FromUserField;
            RemoteTag = uasInviteTransaction.TransactionFinalResponse.Header.From.FromTag;
            CSeq = uasInviteTransaction.TransactionRequest.Header.CSeq;
            CDRId = uasInviteTransaction.CDR.CDRId;
            Owner = owner;
            AdminMemberId = adminMemberId;
            ContentType = uasInviteTransaction.TransactionFinalResponse.Header.ContentType;
            SDP = uasInviteTransaction.TransactionFinalResponse.Body;
            RemoteSDP = uasInviteTransaction.TransactionRequest.Body;
            Inserted = DateTime.Now;

            DialogueId = GetDialogueId(CallId, LocalTag, RemoteTag);

            RemoteTarget = new SIPURI(uasInviteTransaction.TransactionRequest.URI.Scheme, SIPEndPoint.ParseSIPEndPoint(uasInviteTransaction.RemoteEndPoint.ToString()));
            if (uasInviteTransaction.TransactionRequest.Header.Contact != null && uasInviteTransaction.TransactionRequest.Header.Contact.Count > 0) {
                RemoteTarget = uasInviteTransaction.TransactionRequest.Header.Contact[0].ContactURI.CopyOf();
                if (!uasInviteTransaction.TransactionRequest.Header.ProxyReceivedFrom.IsNullOrBlank()) {
                    // Setting the Proxy-ReceivedOn header is how an upstream proxy will let an agent know it should mangle the contact. 
                    if (SIPTransport.IsPrivateAddress(RemoteTarget.Host)) {
                        SIPEndPoint remoteUASSIPEndPoint = SIPEndPoint.ParseSIPEndPoint(uasInviteTransaction.TransactionRequest.Header.ProxyReceivedFrom);
                        RemoteTarget.Host = remoteUASSIPEndPoint.SocketEndPoint.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// This constructor is used by client user agents or SIP elements acting in a client user agent role. When
        /// acting as a client user agent the local fields are contained in the From header and the remote fields are 
        /// in the To header.
        /// </summary>
        public SIPDialogue(
          //SIPTransport sipTransport,
          UACInviteTransaction uacInviteTransaction,
          string owner,
          string adminMemberId) {
            //m_sipTransport = sipTransport;

            Id = Guid.NewGuid();

            CallId = uacInviteTransaction.TransactionRequest.Header.CallId;
            RouteSet = (uacInviteTransaction.TransactionFinalResponse != null && uacInviteTransaction.TransactionFinalResponse.Header.RecordRoutes != null) ? uacInviteTransaction.TransactionFinalResponse.Header.RecordRoutes.Reversed() : null;
            LocalUserField = uacInviteTransaction.TransactionFinalResponse.Header.From.FromUserField;
            LocalTag = uacInviteTransaction.TransactionFinalResponse.Header.From.FromTag;
            RemoteUserField = uacInviteTransaction.TransactionFinalResponse.Header.To.ToUserField;
            RemoteTag = uacInviteTransaction.TransactionFinalResponse.Header.To.ToTag;
            CSeq = uacInviteTransaction.TransactionRequest.Header.CSeq;
            CDRId = uacInviteTransaction.CDR.CDRId;
            Owner = owner;
            AdminMemberId = adminMemberId;
            ContentType = uacInviteTransaction.TransactionRequest.Header.ContentType;
            SDP = uacInviteTransaction.TransactionRequest.Body;
            RemoteSDP = uacInviteTransaction.TransactionFinalResponse.Body;
            //OutboundProxy = uacInviteTransaction.OutboundProxy;
            Inserted = DateTime.Now;

            DialogueId = GetDialogueId(CallId, LocalTag, RemoteTag);

            // Set the dialogue remote target and take care of mangling if an upstream proxy has indicated it's required.
            RemoteTarget = new SIPURI(uacInviteTransaction.TransactionRequest.URI.Scheme, SIPEndPoint.ParseSIPEndPoint(uacInviteTransaction.RemoteEndPoint.ToString()));
            if (uacInviteTransaction.TransactionFinalResponse.Header.Contact != null && uacInviteTransaction.TransactionFinalResponse.Header.Contact.Count > 0) {
                RemoteTarget = uacInviteTransaction.TransactionFinalResponse.Header.Contact[0].ContactURI.CopyOf();
                if (!uacInviteTransaction.TransactionFinalResponse.Header.ProxyReceivedFrom.IsNullOrBlank()) {
                    // Setting the Proxy-ReceivedOn header is how an upstream proxy will let an agent know it should mangle the contact. 
                    if (SIPTransport.IsPrivateAddress(RemoteTarget.Host)) {
                        SIPEndPoint remoteUASSIPEndPoint = SIPEndPoint.ParseSIPEndPoint(uacInviteTransaction.TransactionFinalResponse.Header.ProxyReceivedFrom);
                        RemoteTarget.Host = remoteUASSIPEndPoint.SocketEndPoint.ToString();
                    }
                }
            }
        }

        public static string GetDialogueId(string callId, string localTag, string remoteTag) {
            return Crypto.GetSHAHashAsString(callId + localTag + remoteTag);
        }

        public static string GetDialogueId(SIPHeader sipHeader) {
            return Crypto.GetSHAHashAsString(sipHeader.CallId + sipHeader.To.ToTag + sipHeader.From.FromTag);
        }

        public void Hangup(SIPTransport sipTransport, SIPEndPoint outboundProxy) {
            try {
                SIPEndPoint localEndPoint = (outboundProxy != null) ? sipTransport.GetDefaultSIPEndPoint(outboundProxy.SIPProtocol) : sipTransport.GetDefaultSIPEndPoint(GetRemoteTargetProtocol());
                SIPRequest byeRequest = GetByeRequest(localEndPoint);
                SIPNonInviteTransaction byeTransaction = sipTransport.CreateNonInviteTransaction(byeRequest, sipTransport.GetRequestEndPoint(byeRequest, outboundProxy, true), localEndPoint, outboundProxy);
                byeTransaction.SendReliableRequest();
            }
            catch (Exception excp) {
                logger.Error("Exception SIPDialogue Hangup. " + excp.Message);
                throw;
            }
        }

       /* public void Hangup() {
            SIPEndPoint dstEndPoint = GetDestinationEndPoint();
            SIPEndPoint localSIPEndPoint = m_sipTransport.GetDefaultSIPEndPoint(dstEndPoint);
            if (localSIPEndPoint == null) {
                throw new ApplicationException("Could not locate an appropriate SIP transport channel in SIPDialogue Hangup for protocol " + dstEndPoint.SIPProtocol + ".");
            }
            SIPRequest byeRequest = GetByeRequest(this, localSIPEndPoint);
            SIPNonInviteTransaction byeTransaction = m_sipTransport.CreateNonInviteTransaction(byeRequest, dstEndPoint, localSIPEndPoint, OutboundProxy);
            byeTransaction.SendReliableRequest();
        }

        private SIPEndPoint GetDestinationEndPoint() {
            SIPEndPoint dstEndPoint = OutboundProxy;
            if (dstEndPoint == null) {
                dstEndPoint = (RouteSet == null) ? m_sipTransport.GetURIEndPoint(RemoteTarget, true) : m_sipTransport.GetURIEndPoint(RouteSet.TopRoute.URI, true);
            }

            if (dstEndPoint == null) {
                throw new ApplicationException("The destination SIP end point could not be resolved in SIPDialogue Hangup for a remote target of " + RemoteTarget + ".");
            }

            return dstEndPoint;
        }*/

        private SIPProtocolsEnum GetRemoteTargetProtocol() {
            SIPURI dstURI = (RouteSet == null) ? RemoteTarget : RouteSet.TopRoute.URI;
            return dstURI.Protocol;
        }

        private SIPRequest GetByeRequest(SIPEndPoint localSIPEndPoint) {
            SIPRequest byeRequest = new SIPRequest(SIPMethodsEnum.BYE, RemoteTarget);
            SIPFromHeader byeFromHeader = SIPFromHeader.ParseFromHeader(LocalUserField.ToString());
            SIPToHeader byeToHeader = SIPToHeader.ParseToHeader(RemoteUserField.ToString());
            int cseq = CSeq + 1;

            SIPHeader byeHeader = new SIPHeader(byeFromHeader, byeToHeader, cseq, CallId);
            byeHeader.CSeqMethod = SIPMethodsEnum.BYE;
            byeRequest.Header = byeHeader;
            byeRequest.Header.Routes = RouteSet;

            SIPViaHeader viaHeader = new SIPViaHeader(localSIPEndPoint, CallProperties.CreateBranchId());
            byeRequest.Header.Vias.PushViaHeader(viaHeader);

            return byeRequest;
        }
    }
}
