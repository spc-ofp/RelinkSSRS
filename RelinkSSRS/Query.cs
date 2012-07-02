// -----------------------------------------------------------------------
// <copyright file="Query.cs" company="Secretariat of the Pacific Community">
// Copyright (C) 2012 Secretariat of the Pacific Community
// </copyright>
// -----------------------------------------------------------------------

namespace Spc.Ofp.RelinkSSRS
{
    /*
    * This file is part of TUBS.
    *
    * TUBS is free software: you can redistribute it and/or modify
    * it under the terms of the GNU Affero General Public License as published by
    * the Free Software Foundation, either version 3 of the License, or
    * (at your option) any later version.
    *  
    * TUBS is distributed in the hope that it will be useful,
    * but WITHOUT ANY WARRANTY; without even the implied warranty of
    * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    * GNU Affero General Public License for more details.
    *  
    * You should have received a copy of the GNU Affero General Public License
    * along with TUBS.  If not, see <http://www.gnu.org/licenses/>.
    */
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Class representing an SSRS query.
    /// </summary>
    public class Query
    {
        public const string DesignerNamespace = @"http://schemas.microsoft.com/SQLServer/reporting/reportdesigner";
        public const string ReportingServicesNamespace = @"http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition";
        public const string SharedDatasetNamespace = @"http://schemas.microsoft.com/sqlserver/reporting/2010/01/shareddatasetdefinition";
        
        public Query()
        {
            Parameters = new List<string>();
        }
        
        public IList<string> Parameters { get; protected internal set; }

        public string DataSource { get; set; }

        public string CommandText { get; set; }

        public XElement AsXElement()
        {
            var queryparams =
                from p in this.Parameters
                select new XElement(XName.Get("QueryParameter", ReportingServicesNamespace), new XAttribute("Name", p),
                    new XElement(XName.Get("Value", ReportingServicesNamespace), String.Format("=Parameters!{0}.Value", TrimParam(p))));            
            
            return new XElement(XName.Get("Query", ReportingServicesNamespace),
                new XElement(XName.Get("DataSourceName", ReportingServicesNamespace), DataSource),
                new XElement(XName.Get("QueryParameters", ReportingServicesNamespace), queryparams),
                new XElement(XName.Get("CommandText", ReportingServicesNamespace), CommandText),
                new XElement(XName.Get("UseGenericDesigner", DesignerNamespace), true));
        }

        private static string TrimParam(string paramName)
        {
            if (String.IsNullOrEmpty(paramName))
                return paramName;
            return paramName.Replace("@", String.Empty);
        }

        public static Query Load(string infile)
        {
            Query q = new Query();
            var rsd = XDocument.Load(infile);

            q.DataSource = (
                from e in rsd.Descendants(XName.Get("DataSourceReference", SharedDatasetNamespace))
                select e.Value
            ).FirstOrDefault();

            q.Parameters = (
                from e in rsd.Descendants(XName.Get("DataSetParameter", SharedDatasetNamespace))
                select e.Attribute("Name").Value
            ).ToList();

            q.CommandText = (
                from e in rsd.Descendants(XName.Get("CommandText", SharedDatasetNamespace))
                select e.Value
            ).FirstOrDefault();

            return q;
        }
    }
}
