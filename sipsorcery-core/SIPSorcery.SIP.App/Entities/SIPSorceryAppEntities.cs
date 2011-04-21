using System;
using System.Collections.Generic;

namespace SIPSorcery.SIP.App.Entities
{
	/// <summary>
	/// SIP sorcery app entities. Context class for Entity Framework.
	/// </summary>
	public class SIPSorceryAppEntities
	{
		public SIPSorceryAppEntities ()
		{
			
		}
		
		public List<SIPDialplanLookup> SIPDialplanLookups
		{
			get {
				return null;	
			}
		}
		
		public List<SIPDialplanOption> SIPDialplanOptions
		{
			get
			{
				return null;	
			}
		}
		
		public List<SIPDialplanRoute> SIPDialplanRoutes
		{
			get
			{
				return null;	
			}
		}
		
		public List<SIPDialplanProvider> SIPDialplanProviders
		{
			get
			{
				return null;	
			}
		}
		
		
	}
}

