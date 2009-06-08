﻿// ============================================================================
// FileName: CustomerSessionManager.cs
//
// Description:
// Manages user sessions for authenticated users.
//
// Author(s):
// Aaron Clauson
//
// History:
// 20 May 2009	Aaron Clauson	Created.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIPSorcery.Sys;
using log4net;

namespace SIPSorcery.CRM
{
    public delegate CustomerSession AuthenticateCustomerDelegate(string username, string password, string ipAddress);
    public delegate CustomerSession AuthenticateTokenDelegate(string token);
    public delegate void ExpireTokenDelegate(string token);

    public class CustomerSessionManager {

        private static ILog logger = AppState.logger;

        private SIPAssetPersistor<Customer> m_customerPersistor;
        private SIPAssetPersistor<CustomerSession> m_customerSessionPersistor;

        public SIPAssetPersistor<Customer> CustomerPersistor
        {
            get { return m_customerPersistor; }
        }

        public CustomerSessionManager(StorageTypes storageType, string connectionString) {
            m_customerPersistor = CustomerPersistorFactory.CreateCustomerPersistor(storageType, connectionString);
            m_customerSessionPersistor = CustomerPersistorFactory.CreateCustomerSessionPersistor(storageType, connectionString);
        }

        public CustomerSession Authenticate(string username, string password, string ipAddress) {
            try {
                Customer customer = m_customerPersistor.Get(c => c.CustomerUsername == username && c.CustomerPassword == password);

                if (customer != null) {
                    logger.Debug("Login successful for " + username + ".");

                    Guid sessionId = Guid.NewGuid();
                    CustomerSession customerSession = new CustomerSession(sessionId, customer.CustomerUsername, ipAddress);
                    m_customerSessionPersistor.Add(customerSession);
                    return customerSession;
                }
                else {
                    logger.Debug("Login failed for " + username + ".");
                    return null;
                }
            }
            catch (Exception excp) {
                logger.Error("Exception Authenticate CustomerSessionManager. " + excp.Message);
                throw;
            }
        }

        public CustomerSession Authenticate(string sessionId) {
            try {
                CustomerSession customerSession = m_customerSessionPersistor.Get(s => s.Id == sessionId && !s.Expired);
                //CustomerSession customerSession = m_customerSessionPersistor.Get(s => s.Id == sessionId);

                if (customerSession != null)
                {
                    if (DateTime.Now.Subtract(customerSession.Inserted).TotalMinutes > CustomerSession.MAX_SESSION_LIFETIME_MINUTES)
                    {
                        customerSession.Expired = true;
                        m_customerSessionPersistor.Update(customerSession);
                        return null;
                    }
                    else
                    {
                        //logger.Debug("Authentication token valid for " + sessionId + ".");
                        return customerSession;
                    }
                }
                else {
                    logger.Warn("Authentication token invalid for " + sessionId + ".");
                    return null;
                }
            }
            catch (Exception excp) {
                logger.Error("Exception Authenticate CustomerSessionManager. " + excp.Message);
                throw;
            }
        }

        public void ExpireToken(string sessionId) {

            try {
                CustomerSession customerSession = m_customerSessionPersistor.Get(s => s.Id == sessionId);
                if (customerSession != null)
                {
                    customerSession.Expired = true;
                    m_customerSessionPersistor.Update(customerSession);
                }
            }
            catch (Exception excp) {
                logger.Error("Exception ExpireToken CustomerSessionManager. " + excp.Message);
                throw;
            }
        }
    }
}
