/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenETaxBill.Channel.Library.Net.Dns
{
    #region struct CacheEntry

    internal struct CacheEntry
    {
        object m_RecordObj;
        int m_Time;

        public CacheEntry(object recordObj, int addTime)
        {
            m_RecordObj = recordObj;
            m_Time = addTime;
        }

        public object RecordObj
        {
            get
            {
                return m_RecordObj;
            }
        }

        public int Time
        {
            get
            {
                return m_Time;
            }
        }
    }

    #endregion

    /// <summary>
    /// Summary description for DnsCache.
    /// </summary>
    public class DnsCache
    {
        private static Hashtable m_ChacheTbl = null;
        private static int m_HoldInCacheTime = 1000000;

        public DnsCache()
        {
        }


        #region function InitNewCache

        public static void InitNewCache()
        {
            m_ChacheTbl = new Hashtable();
        }

        #endregion


        #region function GetMXFromCache

        /// <summary>
        /// Tries to get MX records from cache, if any.
        /// </summary>
        /// <param name="domain"></param>
        /// <returns>Returns null if not in cache.</returns>
        public static MX_Record[] GetMXFromCache(string domain)
        {
            try
            {
                if (m_ChacheTbl.Contains(domain + "[MX]"))
                {
                    CacheEntry entry = (CacheEntry)m_ChacheTbl[domain + "[MX]"];

                    // If cache object isn't expired
                    if (entry.Time + m_HoldInCacheTime > Environment.TickCount)
                    {
                        return (MX_Record[])entry.RecordObj;
                    }
                }
            }
            catch//(Exception x)
            {
                //		Console.WriteLine(x.Message);
            }

            return null;
        }

        #endregion

        #region function AddMXToCache

        /// <summary>
        /// Adds domain's MX records to cache.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="mx"></param>
        public static void AddMXToCache(string domain, MX_Record[] mx)
        {
            try
            {
                lock (m_ChacheTbl)
                {
                    // Remove old cache entry, if any.
                    if (m_ChacheTbl.Contains(domain + "[MX]"))
                    {
                        m_ChacheTbl.Remove(domain + "[MX]");
                    }
                    m_ChacheTbl.Add(domain + "[MX]", new CacheEntry(mx, Environment.TickCount));
                }
            }
            catch//(Exception x)
            {
                //		Console.WriteLine(x.Message);
            }
        }

        #endregion


        #region Properties Implementation

        public static bool CacheInited
        {
            get
            {
                return (m_ChacheTbl != null);
            }
        }

        #endregion

    }
}
