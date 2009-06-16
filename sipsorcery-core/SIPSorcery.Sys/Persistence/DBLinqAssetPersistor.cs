// ============================================================================
// FileName: DBLinqAssetPersistor.cs
//
// Description:
// Persistor class for storing SIP assets in relational databases via DBLinq.
//
// Author(s):
// Aaron Clauson
//
// History:
// 20 Apr 2009	Aaron Clauson	Created.
//
// License: 
// This software is licensed under the BSD License http://www.opensource.org/licenses/bsd-license.php
//
// Copyright (c) 2009 Aaron Clauson (aaronc@blueface.ie), Blue Face Ltd, Dublin, Ireland (www.blueface.ie)
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using DbLinq.Data.Linq;
using SIPSorcery.Sys;
using log4net;

namespace SIPSorcery.Sys
{
    public class DBLinqAssetPersistor<T> : SIPAssetPersistor<T> where T : class, ISIPAsset, new()
    {
        private static ILog logger = AppState.logger;
        private static string m_newLine = AppState.NewLine;

        //private DataContext m_dbLinqDataContext;
        //private Table<T> m_dbLinqTable;
        private StorageTypes m_storageType;
        private string m_dbConnStr;

        public override event SIPAssetDelegate<T> Added;
        public override event SIPAssetDelegate<T> Updated;
        public override event SIPAssetDelegate<T> Deleted;

        public DBLinqAssetPersistor(StorageTypes storageType, string connectionString) {
            m_storageType = storageType;
            m_dbConnStr = connectionString;
            //m_dbLinqDataContext = dbLinqDataContext;
            //m_dbLinqTable = m_dbLinqDataContext.GetTable<T>();
        }

        public override T Add(T asset) {
            try {
                //m_dbLinqTable.InsertOnSubmit(asset);
                //m_dbLinqDataContext.SubmitChanges();
                //m_dbLinqDataContext.ExecuteDynamicInsert(asset);
                DataContext dataContext = DBLinqContext.CreateDBLinqDataContext(m_storageType, m_dbConnStr);
                dataContext.ExecuteDynamicInsert(asset);

                if (Added != null) {
                    Added(asset);
                }

                return asset;
            }
            catch (Exception excp) {
                logger.Error("Exception DBLinqAssetPersistor Add (for " + typeof(T).Name + "). " + excp.Message);
                throw;
            }
        }

        public override T Update(T asset) {
            try {
                //m_dbLinqDataContext.ExecuteDynamicUpdate(asset);
                //m_dbLinqTable.InsertOnSubmit(asset);
                //m_dbLinqDataContext.SubmitChanges();
                DataContext dataContext = DBLinqContext.CreateDBLinqDataContext(m_storageType, m_dbConnStr);
                dataContext.ExecuteDynamicUpdate(asset);

                if (Updated != null) {
                    Updated(asset);
                }

                return asset;
            }
            catch (Exception excp) {
                logger.Error("Exception DBLinqAssetPersistor Update (for " + typeof(T).Name + "). " + excp.Message);
                throw;
            }
        }

        public override void Delete(T asset) {
            try {
                //m_dbLinqTable.DeleteOnSubmit(asset);
                //m_dbLinqDataContext.SubmitChanges();
                //m_dbLinqDataContext.ExecuteDynamicDelete(asset);
                DataContext dataContext = DBLinqContext.CreateDBLinqDataContext(m_storageType, m_dbConnStr);
                dataContext.ExecuteDynamicDelete(asset);

                if (Deleted != null) {
                    Deleted(asset);
                }
            }
            catch (Exception excp) {
                logger.Error("Exception DBLinqAssetPersistor Delete (for " + typeof(T).Name + "). " + excp.Message);
                throw;
            }
        }

        public override void Delete(Expression<Func<T, bool>> whereClause) {
            try {
                DataContext dataContext = DBLinqContext.CreateDBLinqDataContext(m_storageType, m_dbConnStr);
                Table<T> table = dataContext.GetTable<T>();

                var batch = from asset in table.Where(whereClause)
                            select asset;

                if (batch.Count() > 0) {
                    T[] batchArray = batch.ToArray();
                    for (int index = 0; index < batchArray.Length; index++) {
                        dataContext.ExecuteDynamicDelete(batchArray[index]);
                    }
                }
            }
            catch (Exception excp) {
                logger.Error("Exception DBLinqAssetPersistor Delete (batch) (for " + typeof(T).Name + "). " + excp.Message);
                throw;
            }
        }

        public override T Get(Guid id) {
            try {
                DataContext dataContext = DBLinqContext.CreateDBLinqDataContext(m_storageType, m_dbConnStr);
                Table<T> table = dataContext.GetTable<T>();

                string idString = id.ToString();

                return (from asset in table
                        where asset.Id == idString
                        select asset).FirstOrDefault();
            }
            catch (Exception excp) {
                logger.Error("Exception DBLinqAssetPersistor Get (id) (for " + typeof(T).Name + "). " + excp.Message);
                throw;
            }
        }

        public override int Count(Expression<Func<T, bool>> whereClause) {
            try {
                DataContext dataContext = DBLinqContext.CreateDBLinqDataContext(m_storageType, m_dbConnStr);
                Table<T> table = dataContext.GetTable<T>();

                if (whereClause == null) {
                    return table.Count();
                }
                else {
                    return table.Count(whereClause);
                }
            }
            catch (Exception excp) {
                logger.Error("Exception DBLinqAssetPersistor Count (for " + typeof(T).Name + "). " + excp.Message);
                throw excp;
            }
        }

        public override T Get(Expression<Func<T, bool>> whereClause) {
            try {
                DataContext dataContext = DBLinqContext.CreateDBLinqDataContext(m_storageType, m_dbConnStr);
                Table<T> table = dataContext.GetTable<T>();

                if (whereClause == null) {
                    throw new ArgumentException("The where clause must be specified for a non-list Get.");
                }
                else {
                    IQueryable<T> getList = from asset in table.Where(whereClause) select asset;
                    return getList.FirstOrDefault();
                }
            }
            catch (Exception excp) {
                logger.Error("Exception DBLinqAssetPersistor Get (where for " + typeof(T).Name + "). " + excp.Message);
                return default(T);
            }
        }

        public override List<T> Get(Expression<Func<T, bool>> whereClause, string orderByField, int offset, int count)
        {
            try
            {
                DataContext dataContext = DBLinqContext.CreateDBLinqDataContext(m_storageType, m_dbConnStr);
                Table<T> table = dataContext.GetTable<T>();

                var query = from asset in table select asset;
                
                if (whereClause != null)
                {
                    query = query.Where(whereClause);
                }

                if (!orderByField.IsNullOrBlank())
                {
                    query = query.OrderBy(orderByField);
                }
                else
                {
                    query = query.OrderBy(x => x.Id);
                }

                if (offset != 0)
                {
                    query = query.Skip(offset);
                }

                if (count < Int32.MaxValue)
                {
                    query = query.Take(count);
                }

                /*List<T> subList = null;

                if (whereClause == null) {
                    subList = (
                        from asset in m_dbLinqTable
                        select asset
                        ).ToList();
                }
                else {
                    subList = (
                        from asset in m_dbLinqTable.Where(whereClause)
                        select asset
                        ).ToList();
                }

                if (subList != null) {
                    if (offset >= 0) {
                        if (count == 0 || count == Int32.MaxValue) {
                            return subList.OrderBy(x => x.Id).Skip(offset).ToList<T>();
                        }
                        else {
                            return subList.OrderBy(x => x.Id).Skip(offset).Take(count).ToList<T>();
                        }
                    }
                    else {
                        return subList.OrderBy(x => x.Id).ToList<T>(); ;
                    }
                }

                return subList;*/

                return query.ToList();
            }
            catch (Exception excp)
            {
                logger.Error("Exception DBLinqAssetPersistor Get List (where for " + typeof(T).Name + "). " + excp.Message);
                return null;
            }
        }
    }
}
