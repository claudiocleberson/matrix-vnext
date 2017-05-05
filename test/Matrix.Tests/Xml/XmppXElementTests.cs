/*
 * Copyright (c) 2003-2017 by AG-Software <info@ag-software.de>
 *
 * All Rights Reserved.
 * See the COPYING file for more information.
 *
 * This file is part of the MatriX project.
 *
 * NOTICE: All information contained herein is, and remains the property
 * of AG-Software and its suppliers, if any.
 * The intellectual and technical concepts contained herein are proprietary
 * to AG-Software and its suppliers and may be covered by German and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 *
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from AG-Software.
 *
 * Contact information for AG-Software is available at http://www.ag-software.de
 */

using Matrix.Xml;
using Shouldly;
using System.Text.RegularExpressions;
using Xunit;

namespace Matrix.Tests.Xml
{
    public class XmppXElementTests
    {
        [Fact]
        public void TestLoadXmlFromString()
        {
            string xml1 = "<a><b>foo</b></a>";
            var elA = XmppXElement.LoadXml(xml1);
            elA.ToString(false).ShouldBe(xml1);
        }

        [Fact]
        public void XmlShouldContainOneCDataElement()
        {
            var el = XmppXElement.LoadXml(Resource.Get("Xml.cdata1.xml"));

            var xml = el.ToString(false);
            Regex.Matches(xml, "CDATA").Count.ShouldBe(1);
        }
    }
}
