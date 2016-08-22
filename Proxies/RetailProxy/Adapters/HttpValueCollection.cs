/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        internal class HttpValueCollection : List<HttpValue>
        {
            public HttpValueCollection()
            {
            }
    
            public HttpValueCollection(string query)
                : this(query, true)
            {
            }
    
            public HttpValueCollection(string query, bool urlencoded)
            {
                if (!string.IsNullOrEmpty(query))
                {
                    this.Parse(query, urlencoded);
                }
            }
    
            public string this[string key]
            {
                get
                {
                    var firstOrDefault = this.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
                    return firstOrDefault == null ? null : firstOrDefault.Value;
                }
    
                set
                {
                    var firstOrDefault = this.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
                    if (firstOrDefault == null)
                    {
                        this.Add(key, value);
                    }
                    else
                    {
                        firstOrDefault.Value = value;
                    }
                }
            }
    
            public void Add(string key, string value)
            {
                this.Add(new HttpValue(key, value));
            }
    
            public bool ContainsKey(string key)
            {
                return this.Any(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            }
    
            public string[] GetValues(string key)
            {
                return this.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToArray();
            }
    
            public void Remove(string key)
            {
                this.RemoveAll(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            }
    
            public override string ToString()
            {
                return this.ToString(true);
            }
    
            public virtual string ToString(bool urlencoded)
            {
                return this.ToString(urlencoded, null);
            }
    
            public virtual string ToString(bool urlencoded, IDictionary excludeKeys)
            {
                if (this.Count == 0)
                {
                    return string.Empty;
                }
    
                StringBuilder stringBuilder = new StringBuilder();
    
                foreach (HttpValue item in this)
                {
                    string key = item.Key;
    
                    if ((excludeKeys == null) || !excludeKeys.Contains(key))
                    {
                        string value = item.Value;
    
                        if (urlencoded)
                        {
                            key = Uri.EscapeDataString(key);
                        }
    
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Append('&');
                        }
    
                        stringBuilder.Append((key != null) ? (key + "=") : string.Empty);
    
                        if ((value != null) && (value.Length > 0))
                        {
                            if (urlencoded)
                            {
                                value = Uri.EscapeDataString(value);
                            }
    
                            stringBuilder.Append(value);
                        }
                    }
                }
    
                return stringBuilder.ToString();
            }
    
            private void Parse(string query, bool urlEncoded)
            {
                int num = (query != null) ? query.Length : 0;
                for (int i = 0; i < num; i++)
                {
                    int keyStartIndex = i;
                    int keyEndIndex = -1;
                    while (i < num)
                    {
                        char ch = query[i];
                        if (ch == '=')
                        {
                            if (keyEndIndex < 0)
                            {
                                keyEndIndex = i;
                            }
                        }
                        else if (ch == '&')
                        {
                            break;
                        }
    
                        i++;
                    }
    
                    string key = null;
                    string value = null;
                    if (keyEndIndex >= 0)
                    {
                        key = query.Substring(keyStartIndex, keyEndIndex - keyStartIndex);
                        value = query.Substring(keyEndIndex + 1, (i - keyEndIndex) - 1);
                    }
                    else
                    {
                        value = query.Substring(keyStartIndex, i - keyStartIndex);
                    }
    
                    if (urlEncoded)
                    {
                        this.Add(Uri.UnescapeDataString(key), Uri.UnescapeDataString(value));
                    }
                    else
                    {
                        this.Add(key, value);
                    }
    
                    if ((i == (num - 1)) && (query[i] == '&'))
                    {
                        this.Add(null, string.Empty);
                    }
                }
            }
        }
    }
}
