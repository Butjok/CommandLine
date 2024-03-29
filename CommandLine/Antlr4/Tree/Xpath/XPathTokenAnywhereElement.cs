/* Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System.Collections.Generic;

namespace Antlr4.Runtime.Tree.Xpath
{
    public class XPathTokenAnywhereElement : XPathElement
    {
        protected internal int tokenType;

        public XPathTokenAnywhereElement(string tokenName, int tokenType)
            : base(tokenName)
        {
            this.tokenType = tokenType;
        }

        public override ICollection<IParseTree> Evaluate(IParseTree t)
        {
            return Trees.FindAllTokenNodes(t, tokenType);
        }
    }
}
