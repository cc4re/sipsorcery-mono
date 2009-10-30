﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using SIPSorcery.Sys;
using SIPSorcery.Persistence;

#if !SILVERLIGHT
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
#endif

namespace SIPSorcery.SIP.App {

    [Table(Name = "sipdialogues")]
    public class SIPDialogueAsset : ISIPAsset
    {
        public const string XML_DOCUMENT_ELEMENT_NAME = "sipdialogues";
        public const string XML_ELEMENT_NAME = "sipdialogue";

        private static string m_newLine = AppState.NewLine;

        //public static readonly string SelectHangupQuery = "select * from sipdialogues where hangupat <= ?1";

        [IgnoreDataMember]
        public SIPDialogue SIPDialogue;

        [Column(Name = "id", DbType = "StringFixedLength", IsPrimaryKey = true, CanBeNull = false)]
        public Guid Id {
            get { return SIPDialogue.Id; }
            set { SIPDialogue.Id = value; }
        }

        [Column(Name = "owner", DbType = "StringFixedLength", CanBeNull = false)]
        public string Owner {
            get { return SIPDialogue.Owner; }
            set { SIPDialogue.Owner = value; }
        }

        [Column(Name = "adminmemberid", DbType = "StringFixedLength", CanBeNull = true)]
        public string AdminMemberId {
            get { return SIPDialogue.AdminMemberId; }
            set { SIPDialogue.AdminMemberId = value; }
        }

        [Column(Name = "dialogueid", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string DialogueId {
            get { return SIPDialogue.DialogueId; }
            set { SIPDialogue.DialogueId = value; }
        }

        [Column(Name = "localtag", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string LocalTag {
            get { return SIPDialogue.LocalTag; }
            set { SIPDialogue.LocalTag = value; }
        }

        [Column(Name = "remotetag", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string RemoteTag {
            get { return SIPDialogue.RemoteTag; }
            set { SIPDialogue.RemoteTag = value; }
        }

        [Column(Name = "callid", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string CallId {
            get { return SIPDialogue.CallId; }
            set { SIPDialogue.CallId = value; }
        }

        [Column(Name = "cseq", DbType = "Int32", IsPrimaryKey = false, CanBeNull = false)]
        public int CSeq {
            get { return SIPDialogue.CSeq; }
            set { SIPDialogue.CSeq = value; }
        }

        [Column(Name = "bridgeid", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string BridgeId {
            get { return SIPDialogue.BridgeId.ToString(); }
            set { SIPDialogue.BridgeId = (!value.IsNullOrBlank()) ? new Guid(value) : Guid.Empty; }
        }

        [Column(Name = "remotetarget", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string RemoteTarget {
            get { return SIPDialogue.RemoteTarget.ToString(); }
            set { SIPDialogue.RemoteTarget = (!value.IsNullOrBlank()) ? SIPURI.ParseSIPURI(value) : null; }
        }

        [IgnoreDataMember]
        [Column(Name = "localuserfield", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string LocalUserField {
            get { return SIPDialogue.LocalUserField.ToString(); }
            set { SIPDialogue.LocalUserField = (!value.IsNullOrBlank()) ? SIPUserField.ParseSIPUserField(value) : null; }
        }

        [DataMember]
        [Column(Name = "remoteuserfield", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string RemoteUserField {
            get { return SIPDialogue.RemoteUserField.ToString(); }
            set { SIPDialogue.RemoteUserField = (!value.IsNullOrBlank()) ? SIPUserField.ParseSIPUserField(value) : null; }
        }

        [Column(Name = "proxysipsocket", DbType = "StringFixedLength", CanBeNull = true)]
        [DataMember]
        public string ProxySIPSocket {
            get { return SIPDialogue.ProxySendFrom; }
            set { SIPDialogue.ProxySendFrom = value;}
        }

        [IgnoreDataMember]
        [Column(Name = "routeset", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = true)]
        public string RouteSet {
            get { return (SIPDialogue.RouteSet != null) ? SIPDialogue.RouteSet.ToString() : null; }
            set { SIPDialogue.RouteSet = (!value.IsNullOrBlank()) ? SIPRouteSet.ParseSIPRouteSet(value) : null; }
        }

        [Column(Name = "cdrid", DbType = "StringFixedLength", IsPrimaryKey = false, CanBeNull = false)]
        public string CDRId {
            get { return SIPDialogue.CDRId.ToString(); }
            set { SIPDialogue.CDRId = (!value.IsNullOrBlank()) ? new Guid(value) : Guid.Empty; }
        }

        [DataMember]
        [Column(Name = "calldurationlimit", DbType = "Int32", IsPrimaryKey = false, CanBeNull = false)]
        public int CallDurationLimit
        {
            get { return SIPDialogue.CallDurationLimit; }
            set { SIPDialogue.CallDurationLimit = value; }
        }

        [DataMember]
        [Column(Name = "inserted", DbType = "StringFixedLength", CanBeNull = false)]
        public DateTime Inserted {
            get { return SIPDialogue.Inserted; }
            set { SIPDialogue.Inserted = value.ToUniversalTime(); }
        }

        [DataMember]
        [Column(Name = "hangupat", DbType = "StringFixedLength", CanBeNull = true)]
        public DateTime? HangupAt {
            get {
                if (CallDurationLimit != 0) {
                    return Inserted.AddSeconds(CallDurationLimit);
                }
                else {
                    return null;
                }
            }
        }

        public SIPDialogueAsset() {
            SIPDialogue = new SIPDialogue();
        }

        public SIPDialogueAsset(SIPDialogue sipDialogue) {
            SIPDialogue = sipDialogue;
        }

#if !SILVERLIGHT

        public SIPDialogueAsset(DataRow row) {
            Load(row);
        }

        public DataTable GetTable() {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("id", typeof(String)));
            table.Columns.Add(new DataColumn("owner", typeof(String)));
            table.Columns.Add(new DataColumn("adminmemberid", typeof(String)));
            table.Columns.Add(new DataColumn("dialogueid", typeof(String)));
            table.Columns.Add(new DataColumn("localtag", typeof(String)));
            table.Columns.Add(new DataColumn("remotetag", typeof(String)));
            table.Columns.Add(new DataColumn("localuserfield", typeof(String)));
            table.Columns.Add(new DataColumn("remoteuserfield", typeof(String)));
            table.Columns.Add(new DataColumn("callid", typeof(String)));
            table.Columns.Add(new DataColumn("cseq", typeof(Int32)));
            table.Columns.Add(new DataColumn("bridgeid", typeof(String)));
            table.Columns.Add(new DataColumn("proxysipsocket", typeof(String)));
            table.Columns.Add(new DataColumn("remotetarget", typeof(String)));
            table.Columns.Add(new DataColumn("routeset", typeof(String)));
            table.Columns.Add(new DataColumn("cdrid", typeof(String)));
            table.Columns.Add(new DataColumn("calldurationlimit", typeof(Int32)));
            table.Columns.Add(new DataColumn("hangupat", typeof(DateTime)));
            table.Columns.Add(new DataColumn("inserted", typeof(DateTime)));
            return table;
        }

        public void Load(DataRow row) {
            SIPDialogue = new SIPDialogue();
            SIPDialogue.Id = new Guid(row["id"] as string);
            SIPDialogue.Owner = row["owner"] as string;
            SIPDialogue.AdminMemberId = row["adminmemberid"] as string;
            SIPDialogue.DialogueId = row["dialogueid"] as string;
            SIPDialogue.LocalTag = row["localtag"] as string;
            SIPDialogue.RemoteTag = row["remotetag"] as string;
            SIPDialogue.LocalUserField = SIPUserField.ParseSIPUserField(row["localuserfield"] as string);
            SIPDialogue.RemoteUserField = SIPUserField.ParseSIPUserField(row["remoteuserfield"] as string);
            SIPDialogue.CallId = row["callid"] as string;
            SIPDialogue.CSeq = Convert.ToInt32(row["cseq"]);
            SIPDialogue.BridgeId = new Guid(row["bridgeid"] as string);
            SIPDialogue.RemoteTarget = SIPURI.ParseSIPURI(row["remotetarget"] as string);
            SIPDialogue.RouteSet = (row["routeset"] != null && row["routeset"] != DBNull.Value && !(row["routeset"] as string).IsNullOrBlank()) ? SIPRouteSet.ParseSIPRouteSet(row["routeset"] as string) : null;
            SIPDialogue.ProxySendFrom = (row["proxysipsocket"] != null && row["proxysipsocket"] != DBNull.Value) ? row["proxysipsocket"] as string : null;
            SIPDialogue.CDRId = new Guid(row["cdrid"] as string);
            SIPDialogue.CallDurationLimit = (row["calldurationlimit"] != null && row["calldurationlimit"] != DBNull.Value) ? Convert.ToInt32(row["calldurationlimit"]) : 0;
            Inserted = Convert.ToDateTime(row["inserted"]);
        }

        public Dictionary<Guid, object> Load(XmlDocument dom) {
            return SIPAssetXMLPersistor<SIPDialogueAsset>.LoadAssetsFromXMLRecordSet(dom);
        }

#endif

        public string ToXML() {
            string dialogueXML =
                " <" + XML_ELEMENT_NAME + ">" + m_newLine +
                ToXMLNoParent() +
                " </" + XML_ELEMENT_NAME + ">" + m_newLine;

            return dialogueXML;
        }

        public string ToXMLNoParent() {
            string hanupAtStr = (HangupAt != null) ? HangupAt.Value.ToString("o") : null;

            string dialogueXML =
                 "  <id>" + SIPDialogue.Id + "</id>" + m_newLine +
                 "  <owner>" + SIPDialogue.Owner + "</owner>" + m_newLine +
                 "  <adminmemberid>" + SIPDialogue.AdminMemberId + "</adminmemberid>" + m_newLine +
                 "  <dialogueid>" + SIPDialogue.DialogueId + "</dialogueid>" + m_newLine +
                 "  <localtag>" + SIPDialogue.LocalTag + "</localtag>" + m_newLine +
                 "  <remotetag>" + SIPDialogue.RemoteTag + "</remotetag>" + m_newLine +
                 "  <callid>" + SIPDialogue.CallId + "</callid>" + m_newLine +
                 "  <cseq>" + SIPDialogue.CSeq + "</cseq>" + m_newLine +
                 "  <bridgeid>" + SIPDialogue.BridgeId + "</bridgeid>" + m_newLine +
                 "  <remotetarget>" + SafeXML.MakeSafeXML(SIPDialogue.RemoteTarget.ToString()) + "</remotetarget>" + m_newLine +
                 "  <localuserfield>" + SafeXML.MakeSafeXML(SIPDialogue.LocalUserField.ToString()) + "</localuserfield>" + m_newLine +
                 "  <remoteuserfield>" + SafeXML.MakeSafeXML(SIPDialogue.RemoteUserField.ToString()) + "</remoteuserfield>" + m_newLine +
                 "  <routeset>" + SafeXML.MakeSafeXML(RouteSet) + "</routeset>" + m_newLine +
                 "  <cdrid>" + SIPDialogue.CDRId + "</cdrid>" + m_newLine +
                 "  <calldurationlimit>" + SIPDialogue.CallDurationLimit + "</calldurationlimit>" + m_newLine +
                 "  <inserted>" + Inserted.ToString("o") + "</inserted>" + m_newLine +
                 "  <hangupat>" + hanupAtStr + "</hangupat>" + m_newLine;

            return dialogueXML;
        }

        public string GetXMLElementName() {
            return XML_ELEMENT_NAME;
        }

        public string GetXMLDocumentElementName() {
            return XML_DOCUMENT_ELEMENT_NAME;
        }
    }
}
