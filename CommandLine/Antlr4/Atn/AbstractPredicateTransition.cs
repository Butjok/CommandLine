/* Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

namespace Antlr4.Runtime.Atn
{
    /// <author>Sam Harwell</author>
    public abstract class AbstractPredicateTransition : Transition
    {
        public AbstractPredicateTransition(ATNState target)
            : base(target)
        {
        }
    }
}
