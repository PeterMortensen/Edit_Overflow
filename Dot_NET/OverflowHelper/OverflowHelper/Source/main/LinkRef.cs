/****************************************************************************
 * Copyright (C) 2011 Peter Mortensen                                       *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: encapsulates details about reference (link) identifiers (IDs)   *
 *          used in the Stack Exchange network (for example, making it      * 
 *          unique or knowledge of what consistutes valid IDs).             *
 *                                                                          *
 *                                                                          *
 * Sample - the number in the Markdown source must be unique, 1, 2 and 3:   *
 *                                                                          *
 *     [Scatter plot, with trendline][2]                                    *
 *     [X-Y plot with the actual data points shown, line points][3]         *
 *     [Sticks plot, with overlayed annotation (`TextBox`es, in fact)][4]   *
 *                                                                          *
 *                                                                          *
 *     [2]: http://msquant.sourceforge.net                                  *
 *     [3]: http://i.stack.imgur.com/bvUae.png                              *
 *     [4]: http://i.stack.imgur.com/xyznI.png                              *
 *                                                                          *
 ****************************************************************************/


//PM_AUTOMATIC_MARKDOWN_LINK_IDS 2012-01-30
namespace OverflowHelper.core
{
    public class LinkRef
    {
        int mRef;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public LinkRef()
        {
            mRef = 1;
        } //Constructor.


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string GetRef()
        {
            //string toReturn = mRef.ToString();

            return (mRef++).ToString(); //For now.
        } //

    } //class LinkRef

} //OverflowHelper.core

