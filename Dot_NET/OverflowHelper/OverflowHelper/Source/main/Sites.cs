
/***************************************************************************
*                                                                          *
*   This class has knowledge of sites, e.g. in the Stack Exchange network. *
*                                                                          *
*   It also holds the active site.                                         *
*                                                                          *
*                                                                          *
*                                                                          *
*                                                                          *
****************************************************************************/

//Future: add a new field, the name of a site. 
//       
//        E.g. XXX is "Stack Apps".


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


using System.Collections.Generic;


// Site information
//
//  Site            URL                        Questions (2014-06-06)
//  --------------------------------------------------------------
//  Stack Overflow  http://stackoverflow.com/  7,432,755
//  Ask Ubuntu      http://askubuntu.com/        158,469



/***************************************************************************
*    <placeholder for header>                                              *
****************************************************************************/
namespace OverflowHelper.core
{


    /***************************************************************************
    *    <placeholder for header>                                              *
    ****************************************************************************/
    public struct siteItem
    {
        public string siteDomainURL; //Sample: "askubuntu.com"
        public int questions;
        public int posts;
        public int order;           //Determines in what order a site 
                                    //appear in the user interface.

        public int unansweredQuestions;

        public string name; //Sample: "Ask Ubuntu"        
    }


    /***************************************************************************
    *    <placeholder for header>                                              *
    ****************************************************************************/
    public class Sites
    {
        private string mCurrentSiteDomainURL;

        // The key is the site domain URL, e.g. "physics.stackexchange.com"
        private Dictionary<string, siteItem> mDomainURL2SiteInfo;



        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        public Sites()
        {
            mCurrentSiteDomainURL = "stackoverflow.com";

            mDomainURL2SiteInfo = new Dictionary<string, siteItem>();

            //           Domain URL                     Questions  Posts   Unanswered  Order
            //                                                             questions

            addSiteItem("meta.stackexchange.com", 1000, 8000, 10, 1, "");
            addURL("meta.stackexchange.com", "Meta Stack Exchange (MSE)");

            addURL("meta.stackoverflow.com", "Meta Stack Overflow (MSO)");

            addURL("stackapps.com", "Stack Apps");

            addURL("data.stackexchange.com", "Stack Exchange Data Explorer (SEDE)");
            
            // addSiteItem("askubuntu.com",               158469,    477599,   31450,  1, "");
            addSiteItem("askubuntu.com"                ,   215253,  730135,    72526,  2, ""); // 2016-02-05
            addURL("askubuntu.com", "Ask Ubuntu");

            // addSiteItem("stackoverflow.com",           7432755, 24080096,  803224,  1, "");
            addSiteItem("stackoverflow.com"            , 11014245, 35231044, 2962635,  4, ""); // 2016-02-05

            addSiteItem("superuser.com"                ,   223208,   764414,   31576,  3, "");

            addSiteItem("area51.stackexchange.com"     ,     1000,     8000,      10, 77, "");
            addSiteItem("english.stackexchange.com"    ,    37980,   175386,      51, 77, "");
            addSiteItem("serverfault.com"              ,   175619,   602315,   14588,  5, "");
            addSiteItem("physics.stackexchange.com"    ,     1000,     8000,      10, 77, "");
            addSiteItem("softwareengineering.stackexchange.com", 31607, 243217, 700, 6, "");
            addSiteItem("webapps.stackexchange.com"    ,     1000,     8000,      10, 77, "");
            addSiteItem("electronics.stackexchange.com",     1000,     8000,      10, 77, "");
            addSiteItem("raspberrypi.stackexchange.com",     1000,     8000,      10, 77, "");
            addSiteItem("bitcoin.stackexchange.com"    ,     1000,     8000,      10, 77, "");
            addSiteItem("android.stackexchange.com"    ,     1000,     8000,      10, 77, "");
            addSiteItem("unix.stackexchange.com"       ,     1000,     8000,      10, 77, "");
            addSiteItem("math.stackexchange.com"       ,     1000,     8000,      10, 77, "");

            addURL("stackoverflow.com", "Stack Overflow");
            addURL("superuser.com", "Super User");
            addURL("area51.stackexchange.com", "Area 51");
            addURL("english.stackexchange.com", "English Language & Usage");
            addURL("serverfault.com", "Server Fault");
            addURL("physics.stackexchange.com", "Physics");
            addURL("softwareengineering.stackexchange.com", "Programmers");
            addURL("webapps.stackexchange.com", "Web Applications");
            addURL("electronics.stackexchange.com", "Electrical Engineering");
            addURL("raspberrypi.stackexchange.com", "Raspberry Pi");
            addURL("bitcoin.stackexchange.com", "Bitcoin");
            addURL("android.stackexchange.com", "Android Enthusiasts");
            addURL("unix.stackexchange.com", "Unix & Linux");
            addURL("math.stackexchange.com", "Mathematics");
            addURL("codereview.stackexchange.com", "Code Review");
            addURL("workplace.stackexchange.com", "The Workplace");
            addURL("ux.stackexchange.com", "User Experience");
            addURL("tex.stackexchange.com", "TeX - LaTeX");
            addURL("writers.stackexchange.com", "Writers");
            addURL("stats.stackexchange.com", "Cross Validated");
            addURL("dba.stackexchange.com", "Database Administrators");
            addURL("arduino.stackexchange.com", "Arduino");
            addURL("german.stackexchange.com", "German Language");
            addURL("drupal.stackexchange.com", "Drupal Answers");
            addURL("tor.stackexchange.com", "Tor");
            addURL("scifi.stackexchange.com", "Science Fiction & Fantasy");
            addURL("sharepoint.stackexchange.com", "SharePoint");
            addURL("productivity.stackexchange.com", "Personal Productivity");
            addURL("gis.stackexchange.com", "Geographic Information Systems");
            addURL("mathematica.stackexchange.com", "Mathematica");
            addURL("codegolf.stackexchange.com", "Programming Puzzles & Code Golf");
            addURL("chemistry.stackexchange.com", "Chemistry");
            addURL("gamedev.stackexchange.com", "Game Development");
            addURL("gaming.stackexchange.com", "Arqade");
            addURL("ell.stackexchange.com", "English Language Learners");
            addURL("webmasters.stackexchange.com", "Webmasters");
            addURL("academia.stackexchange.com", "Academia");
            addURL("apple.stackexchange.com", "Ask Different");
            addURL("scicomp.stackexchange.com", "Computational Science");
            addURL("security.stackexchange.com", "Security");
            addURL("history.stackexchange.com", "History");
            addURL("space.stackexchange.com", "Space Exploration");
            addURL("aviation.stackexchange.com", "Aviation");
            addURL("dsp.stackexchange.com", "Signal Processing");
            addURL("robotics.stackexchange.com", "Robotics");
            addURL("biology.stackexchange.com", "Biology");
            addURL("reverseengineering.stackexchange.com", "Reverse Engineering"); //Still 123 rep...
            addURL("softwarerecs.stackexchange.com", "Software Recommendations");
            addURL("blender.stackexchange.com", "Blender");
            addURL("salesforce.stackexchange.com", "Salesforce");
            addURL("money.stackexchange.com", "Personal Finance & Money");
            addURL("parenting.stackexchange.com", "Parenting");
            addURL("bicycles.stackexchange.com", "Bicycles");
            addURL("islam.stackexchange.com", "Islam");
            addURL("travel.stackexchange.com", "Travel");
            addURL("movies.stackexchange.com", "Movies & TV");
            addURL("wordpress.stackexchange.com", "WordPress Development");
            addURL("worldbuilding.stackexchange.com", "Worldbuilding");
            addURL("mathoverflow.net", "MathOverflow");
            addURL("quant.stackexchange.com", "Quantitative Finance");
            addURL("music.stackexchange.com", "Music: Practice & Theory");
            addURL("puzzling.stackexchange.com", "Puzzling");
            addURL("3dprinting.stackexchange.com", "3D Printing");
            addURL("magento.stackexchange.com", "Magento");
            addURL("cs.stackexchange.com", "Computer Science");
            addURL("computergraphics.stackexchange.com", "Computer Graphics");
            addURL("skeptics.stackexchange.com", "Skeptics");
            addURL("mechanics.stackexchange.com", "Motor Vehicle Maintenance & Repair");
            addURL("diy.stackexchange.com", "Home Improvement");
            addURL("lifehacks.stackexchange.com", "Lifehacks");
            addURL("astronomy.stackexchange.com", "Astronomy");
            addURL("graphicdesign.stackexchange.com", "Graphic Design");
            addURL("earthscience.stackexchange.com", "Earth Science");
            addURL("photo.stackexchange.com", "Photography");
            addURL("sqa.stackexchange.com", "Software Quality Assurance & Testing"); //Still 123 rep...                
            addURL("ethereum.stackexchange.com", "Ethereum");

            addURL("crafts.stackexchange.com", "Arts & Crafts");

            addURL("hinduism.stackexchange.com", "Hinduism");

            addURL("emacs.stackexchange.com", "Emacs");
            addURL("cooking.stackexchange.com", "Seasoned Advice");

            addURL("interpersonal.stackexchange.com", "Interpersonal Skills");
            addURL("politics.stackexchange.com", "Politics");
            addURL("chess.stackexchange.com", "Chess");

            addURL("ai.stackexchange.com", "Artificial Intelligence");

            addURL("hsm.stackexchange.com", "History of Science and Mathematics");

            addURL("anime.stackexchange.com", "Anime & Manga");
        }


        /****************************************************************************
        *                                                                           *
        *    anOrder: determines in what order sites appear in the user interface.  *
        *                                                                           *
        ****************************************************************************/
        private void addSiteItem(string aSiteDomainURL, 
                                 int    aQuestions, 
                                 int    aPosts, 
                                 int    anUnansweredQuestions,
                                 int    anOrder,
                                 string aName)
        {
            siteItem someItem;

            someItem.siteDomainURL = aSiteDomainURL;
            someItem.questions = aQuestions;
            someItem.posts = aPosts;
            someItem.order = anOrder;
            someItem.unansweredQuestions = anUnansweredQuestions;

            someItem.name = aName;

            mDomainURL2SiteInfo.Add(aSiteDomainURL, someItem);
        }

        
        /****************************************************************************
        *                                                                           *
        ****************************************************************************/
        private void addURL(string aSiteDomainURL, string aSiteName)
        {
            //For now: Just add to the normal list with some 
            //         acceptable default values.

            int len = aSiteName.Length;
            string msg = null;
            if (len == 0)
            {
                msg = "Empty site name for '" +
                      aSiteDomainURL + "...'";
            }
            else
            {
                if (aSiteName.Substring(len - 1, 1) == " ")
                {
                    msg = "The site name, '" + aSiteName +
                          "', contains a trailing space...";
  
                }
            }

            if (msg != null)
            {
                System.Windows.Forms.MessageBox.Show(msg);
                System.Diagnostics.Trace.WriteLine(msg);  
            }
            
            int currentItems = mDomainURL2SiteInfo.Count;
            int orderKey = currentItems + 200;

            if (!mDomainURL2SiteInfo.ContainsKey(aSiteDomainURL))
            {
                addSiteItem(aSiteDomainURL, 100, 200, 10, orderKey, aSiteName);
            }
            else
            {
                siteItem someItem = mDomainURL2SiteInfo[aSiteDomainURL];
                someItem.name = aSiteName;
                mDomainURL2SiteInfo[aSiteDomainURL] = someItem; //Write back...
            }
        } //addURL()


        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        public string getCurrentSiteDomainURL()
        {
            return mCurrentSiteDomainURL;
        }


        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        public void setCurrentSiteDomainURL(string aNewCurrentSiteURL)
        {
            mCurrentSiteDomainURL = aNewCurrentSiteURL;
        }
        

        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        private siteItem getCurrentSiteInfo()
        {

            // siteItem currentSiteInfo;
            //
            // if (!mDomainURL2SiteInfo.TryGetValue(
            //         mCurrentSiteDomainURL, out currentSiteInfo)
            //    )
            // {
            //     //Fake values so the rest still works.
            //     currentSiteInfo.siteDomainURL = mCurrentSiteDomainURL;
            //     currentSiteInfo.questions = 2000;
            //     currentSiteInfo.posts = 4000;
            //     currentSiteInfo.order = 999;
            //     currentSiteInfo.unansweredQuestions = 50;
            // }
            // return currentSiteInfo;

            return getSiteInfo(mCurrentSiteDomainURL);
        }
        

        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        public int getPosts()
        {
            siteItem currentSiteInfo = getCurrentSiteInfo();
            return currentSiteInfo.posts;
        }


        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        public int getUnansweredQuestions()
        {
            siteItem currentSiteInfo = getCurrentSiteInfo();
            return currentSiteInfo.unansweredQuestions;
        } //getUnansweredQuestions()


        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        public List<string> getSiteURLs()
        {
            int currentItems = mDomainURL2SiteInfo.Count;
            List<string> toReturn = new List<string> (currentItems);

            //No sorting for now...
            Dictionary<string, siteItem>.Enumerator hashEnumerator2 =
              mDomainURL2SiteInfo.GetEnumerator();
            while (hashEnumerator2.MoveNext())
            {
                string curSiteDomainURL = hashEnumerator2.Current.Key;
                siteItem curSiteInformation = hashEnumerator2.Current.Value;

                toReturn.Add(curSiteDomainURL);
            } //Hash iteration.
            return toReturn;
        } //getSiteURLs()


        /***************************************************************************
        *                                                                          *
        *    (Exposes type siteItem...)                                            *
        *                                                                          *
        ****************************************************************************/
        public siteItem getSiteInfo(string aCurrentSiteURL)
        {
            siteItem currentSiteInfo;

            if (!mDomainURL2SiteInfo.TryGetValue(
                    aCurrentSiteURL, out currentSiteInfo)
               )
            {
                //Fake values so the rest still works.
                currentSiteInfo.siteDomainURL = mCurrentSiteDomainURL;
                currentSiteInfo.questions = 2000;
                currentSiteInfo.posts = 4000;
                currentSiteInfo.order = 999;
                currentSiteInfo.unansweredQuestions = 50;
            }
            return currentSiteInfo;
        }


    } //class Sites


} //OverflowHelper.core

